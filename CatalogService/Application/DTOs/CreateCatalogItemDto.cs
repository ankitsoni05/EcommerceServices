namespace CatalogService.Application.DTOs;

public class CreateCatalogItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableStock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int CatalogBrandId { get; set; }
    public int CatalogTypeId { get; set; }
}
