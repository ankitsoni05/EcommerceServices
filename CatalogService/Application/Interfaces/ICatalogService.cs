using CatalogService.Application.DTOs;

namespace CatalogService.Application.Interfaces;

public interface ICatalogService
{
    Task<IEnumerable<CatalogItemDto>> GetAllItemsAsync();
    Task<CatalogItemDto?> GetItemByIdAsync(int id);
    Task<IEnumerable<CatalogItemDto>> GetItemsByBrandAsync(int brandId);
    Task<IEnumerable<CatalogItemDto>> GetItemsByTypeAsync(int typeId);
    Task<CatalogItemDto> CreateItemAsync(CreateCatalogItemDto item);
    Task UpdateItemAsync(int id, CreateCatalogItemDto item);
    Task DeleteItemAsync(int id);
}
