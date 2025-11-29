using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Infrastructure.Repositories;

public class InMemoryCatalogRepository : ICatalogRepository
{
    private readonly List<CatalogItem> _items;
    private readonly List<CatalogBrand> _brands;
    private readonly List<CatalogType> _types;
    private int _nextId = 1;

    public InMemoryCatalogRepository()
    {
        _brands = new List<CatalogBrand>
        {
            new CatalogBrand { Id = 1, Name = "Samsung" },
            new CatalogBrand { Id = 2, Name = "Apple" },
            new CatalogBrand { Id = 3, Name = "Sony" }
        };

        _types = new List<CatalogType>
        {
            new CatalogType { Id = 1, Name = "Smartphone" },
            new CatalogType { Id = 2, Name = "Laptop" },
            new CatalogType { Id = 3, Name = "Headphones" }
        };

        _items = new List<CatalogItem>
        {
            new CatalogItem
            {
                Id = _nextId++,
                Name = "Samsung Galaxy S24",
                Description = "Latest flagship smartphone",
                Price = 999.99m,
                AvailableStock = 50,
                ImageUrl = "/images/galaxy-s24.jpg",
                CatalogBrandId = 1,
                CatalogBrand = _brands[0],
                CatalogTypeId = 1,
                CatalogType = _types[0]
            },
            new CatalogItem
            {
                Id = _nextId++,
                Name = "iPhone 15 Pro",
                Description = "Apple's premium smartphone",
                Price = 1199.99m,
                AvailableStock = 30,
                ImageUrl = "/images/iphone-15-pro.jpg",
                CatalogBrandId = 2,
                CatalogBrand = _brands[1],
                CatalogTypeId = 1,
                CatalogType = _types[0]
            },
            new CatalogItem
            {
                Id = _nextId++,
                Name = "Sony WH-1000XM5",
                Description = "Premium noise-canceling headphones",
                Price = 399.99m,
                AvailableStock = 100,
                ImageUrl = "/images/sony-wh-1000xm5.jpg",
                CatalogBrandId = 3,
                CatalogBrand = _brands[2],
                CatalogTypeId = 3,
                CatalogType = _types[2]
            }
        };
    }

    public Task<IEnumerable<CatalogItem>> GetAllItemsAsync()
    {
        return Task.FromResult<IEnumerable<CatalogItem>>(_items);
    }

    public Task<CatalogItem?> GetItemByIdAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<CatalogItem>> GetItemsByBrandAsync(int brandId)
    {
        var items = _items.Where(i => i.CatalogBrandId == brandId);
        return Task.FromResult<IEnumerable<CatalogItem>>(items);
    }

    public Task<IEnumerable<CatalogItem>> GetItemsByTypeAsync(int typeId)
    {
        var items = _items.Where(i => i.CatalogTypeId == typeId);
        return Task.FromResult<IEnumerable<CatalogItem>>(items);
    }

    public Task<CatalogItem> AddItemAsync(CatalogItem item)
    {
        item.Id = _nextId++;
        item.CatalogBrand = _brands.FirstOrDefault(b => b.Id == item.CatalogBrandId);
        item.CatalogType = _types.FirstOrDefault(t => t.Id == item.CatalogTypeId);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task UpdateItemAsync(CatalogItem item)
    {
        var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            existingItem.AvailableStock = item.AvailableStock;
            existingItem.ImageUrl = item.ImageUrl;
            existingItem.CatalogBrandId = item.CatalogBrandId;
            existingItem.CatalogBrand = _brands.FirstOrDefault(b => b.Id == item.CatalogBrandId);
            existingItem.CatalogTypeId = item.CatalogTypeId;
            existingItem.CatalogType = _types.FirstOrDefault(t => t.Id == item.CatalogTypeId);
        }
        return Task.CompletedTask;
    }

    public Task DeleteItemAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<CatalogBrand>> GetBrandsAsync()
    {
        return Task.FromResult<IEnumerable<CatalogBrand>>(_brands);
    }

    public Task<IEnumerable<CatalogType>> GetTypesAsync()
    {
        return Task.FromResult<IEnumerable<CatalogType>>(_types);
    }
}
