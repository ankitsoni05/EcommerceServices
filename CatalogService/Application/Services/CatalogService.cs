using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repository;

    public CatalogService(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CatalogItemDto>> GetAllItemsAsync()
    {
        var items = await _repository.GetAllItemsAsync();
        return items.Select(MapToDto);
    }

    public async Task<CatalogItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _repository.GetItemByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<IEnumerable<CatalogItemDto>> GetItemsByBrandAsync(int brandId)
    {
        var items = await _repository.GetItemsByBrandAsync(brandId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<CatalogItemDto>> GetItemsByTypeAsync(int typeId)
    {
        var items = await _repository.GetItemsByTypeAsync(typeId);
        return items.Select(MapToDto);
    }

    public async Task<CatalogItemDto> CreateItemAsync(CreateCatalogItemDto dto)
    {
        var item = new CatalogItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            AvailableStock = dto.AvailableStock,
            ImageUrl = dto.ImageUrl,
            CatalogBrandId = dto.CatalogBrandId,
            CatalogTypeId = dto.CatalogTypeId
        };

        var createdItem = await _repository.AddItemAsync(item);
        return MapToDto(createdItem);
    }

    public async Task UpdateItemAsync(int id, CreateCatalogItemDto dto)
    {
        var existingItem = await _repository.GetItemByIdAsync(id);
        if (existingItem == null)
        {
            throw new KeyNotFoundException($"Catalog item with id {id} not found");
        }

        existingItem.Name = dto.Name;
        existingItem.Description = dto.Description;
        existingItem.Price = dto.Price;
        existingItem.AvailableStock = dto.AvailableStock;
        existingItem.ImageUrl = dto.ImageUrl;
        existingItem.CatalogBrandId = dto.CatalogBrandId;
        existingItem.CatalogTypeId = dto.CatalogTypeId;

        await _repository.UpdateItemAsync(existingItem);
    }

    public async Task DeleteItemAsync(int id)
    {
        await _repository.DeleteItemAsync(id);
    }

    private static CatalogItemDto MapToDto(CatalogItem item)
    {
        return new CatalogItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            AvailableStock = item.AvailableStock,
            ImageUrl = item.ImageUrl,
            BrandName = item.CatalogBrand?.Name ?? string.Empty,
            TypeName = item.CatalogType?.Name ?? string.Empty
        };
    }
}
