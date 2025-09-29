using Inventory.Data;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers;

[ApiController]
[Route("api/v1/properties")]
public class PropertyDefinitionsController : ControllerBase
{
    private readonly InventoryContext _db;
    public PropertyDefinitionsController(InventoryContext db) => _db = db;

    // GET /api/v1/properties?q=name
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q)
    {
        var query = _db.Set<PropertyDefinition>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(p => EF.Functions.Like(p.Name, like));
        }

        var list = await query.OrderBy(p => p.Name).ToListAsync();
        return Ok(list);
    }

    // POST /api/v1/properties
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropertyDefinition dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name required." });

        dto.Description ??= string.Empty;

        _db.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { id = dto.Id }, new { id = dto.Id });
    }
}