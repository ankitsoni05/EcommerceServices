using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Interfaces;

public interface ICatalogRepository
{
    Task<IEnumerable<CatalogItem>> GetAllItemsAsync();
    Task<CatalogItem?> GetItemByIdAsync(int id);
    Task<IEnumerable<CatalogItem>> GetItemsByBrandAsync(int brandId);
    Task<IEnumerable<CatalogItem>> GetItemsByTypeAsync(int typeId);
    Task<CatalogItem> AddItemAsync(CatalogItem item);
    Task UpdateItemAsync(CatalogItem item);
    Task DeleteItemAsync(int id);
    Task<IEnumerable<CatalogBrand>> GetBrandsAsync();
    Task<IEnumerable<CatalogType>> GetTypesAsync();
}
