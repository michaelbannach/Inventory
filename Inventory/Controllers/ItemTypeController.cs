using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Inventory.Data;
using Inventory.Models;

namespace IvuInventar.Controllers;

[Route("api/v1/itemtypes")]
[ApiController]
public sealed class ItemTypeController : ControllerBase
{
    private readonly InventoryContext _context;
    private readonly ILogger<ItemTypeController> _logger;

    public ItemTypeController(InventoryContext context, ILogger<ItemTypeController> logger)
    {
        _context = context;
        _logger  = logger;
    }


    public record ItemTypeGridDto(
        int Id,
        string Name,
        string? Description,
        List<string> Properties,
        int More
    );

// GET /api/v1/itemtypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemTypeGridDto>>> GetItemTypes()
    {
        // alles in SQL berechnen: die ersten 3 Namen + Gesamtanzahl
        var data = await _context.ItemTypes
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                First3 = t.ItemTypeProperties
                    .OrderBy(tp => tp.Id)
                    .Select(tp => tp.PropertyDefinition.Name)
                    .Take(3)
                    .ToList(),
                Total = t.ItemTypeProperties.Count()
            })
            .ToListAsync();

        var result = data.Select(x => new ItemTypeGridDto(
            x.Id,
            x.Name,
            x.Description,
            x.First3,
            x.Total > 3 ? x.Total - 3 : 0
        ));

        return Ok(result);
    }
    
    // GET /api/v1/itemtypes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetItemTypeById(int id)
    {
        try
        {
            var type = await _context.ItemTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type is null)
            {
                _logger.LogInformation("ItemType {Id} not found", id);
                return NotFound();
            }

            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving item type");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateItemType([FromBody] ItemType itemType)
    {
        if (string.IsNullOrWhiteSpace(itemType.Name))
            return BadRequest("Name must not be empty.");
        if (itemType.AreaId <= 0)
            return BadRequest("AreaId must be a positive integer.");
    
        itemType.Description ??= string.Empty;
        itemType.ItemTypeProperties ??= new List<ItemTypeProperty>(); // sofern verwendet
    
        try
        {
            _context.ItemTypes.Add(itemType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemTypeById), new { id = itemType.Id }, new { id = itemType.Id });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating item type");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating item type");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItemType(int id, [FromBody] ItemType itemType)
    {
        if (string.IsNullOrWhiteSpace(itemType.Name))
            return BadRequest("Name must not be empty.");
        if (itemType.AreaId <= 0)
            return BadRequest("AreaId must be a positive integer.");
        if (id != itemType.Id)
            return BadRequest("Id mismatch.");

        try
        {
            var existing = await _context.ItemTypes
                .Include(t => t.ItemTypeProperties)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existing is null) return NotFound();

            existing.Name = itemType.Name.Trim();
            existing.Description = itemType.Description?.Trim() ?? string.Empty;
            existing.AreaId = itemType.AreaId;
            existing.ItemTypeProperties = itemType.ItemTypeProperties ?? new List<ItemTypeProperty>();

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating item type");
        }
    }


    // PATCH /api/v1/itemtypes/{id}/properties
    [HttpPatch("{id:int}/properties")]
    public async Task<IActionResult> UpdateItemTypeProperties(int id, [FromBody] List<int> properties)
    {
        if (properties is null) return BadRequest("Properties list must not be null.");

        try
        {
            var existing = await _context.ItemTypes
                .Include(t => t.ItemTypeProperties)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existing is null) return NotFound();

            // Alte Werte entfernen und neue ItemTypeProperty-Objekte zuweisen
            existing.ItemTypeProperties = properties
                .Distinct()
                .Select(propertyId => new ItemTypeProperty
                {
                    ItemTypeId = id,
                    PropertyDefinitionId = propertyId
                })
                .ToList();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated properties for ItemType {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating properties for ItemType {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating properties");
        }
    }


    // DELETE /api/v1/itemtypes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItemType(int id)
    {
        try
        {
            var type = await _context.ItemTypes.FirstOrDefaultAsync(t => t.Id == id);
            if (type is null) return NotFound();

            _context.ItemTypes.Remove(type);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted ItemType {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item type {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting item type");
        }
    }
}
