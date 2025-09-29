using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Models;
using Inventory.Dtos;

namespace Inventory.Controllers;

[Route("api/v1/items")]
[ApiController]
public class ItemController : ControllerBase
{
    private readonly InventoryContext _context;
    private readonly ILogger<ItemController> _logger;

    public ItemController(InventoryContext context, ILogger<ItemController> logger)
    {
        _context = context;
        _logger  = logger;
    }

    // Nur Response-DTO (wird nicht gebunden) → kann hier intern bleiben
    private sealed record ItemAtLocationDto(
        int Id,
        string Name,
        string? Description,
        int ItemTypeId,
        string? ItemTypeName,
        int Amount,
        int? CriticalAmount
    );

    // -----------------------------
    // GET /api/v1/items/by-location?room=R1[&rackNo=A][&bin=01]
    // Nettomenge (Eingänge - Ausgänge) für den konkreten Standort
    // -----------------------------
    [HttpGet("by-location")]
    public async Task<IActionResult> GetItemsByLocation(
        [FromQuery] string room,
        [FromQuery] string? rackNo,
        [FromQuery] string? bin)
    {
        if (string.IsNullOrWhiteSpace(room))
            return BadRequest("room required");

        // 1) passende Locations finden
        var locIdsQuery = _context.Locations.AsNoTracking().Where(l => l.Room == room);
        if (!string.IsNullOrWhiteSpace(rackNo)) locIdsQuery = locIdsQuery.Where(l => l.RackNo == rackNo);
        if (!string.IsNullOrWhiteSpace(bin))    locIdsQuery = locIdsQuery.Where(l => l.Bin == bin);

        var locIds = await locIdsQuery.Select(l => l.Id).ToListAsync();
        if (locIds.Count == 0) return Ok(Array.Empty<ItemAtLocationDto>());

        // 2) Netto-Bestand pro Item an diesen Locations
        var perItem = await _context.TransactionItems
            .AsNoTracking()
            .Where(t => t.LocationId != null && locIds.Contains(t.LocationId!.Value))
            .GroupBy(t => t.ItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                In  = g.Where(x => x.StockInId  != null).Sum(x => (int?)x.Amount) ?? 0,
                Out = g.Where(x => x.StockOutId != null).Sum(x => (int?)x.Amount) ?? 0
            })
            .ToListAsync();

        if (perItem.Count == 0) return Ok(Array.Empty<ItemAtLocationDto>());

        var itemIds     = perItem.Select(x => x.ItemId).ToList();
        var netByItemId = perItem.ToDictionary(x => x.ItemId, x => x.In - x.Out);

        // 3) Stammdaten inkl. ItemTypeName
        var items = await _context.Items
            .AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.Description,
                i.ItemTypeId,
                ItemTypeName = _context.ItemTypes
                    .Where(t => t.Id == i.ItemTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault(),
                i.CriticalAmount
            })
            .ToListAsync();

        // 4) nur positive Standortbestände
        var result = items
            .Select(i => new ItemAtLocationDto(
                i.Id,
                i.Name,
                i.Description,
                i.ItemTypeId,
                i.ItemTypeName,
                Amount: netByItemId[i.Id],
                CriticalAmount: i.CriticalAmount
            ))
            .Where(x => x.Amount > 0)
            .OrderBy(x => x.Name);

        return Ok(result);
    }

    // -----------------------------
    // GET /api/v1/items?typeId=&q=&room=
    // Standardliste mit optionalem Filter nach Typ, Suche und Raum
    // -----------------------------
    [HttpGet]
    public async Task<IActionResult> GetItems(
        [FromQuery] int? typeId,
        [FromQuery] string? q,
        [FromQuery] string? room)
    {
        IQueryable<Item> baseQuery = _context.Items.AsNoTracking();

        if (typeId is > 0)
            baseQuery = baseQuery.Where(i => i.ItemTypeId == typeId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            baseQuery = baseQuery.Where(i =>
                EF.Functions.Like(i.Name, like) ||
                EF.Functions.Like(i.Description ?? string.Empty, like));
        }

        if (!string.IsNullOrWhiteSpace(room))
        {
            baseQuery = baseQuery.Where(i =>
                _context.TransactionItems.Any(t =>
                    t.ItemId == i.Id &&
                    t.Location != null &&
                    t.Location.Room == room));
        }

        // Projektion inkl. Typ-Name & letztem In/Out
        var data = await baseQuery
            .Select(i => new
            {
                i.Id,
                i.Name,
                i.Description,
                i.ItemTypeId,
                ItemTypeName = _context.ItemTypes
                    .Where(t => t.Id == i.ItemTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault(),
                i.Amount,
                i.CriticalAmount,
                i.CreatedAt,
                i.AmountLastChangedAt,
                LastIn = _context.TransactionItems
                    .Where(t => t.ItemId == i.Id && t.StockInId != null)
                    .Max(t => (DateTimeOffset?)t.StockIn!.IncomeAt),
                LastOut = _context.TransactionItems
                    .Where(t => t.ItemId == i.Id && t.StockOutId != null)
                    .Max(t => (DateTimeOffset?)t.StockOut!.OutcomeAt),
            })
            .ToListAsync();

        var result = data.Select(x => new
        {
            x.Id,
            x.Name,
            x.Description,
            x.ItemTypeId,
            itemTypeName = x.ItemTypeName, // camelCase fürs JSON
            x.Amount,
            x.CriticalAmount,
            lastStockIn = x.LastIn,
            lastStockOut = x.LastOut,
            amountLastChangedAt =
                new DateTimeOffset?[] { x.AmountLastChangedAt, x.LastIn, x.LastOut }.Max()
                ?? (DateTimeOffset?)x.CreatedAt
        });

        return Ok(result);
    }

    // -----------------------------
    // GET /api/v1/items/{id}
    // -----------------------------
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetItemById(int id)
    {
        try
        {
            var item = await _context.Items
                .AsNoTracking()
                .Include(i => i.Properties)
                // .ThenInclude(p => p.PropertyDefinition) // optional, falls du Namen serverseitig brauchst
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item is null) return NotFound();
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving item");
        }
    }

    // -----------------------------
    // POST /api/v1/items
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name must not be empty.");
        if (dto.ItemTypeId <= 0)
            return BadRequest("ItemTypeId must be a positive integer.");
        if (dto.Amount < 0)
            return BadRequest("Amount must not be negative.");

        var entity = new Item
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            ItemTypeId = dto.ItemTypeId,
            Amount = dto.Amount,
            CriticalAmount = dto.CriticalAmount,
            AmountLastChangedAt = DateTimeOffset.UtcNow,
            Properties = new List<PropertyValue>()
        };

        if (dto.Properties is { Count: > 0 })
        {
            foreach (var p in dto.Properties)
            {
                entity.Properties.Add(new PropertyValue
                {
                    PropertyDefinitionId = p.PropertyDefinitionId,
                    Value = p.Value ?? string.Empty
                    // ItemId wird durch Relationship beim Save gesetzt
                });
            }
        }

        _context.Items.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created Item {Id}", entity.Id);
        return CreatedAtAction(nameof(GetItemById), new { id = entity.Id }, new { id = entity.Id });
    }

    // -----------------------------
    // PUT /api/v1/items/{id}
    // -----------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name must not be empty.");
        if (dto.ItemTypeId <= 0)
            return BadRequest("ItemTypeId must be a positive integer.");
        if (dto.Amount < 0)
            return BadRequest("Amount must not be negative.");

        var existing = await _context.Items
            .Include(i => i.Properties)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existing is null) return NotFound();

        var amountChanged = existing.Amount != dto.Amount;

        existing.Name = dto.Name.Trim();
        existing.Description = dto.Description?.Trim() ?? string.Empty;
        existing.ItemTypeId = dto.ItemTypeId;
        existing.Amount = dto.Amount;
        existing.CriticalAmount = dto.CriticalAmount;

        if (amountChanged)
            existing.AmountLastChangedAt = DateTimeOffset.UtcNow;

        // Properties ersetzen (einfach & eindeutig)
        existing.Properties.Clear();
        if (dto.Properties is { Count: > 0 })
        {
            foreach (var p in dto.Properties)
            {
                existing.Properties.Add(new PropertyValue
                {
                    ItemId = existing.Id,
                    PropertyDefinitionId = p.PropertyDefinitionId,
                    Value = p.Value ?? string.Empty
                });
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated Item {Id}", id);
        return NoContent();
    }

    // -----------------------------
    // PATCH /api/v1/items/{id}/properties
    // -----------------------------
    [HttpPatch("{id:int}/properties")]
    public async Task<IActionResult> UpdateItemProperties(int id, [FromBody] List<PropertyValueDto> properties)
    {
        if (properties is null)
            return BadRequest("Properties list must not be null.");

        try
        {
            var item = await _context.Items
                .Include(i => i.Properties)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item is null) return NotFound();

            item.Properties.Clear();
            foreach (var p in properties)
            {
                item.Properties.Add(new PropertyValue
                {
                    ItemId = item.Id,
                    PropertyDefinitionId = p.PropertyDefinitionId,
                    Value = p.Value ?? string.Empty
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated properties for Item {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating properties for Item {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating properties");
        }
    }

    // -----------------------------
    // DELETE /api/v1/items/{id}
    // -----------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item is null) return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted Item {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting item");
        }
    }
}

