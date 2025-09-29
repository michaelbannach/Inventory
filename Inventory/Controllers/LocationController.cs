using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Models;
using Inventory.Dtos;

namespace Inventory.Controllers;

[Route("api/v1/locations")]
[ApiController]
public sealed class LocationController : ControllerBase
{
    private readonly InventoryContext _db;
    private readonly ILogger<LocationController> _logger;

    public LocationController(InventoryContext db, ILogger<LocationController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/v1/locations[?q=...]
    [HttpGet]
    public async Task<ActionResult> GetLocations([FromQuery] string? q)
    {
        try
        {
            var query = _db.Locations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var like = $"%{q}%";
                query = query.Where(l =>
                    EF.Functions.Like(l.Room, like)   ||
                    EF.Functions.Like(l.RackNo, like) ||
                    EF.Functions.Like(l.Bin, like));
            }

            var list = await query
                .OrderBy(l => l.Room).ThenBy(l => l.RackNo).ThenBy(l => l.Bin)
                .Select(l => new LocationDto(l.Id, l.Room, l.RackNo, l.Bin, l.Label))
                .ToListAsync();

            _logger.LogInformation("Found {Count} locations (q={Query})", list.Count, q);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving locations");
        }
    }

    // GET /api/v1/locations/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetLocationById(int id)
    {
        try
        {
            var loc = await _db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null) return NotFound();

            var dto = new LocationDto(loc.Id, loc.Room, loc.RackNo, loc.Bin, loc.Label);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving location");
        }
    }

    // POST /api/v1/locations
    [HttpPost]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Room) ||
            string.IsNullOrWhiteSpace(dto.RackNo) ||
            string.IsNullOrWhiteSpace(dto.Bin))
            return BadRequest(new { error = "Room, RackNo und Bin sind erforderlich." });

        try
        {
            var entity = new Location
            {
                Room = dto.Room.Trim(),
                RackNo = dto.RackNo.Trim(),
                Bin = dto.Bin.Trim(),
                // optional: Basisklasse weiterhin füllen, falls anderswo verwendet:
                Name = $"{dto.Room}-{dto.RackNo}-{dto.Bin}",
                Description = string.Empty
            };

            _db.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Created Location {Id}", entity.Id);
            return CreatedAtAction(nameof(GetLocationById), new { id = entity.Id }, new { id = entity.Id });
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Locations_Room_RackNo_Bin") == true)
        {
            return Conflict(new { error = "Die Kombination aus Room/RackNo/Bin existiert bereits." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating location");
        }
    }

    // PUT /api/v1/locations/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Room) ||
            string.IsNullOrWhiteSpace(dto.RackNo) ||
            string.IsNullOrWhiteSpace(dto.Bin))
            return BadRequest(new { error = "Room, RackNo und Bin sind erforderlich." });

        try
        {
            var existing = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (existing is null) return NotFound();

            existing.Room = dto.Room.Trim();
            existing.RackNo = dto.RackNo.Trim();
            existing.Bin = dto.Bin.Trim();
            // optional: Basisklasse mitziehen
            existing.Name = $"{existing.Room}-{existing.RackNo}-{existing.Bin}";
            existing.Description = string.Empty;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Updated Location {Id}", id);
            return NoContent();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Locations_Room_RackNo_Bin") == true)
        {
            return Conflict(new { error = "Die Kombination aus Room/RackNo/Bin existiert bereits." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating location");
        }
    }

    // PATCH /api/v1/locations/{id}
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchLocation(int id, [FromBody] Dictionary<string, string?> changes)
    {
        if (changes is null || changes.Count == 0)
            return BadRequest(new { error = "No changes provided." });

        try
        {
            var existing = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (existing is null) return NotFound();

            if (changes.TryGetValue("room", out var room) && !string.IsNullOrWhiteSpace(room))
                existing.Room = room.Trim();

            if (changes.TryGetValue("rackNo", out var rack) && !string.IsNullOrWhiteSpace(rack))
                existing.RackNo = rack.Trim();

            if (changes.TryGetValue("bin", out var bin) && !string.IsNullOrWhiteSpace(bin))
                existing.Bin = bin.Trim();

            // optional: Label/Name synchron halten
            existing.Name = $"{existing.Room}-{existing.RackNo}-{existing.Bin}";
            existing.Description = string.Empty;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Patched Location {Id}", id);
            return NoContent();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Locations_Room_RackNo_Bin") == true)
        {
            return Conflict(new { error = "Die Kombination aus Room/RackNo/Bin existiert bereits." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching location {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error patching location");
        }
    }

    // DELETE /api/v1/locations/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        try
        {
            var loc = await _db.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc is null) return NotFound();

            _db.Locations.Remove(loc);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted Location {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting location");
        }
    }
}
