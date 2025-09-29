using Inventory.Dtos;
using Inventory.Services;
using Inventory.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers;

[ApiController]
[Route("api/v1/stock-movements")]
public class StockMovementController : ControllerBase
{
    private readonly IInventoryService _svc;
    private readonly InventoryContext _db;

    public StockMovementController(IInventoryService svc, InventoryContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ---------------------------
    // Read-Models für Übersichten
    // ---------------------------
    public record MovementItemRow(int ItemId, int Amount, string? ItemName);

    public record StockInRow(
        int Id,
        string? OrderNumber,
        string? DeliveryNumber,
        DateTimeOffset IncomeAt,
        string? CreatedBy,
        List<MovementItemRow> Items
    );

    public record StockOutRow(
        int Id,
        DateTimeOffset OutcomeAt,
        string? Description,
        List<MovementItemRow> Items
    );

    // -------------
    // GET: Eingänge
    // -------------
    [HttpGet("in")]
    public async Task<ActionResult<List<StockInRow>>> ListIn()
    {
        var data = await _db.StockIns
            .AsNoTracking()
            .Include(s => s.Items).ThenInclude(t => t.Item)
            .OrderByDescending(s => s.IncomeAt)
            .Select(s => new StockInRow(
                s.Id,
                s.OrderNumber,
                s.DeliveryNumber,
                s.IncomeAt,
                s.CreatedBy,
                s.Items.Select(i => new MovementItemRow(
                    i.ItemId,
                    i.Amount,
                    i.Item != null ? i.Item.Name : null
                )).ToList()
            ))
            .ToListAsync();

        return Ok(data);
    }

    // ------------
    // GET: Ausgänge
    // ------------
    [HttpGet("out")]
    public async Task<ActionResult<List<StockOutRow>>> ListOut()
    {
        var data = await _db.StockOuts
            .AsNoTracking()
            .Include(s => s.TransactionItems).ThenInclude(t => t.Item)
            .OrderByDescending(s => s.OutcomeAt)
            .Select(s => new StockOutRow(
                s.Id,
                s.OutcomeAt,
                s.Description,
                s.TransactionItems.Select(i => new MovementItemRow(
                    i.ItemId,
                    i.Amount,
                    i.Item != null ? i.Item.Name : null
                )).ToList()
            ))
            .ToListAsync();

        return Ok(data);
    }

    // --------------
    // POST: Ausgang
    // --------------
    [HttpPost("out")]
    public async Task<IActionResult> CreateOut([FromBody] CreateStockOutDto dto)
    {
        if (dto is null || dto.Items is null || dto.Items.Count == 0)
            return BadRequest(new { error = "Items required." });

        if (dto.Items.Any(x => x.ItemId <= 0 || x.Amount <= 0))
            return BadRequest(new { error = "Each item needs a valid ItemId and Amount > 0." });

        try
        {
            var id = await _svc.CreateStockOutAsync(dto);
            return CreatedAtAction(nameof(CreateOut), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    // -------------
    // POST: Eingang
    // -------------
    [HttpPost("in")]
    public async Task<IActionResult> CreateIn([FromBody] CreateStockInDto dto)
    {
        if (dto is null || dto.Items is null || dto.Items.Count == 0)
            return BadRequest(new { error = "Items required." });

        // Für Wareneingang ist der Lagerort pro Zeile Pflicht
        if (dto.Items.Any(x => x.ItemId <= 0 || x.Amount <= 0 || x.LocationId <= 0))
            return BadRequest(new { error = "Each item needs ItemId > 0, Amount > 0 and LocationId > 0." });

        try
        {
            var id = await _svc.CreateStockInAsync(dto);
            return CreatedAtAction(nameof(CreateIn), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
