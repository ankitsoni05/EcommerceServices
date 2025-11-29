# Redis Caching Implementation Guide

## ?? Overview

This document explains the Redis caching implementation for the Catalog Service using the **Decorator Pattern**.

---

## ?? Architecture Decision

### **Pattern: Service-Level Decorator with Cache-Aside Strategy**

```
????????????????
?  Controller  ?
????????????????
       ?
       ?
???????????????????????????
?  CachedCatalogService   ? ??? Decorator (Caching Layer)
?  (Decorator)            ?
???????????????????????????
       ?                  ?
       ?                  ? Redis Cache
       ?                  ?
????????????????????????  ????????????????
?  CatalogService      ?  ?    Redis     ?
?  (Core Business)     ?  ?   (Cache)    ?
????????????????????????  ????????????????
       ?
       ?
????????????????????????
?  Repository          ?
????????????????????????
       ?
       ?
????????????????????????
?  PostgreSQL Database ?
????????????????????????
```

---

## ? Why This Pattern?

### **Advantages:**

1. **? Clean Architecture Compliant**
   - Caching logic stays in Application layer
   - Infrastructure concerns (Redis) properly isolated
   - Business logic remains pure

2. **? SOLID Principles**
   - **Single Responsibility**: Each class has one reason to change
   - **Open/Closed**: Extended with caching without modifying original code
   - **Dependency Inversion**: Depends on abstractions (ICatalogService)

3. **? Testability**
   ```csharp
   // Test without cache
   var service = new CatalogService(mockRepository);
   
   // Test with cache
   var cachedService = new CachedCatalogService(service, mockCache, mockLogger);
   ```

4. **? Flexibility**
   - Easy to enable/disable caching (DI configuration)
   - Can swap cache implementations
   - Multiple decorators can be chained

5. **? Cache DTOs (Not Entities)**
   - Smaller cache size
   - No EF Core navigation property issues
   - Ready-to-serialize objects

---

## ?? Implementation Details

### **1. Cache Keys Structure**

```
catalog:items:all                    ? All items
catalog:items:{id}                   ? Specific item
catalog:items:brand:{brandId}        ? Items by brand
catalog:items:type:{typeId}          ? Items by type
```

**Benefits:**
- Hierarchical structure
- Easy to identify and invalidate
- Namespace prefix prevents collisions

---

### **2. Cache Expiration**

**Strategy:** Time-based expiration (TTL)

```csharp
AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
```

**Why 10 minutes?**
- ? Balance between freshness and cache hit rate
- ? Catalog data doesn't change frequently
- ? Acceptable staleness for e-commerce

**Alternatives:**
```csharp
// Short TTL for frequently changing data
TimeSpan.FromMinutes(5)

// Longer TTL for static data
TimeSpan.FromHours(1)

// Sliding expiration (resets on access)
SlidingExpiration = TimeSpan.FromMinutes(10)
```

---

### **3. Cache Invalidation Strategy**

#### **Approach: Granular Invalidation**

**On Create/Update:**
```csharp
await InvalidateCachesAsync(itemId, brandId, typeId);

// Invalidates:
// - catalog:items:all
// - catalog:items:{id}
// - catalog:items:brand:{brandId}
// - catalog:items:type:{typeId}
```

**On Delete:**
```csharp
await InvalidateAllCachesAsync();

// Invalidates:
// - catalog:items:all
// (Individual items expire naturally)
```

---

### **4. Error Handling**

**Cache Failures Don't Break the Application:**

```csharp
try
{
    var serialized = JsonSerializer.Serialize(data);
    await _cache.SetStringAsync(cacheKey, serialized, options);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to cache data for key: {CacheKey}", cacheKey);
    // Don't throw - return data even if caching failed
}
```

**Corrupted Cache Data:**
```csharp
try
{
    return JsonSerializer.Deserialize<T>(cachedData)!;
}
catch (JsonException ex)
{
    _logger.LogWarning(ex, "Failed to deserialize cached data");
    await _cache.RemoveAsync(cacheKey); // Remove bad data
    // Fall through to fetch fresh data
}
```

---

## ?? Performance Impact

### **Expected Improvements:**

| Metric | Without Cache | With Cache | Improvement |
|--------|--------------|------------|-------------|
| Response Time (GetAll) | ~150ms | ~5ms | **30x faster** |
| Database Load | 100% | ~10% | **90% reduction** |
| Scalability | Limited by DB | High | **10x more requests** |

### **Cache Hit Rate Expectations:**

- **GetAllItems**: 80-90% (frequently accessed)
- **GetItemById**: 60-70% (browsing patterns)
- **GetItemsByBrand**: 70-80% (popular brands)

---

## ?? Usage Examples

### **Scenario 1: First Request (Cache Miss)**

```
Client ? Controller ? CachedService
                         ? (cache miss)
                      InnerService ? Repository ? Database
                         ?
                      Redis (store)
                         ?
                      Controller ? Response
```

**Logs:**
```
[15:30:42 DBG] Cache miss for key: catalog:items:all
[15:30:42 INF] Executing DbCommand (23ms)...
[15:30:42 DBG] Cached data for key: catalog:items:all with expiration: 00:10:00
```

---

### **Scenario 2: Subsequent Request (Cache Hit)**

```
Client ? Controller ? CachedService
                         ? (cache hit)
                      Redis ? Data
                         ?
                      Controller ? Response
```

**Logs:**
```
[15:31:15 DBG] Cache hit for key: catalog:items:all
```

**No database query executed!**

---

### **Scenario 3: Create New Item (Invalidation)**

```
Client ? Controller ? CachedService ? InnerService ? Repository ? Database
                         ?
                   Invalidate Caches
                         ?
                   Redis (remove keys)
```

**Logs:**
```
[15:35:20 INF] Created catalog item 42 and invalidated related caches
[15:35:20 DBG] Invalidated cache key: catalog:items:all
[15:35:20 DBG] Invalidated cache key: catalog:items:42
[15:35:20 DBG] Invalidated cache key: catalog:items:brand:5
[15:35:20 DBG] Invalidated cache key: catalog:items:type:3
```

---

## ?? Configuration

### **Development (appsettings.Development.json)**
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### **Production (appsettings.json)**
```json
{
  "ConnectionStrings": {
    "Redis": "redis-cluster:6379,password=YourSecurePassword,ssl=true,abortConnect=false"
  }
}
```

### **Docker Compose**
```yaml
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes

volumes:
  redis-data:
```

---

## ?? Testing

### **Unit Tests (Without Cache)**
```csharp
[Fact]
public async Task GetAllItemsAsync_ShouldReturnItems()
{
    // Arrange
    var mockRepo = new Mock<ICatalogRepository>();
    mockRepo.Setup(r => r.GetAllItemsAsync())
        .ReturnsAsync(new List<CatalogItem> { /* ... */ });
    
    var service = new CatalogService(mockRepo.Object);
    
    // Act
    var result = await service.GetAllItemsAsync();
    
    // Assert
    Assert.NotEmpty(result);
}
```

### **Integration Tests (With Cache)**
```csharp
[Fact]
public async Task CachedService_ShouldReturnCachedData()
{
    // Arrange
    var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
    var innerService = new Mock<ICatalogService>();
    innerService.Setup(s => s.GetAllItemsAsync())
        .ReturnsAsync(new List<CatalogItemDto> { /* ... */ });
    
    var cachedService = new CachedCatalogService(
        innerService.Object, 
        cache, 
        Mock.Of<ILogger<CachedCatalogService>>());
    
    // Act
    var result1 = await cachedService.GetAllItemsAsync();
    var result2 = await cachedService.GetAllItemsAsync();
    
    // Assert
    innerService.Verify(s => s.GetAllItemsAsync(), Times.Once); // Called only once!
}
```

---

## ?? Monitoring & Observability

### **Key Metrics to Track:**

1. **Cache Hit Rate**
   ```csharp
   CacheHits / (CacheHits + CacheMisses) * 100
   ```

2. **Average Response Time**
   - With cache: ~5ms
   - Without cache: ~150ms

3. **Cache Size**
   ```bash
   redis-cli INFO memory
   ```

4. **Eviction Rate**
   ```bash
   redis-cli INFO stats | grep evicted_keys
   ```

### **Application Insights / Prometheus Metrics:**
```csharp
// Add to CachedCatalogService
_metrics.RecordCacheHit(cacheKey);
_metrics.RecordCacheMiss(cacheKey);
_metrics.RecordCacheLatency(duration);
```

---

## ?? Troubleshooting

### **Problem: Cache Never Hits**

**Check:**
1. Redis connection string correct?
2. Redis service running?
3. Cache keys consistent?

```bash
# Test Redis connection
redis-cli ping
# Should return: PONG

# Check cache keys
redis-cli KEYS "catalog:*"
```

---

### **Problem: Stale Data**

**Solutions:**
1. Reduce TTL
2. Implement cache invalidation on updates
3. Use sliding expiration

---

### **Problem: Memory Pressure**

**Solutions:**
1. Set max memory limit in Redis
2. Configure eviction policy
3. Reduce TTL
4. Use cache selectively (only hot data)

```bash
# Redis configuration
maxmemory 2gb
maxmemory-policy allkeys-lru
```

---

## ?? Alternative Patterns Comparison

| Pattern | Pros | Cons | Use Case |
|---------|------|------|----------|
| **Decorator (Service)** ? | Clean, testable, SOLID | Extra abstraction | **Recommended for this app** |
| Decorator (Repository) | Caches entities | Navigation properties issue | Large entities |
| Cache-Aside (Inline) | Simple | Mixed responsibilities | Small apps |
| Output Cache | Minimal code | HTTP-only | Static content APIs |

---

## ?? References

- [Microsoft Docs: Distributed Caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Redis Best Practices](https://redis.io/docs/management/optimization/)
- [Decorator Pattern](https://refactoring.guru/design-patterns/decorator)

---

## ? Checklist

- [x] Redis package installed
- [x] Connection string configured
- [x] Decorator service implemented
- [x] DI registration updated
- [x] Cache invalidation strategy defined
- [x] Error handling added
- [x] Logging implemented
- [ ] Unit tests written
- [ ] Integration tests written
- [ ] Performance tests conducted
- [ ] Monitoring configured

---

## ?? Key Takeaways

1. **Decorator pattern** provides clean separation of concerns
2. **Cache DTOs**, not entities, for better serialization
3. **Invalidate proactively** on write operations
4. **Handle failures gracefully** - cache is enhancement, not requirement
5. **Monitor cache hit rates** to optimize TTL and invalidation
