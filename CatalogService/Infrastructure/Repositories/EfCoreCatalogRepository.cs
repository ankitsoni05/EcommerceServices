using Microsoft.EntityFrameworkCore;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;

namespace CatalogService.Infrastructure.Repositories;

/// <summary>
/// EfCoreCatalogRepository: EF Core implementation of ICatalogRepository.
/// 
/// REPOSITORY PATTERN IMPLEMENTATION:
/// - Abstracts data access logic from business logic
/// - Implements interface defined in Domain layer
/// - Lives in Infrastructure layer (technical detail)
/// - Can be swapped with other implementations (InMemory, Dapper, etc.)
/// 
/// WHY THIS APPROACH:
/// 1. Testability: Application layer can use mock repositories
/// 2. Flexibility: Change database technology without touching business logic
/// 3. Separation: SQL queries stay in Infrastructure, not in Application
/// 4. Single Responsibility: Only responsible for data access
/// 
/// WHAT BELONGS HERE:
/// ? Database queries (LINQ, SQL)
/// ? EF Core operations (Include, tracking, etc.)
/// ? Data access optimization
/// 
/// WHAT DOESN'T BELONG HERE:
/// ? Business logic (belongs in Domain/Application)
/// ? DTOs (belongs in Application)
/// ? HTTP concerns (belongs in Presentation)
/// </summary>
public class EfCoreCatalogRepository : ICatalogRepository
{
    private readonly CatalogDbContext _context;

    public EfCoreCatalogRepository(CatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all catalog items with related entities (Brand, Type).
    /// Uses Include() to perform eager loading and avoid N+1 queries.
    /// </summary>
    public async Task<IEnumerable<CatalogItem>> GetAllItemsAsync()
    {
        return await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .AsNoTracking() // Read-only query, better performance
            .ToListAsync();
    }

    /// <summary>
    /// Get a single catalog item by ID with related entities.
    /// Returns null if not found.
    /// </summary>
    public async Task<CatalogItem?> GetItemByIdAsync(int id)
    {
        return await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>
    /// Get all items for a specific brand.
    /// </summary>
    public async Task<IEnumerable<CatalogItem>> GetItemsByBrandAsync(int brandId)
    {
        return await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .Where(i => i.CatalogBrandId == brandId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Get all items for a specific type/category.
    /// </summary>
    public async Task<IEnumerable<CatalogItem>> GetItemsByTypeAsync(int typeId)
    {
        return await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .Where(i => i.CatalogTypeId == typeId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Add a new catalog item to the database.
    /// EF Core will generate the ID automatically.
    /// </summary>
    public async Task<CatalogItem> AddItemAsync(CatalogItem item)
    {
        // Add entity to context (in-memory tracking)
        _context.CatalogItems.Add(item);
        
        // Save changes to database (executes INSERT)
        await _context.SaveChangesAsync();
        
        // Load related entities for the returned item
        await _context.Entry(item)
            .Reference(i => i.CatalogBrand)
            .LoadAsync();
        await _context.Entry(item)
            .Reference(i => i.CatalogType)
            .LoadAsync();
        
        return item;
    }

    /// <summary>
    /// Update an existing catalog item.
    /// Assumes the item entity is already tracked or will be attached.
    /// </summary>
    public async Task UpdateItemAsync(CatalogItem item)
    {
        // Attach and mark as modified if not already tracked
        var entry = _context.Entry(item);
        if (entry.State == EntityState.Detached)
        {
            _context.CatalogItems.Attach(item);
            entry.State = EntityState.Modified;
        }
        else
        {
            _context.CatalogItems.Update(item);
        }
        
        // Save changes to database (executes UPDATE)
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete a catalog item by ID.
    /// </summary>
    public async Task DeleteItemAsync(int id)
    {
        // Find the entity
        var item = await _context.CatalogItems.FindAsync(id);
        
        if (item != null)
        {
            // Remove from context
            _context.CatalogItems.Remove(item);
            
            // Save changes to database (executes DELETE)
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Get all catalog brands.
    /// </summary>
    public async Task<IEnumerable<CatalogBrand>> GetBrandsAsync()
    {
        return await _context.CatalogBrands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get all catalog types.
    /// </summary>
    public async Task<IEnumerable<CatalogType>> GetTypesAsync()
    {
        return await _context.CatalogTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
