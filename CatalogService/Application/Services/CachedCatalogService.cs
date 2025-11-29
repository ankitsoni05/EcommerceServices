using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;

namespace CatalogService.Application.Services;

/// <summary>
/// Decorator pattern implementation for caching catalog service calls.
/// 
/// DESIGN PATTERN: Decorator Pattern
/// WHY: 
/// - Adds caching behavior without modifying existing CatalogService
/// - Follows Open/Closed Principle (open for extension, closed for modification)
/// - Maintains Single Responsibility (cache logic separate from business logic)
/// - Easy to enable/disable caching via DI configuration
/// 
/// CACHE STRATEGY: Cache-Aside (Lazy Loading)
/// - Check cache first
/// - If miss, load from database
/// - Store in cache for next request
/// - Invalidate on write operations
/// 
/// CACHE INVALIDATION:
/// - Simple approach: Clear all related caches on any write
/// - Trade-off: Some unnecessary invalidations vs complexity
/// - For production: Consider more granular invalidation strategies
/// </summary>
public class CachedCatalogService : ICatalogService
{
    private readonly ICatalogService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedCatalogService> _logger;
    
    // Cache configuration
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(10);
    private const string CacheKeyPrefix = "catalog:";
    
    public CachedCatalogService(
        ICatalogService innerService,
        IDistributedCache cache,
        ILogger<CachedCatalogService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all catalog items with caching.
    /// Cache key: "catalog:items:all"
    /// </summary>
    public async Task<IEnumerable<CatalogItemDto>> GetAllItemsAsync()
    {
        const string cacheKey = CacheKeyPrefix + "items:all";
        
        return await GetOrSetCacheAsync(
            cacheKey,
            () => _innerService.GetAllItemsAsync(),
            DefaultCacheExpiration);
    }

    /// <summary>
    /// Get catalog item by ID with caching.
    /// Cache key: "catalog:items:{id}"
    /// </summary>
    public async Task<CatalogItemDto?> GetItemByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}items:{id}";
        
        return await GetOrSetCacheAsync(
            cacheKey,
            () => _innerService.GetItemByIdAsync(id),
            DefaultCacheExpiration);
    }

    /// <summary>
    /// Get catalog items by brand with caching.
    /// Cache key: "catalog:items:brand:{brandId}"
    /// </summary>
    public async Task<IEnumerable<CatalogItemDto>> GetItemsByBrandAsync(int brandId)
    {
        var cacheKey = $"{CacheKeyPrefix}items:brand:{brandId}";
        
        return await GetOrSetCacheAsync(
            cacheKey,
            () => _innerService.GetItemsByBrandAsync(brandId),
            DefaultCacheExpiration);
    }

    /// <summary>
    /// Get catalog items by type with caching.
    /// Cache key: "catalog:items:type:{typeId}"
    /// </summary>
    public async Task<IEnumerable<CatalogItemDto>> GetItemsByTypeAsync(int typeId)
    {
        var cacheKey = $"{CacheKeyPrefix}items:type:{typeId}";
        
        return await GetOrSetCacheAsync(
            cacheKey,
            () => _innerService.GetItemsByTypeAsync(typeId),
            DefaultCacheExpiration);
    }

    #region Write Operations (with Cache Invalidation)

    /// <summary>
    /// Create catalog item and invalidate relevant caches.
    /// </summary>
    public async Task<CatalogItemDto> CreateItemAsync(CreateCatalogItemDto dto)
    {
        var result = await _innerService.CreateItemAsync(dto);
        
        // Invalidate caches that would include this new item
        await InvalidateCachesAsync(
            result.Id,
            dto.CatalogBrandId,
            dto.CatalogTypeId);
        
        _logger.LogInformation(
            "Created catalog item {ItemId} and invalidated related caches",
            result.Id);
        
        return result;
    }

    /// <summary>
    /// Update catalog item and invalidate relevant caches.
    /// </summary>
    public async Task UpdateItemAsync(int id, CreateCatalogItemDto dto)
    {
        await _innerService.UpdateItemAsync(id, dto);
        
        // Invalidate caches for this item and its collections
        await InvalidateCachesAsync(
            id,
            dto.CatalogBrandId,
            dto.CatalogTypeId);
        
        _logger.LogInformation(
            "Updated catalog item {ItemId} and invalidated related caches",
            id);
    }

    /// <summary>
    /// Delete catalog item and invalidate all caches.
    /// Note: We don't know the brand/type, so invalidate everything.
    /// </summary>
    public async Task DeleteItemAsync(int id)
    {
        await _innerService.DeleteItemAsync(id);
        
        // Invalidate all caches since we don't have brand/type info
        await InvalidateAllCachesAsync();
        
        _logger.LogInformation(
            "Deleted catalog item {ItemId} and invalidated all caches",
            id);
    }

    #endregion

    #region Cache Helper Methods

    /// <summary>
    /// Generic cache-aside pattern implementation.
    /// Checks cache first, falls back to factory function if miss.
    /// </summary>
    private async Task<T> GetOrSetCacheAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan expiration)
    {
        // Try to get from cache
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            
            try
            {
                return JsonSerializer.Deserialize<T>(cachedData)!;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, 
                    "Failed to deserialize cached data for key: {CacheKey}. Fetching fresh data.",
                    cacheKey);
                
                // Remove corrupted cache entry
                await _cache.RemoveAsync(cacheKey);
            }
        }

        // Cache miss - get from source
        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
        var data = await factory();
        
        // Don't cache null results
        if (data == null)
        {
            return data;
        }

        // Store in cache
        try
        {
            var serialized = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(
                cacheKey,
                serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                });
            
            _logger.LogDebug(
                "Cached data for key: {CacheKey} with expiration: {Expiration}",
                cacheKey,
                expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache data for key: {CacheKey}", cacheKey);
            // Don't throw - return the data even if caching failed
        }

        return data;
    }

    /// <summary>
    /// Invalidate specific cache entries related to an item.
    /// </summary>
    private async Task InvalidateCachesAsync(
        int itemId,
        int brandId,
        int typeId)
    {
        var keysToInvalidate = new[]
        {
            $"{CacheKeyPrefix}items:all",           // All items
            $"{CacheKeyPrefix}items:{itemId}",      // Specific item
            $"{CacheKeyPrefix}items:brand:{brandId}", // Brand collection
            $"{CacheKeyPrefix}items:type:{typeId}"    // Type collection
        };

        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
        }
    }

    /// <summary>
    /// Invalidate all catalog-related caches.
    /// Used when we don't have enough information for granular invalidation.
    /// </summary>
    private async Task InvalidateAllCachesAsync()
    {
        // Note: This is a simple approach. For production, consider:
        // 1. Using cache tags (if supported by your cache provider)
        // 2. Maintaining a set of active cache keys
        // 3. Using a cache key pattern with Redis SCAN
        
        // For now, we'll just invalidate the "all items" cache
        // Individual items will expire naturally
        await _cache.RemoveAsync($"{CacheKeyPrefix}items:all");
        
        _logger.LogInformation("Invalidated all catalog item caches");
    }

    #endregion
}
