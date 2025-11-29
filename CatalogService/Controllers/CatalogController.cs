using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetAllItems()
    {
        try
        {
            var items = await _catalogService.GetAllItemsAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all catalog items");
            return StatusCode(500, "An error occurred while retrieving catalog items");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CatalogItemDto>> GetItemById(int id)
    {
        try
        {
            var item = await _catalogService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound($"Catalog item with id {id} not found");
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving catalog item {ItemId}", id);
            return StatusCode(500, "An error occurred while retrieving the catalog item");
        }
    }

    [HttpGet("brand/{brandId}")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetItemsByBrand(int brandId)
    {
        try
        {
            var items = await _catalogService.GetItemsByBrandAsync(brandId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for brand {BrandId}", brandId);
            return StatusCode(500, "An error occurred while retrieving catalog items");
        }
    }

    [HttpGet("type/{typeId}")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetItemsByType(int typeId)
    {
        try
        {
            var items = await _catalogService.GetItemsByTypeAsync(typeId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for type {TypeId}", typeId);
            return StatusCode(500, "An error occurred while retrieving catalog items");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CatalogItemDto>> CreateItem([FromBody] CreateCatalogItemDto dto)
    {
        try
        {
            var item = await _catalogService.CreateItemAsync(dto);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating catalog item");
            return StatusCode(500, "An error occurred while creating the catalog item");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateCatalogItemDto dto)
    {
        try
        {
            await _catalogService.UpdateItemAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating catalog item {ItemId}", id);
            return StatusCode(500, "An error occurred while updating the catalog item");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            await _catalogService.DeleteItemAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting catalog item {ItemId}", id);
            return StatusCode(500, "An error occurred while deleting the catalog item");
        }
    }
}
