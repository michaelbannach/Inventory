using Inventory.Data;
using Inventory.Dtos;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Services;

public class InventoryService : IInventoryService
{
    private readonly InventoryContext _db;
    public InventoryService(InventoryContext db) => _db = db;

    public async Task<int> CreateStockInAsync(CreateStockInDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new ArgumentException("Items required");

        // Validierung: pro Zeile ItemId > 0, Amount > 0, LocationId > 0
        if (dto.Items.Any(p => p.ItemId <= 0 || p.Amount <= 0 || p.LocationId <= 0))
            throw new ArgumentException("Each item needs ItemId > 0, Amount > 0 and LocationId > 0.");

        var itemIds = dto.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await _db.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
        {
            var missing = string.Join(", ", itemIds.Except(items.Select(i => i.Id)));
            throw new InvalidOperationException($"Items not found: {missing}");
        }

        // (optional) Location-Existenz prüfen
        var locIds = dto.Items.Select(i => i.LocationId).Distinct().ToList();
        var foundLocIds = await _db.Locations
            .Where(l => locIds.Contains(l.Id))
            .Select(l => l.Id)
            .ToListAsync();
        var missingLocIds = locIds.Except(foundLocIds).ToList();
        if (missingLocIds.Count > 0)
            throw new InvalidOperationException($"Locations not found: {string.Join(", ", missingLocIds)}");
        var now = DateTimeOffset.UtcNow;

        // Bestand erhöhen
        foreach (var p in dto.Items)
        {
            var item = items.First(i => i.Id == p.ItemId);
            item.Amount += p.Amount;
            item.AmountLastChangedAt = now;
        }

        var stockIn = new StockIn
        {
            Name = "StockIn",
            Description = $"{dto.Items.Count} Position(en)",
            CreatedAt = now,
            OrderNumber = dto.OrderNumber ?? string.Empty,
            DeliveryNumber = dto.DeliveryNumber ?? string.Empty,
            IncomeAt = dto.IncomeAt,

            // WICHTIG: LocationId pro Position setzen
            Items = dto.Items.Select(x => new TransactionItem
            {
                ItemId = x.ItemId,
                Amount = x.Amount,
                LocationId = x.LocationId
            }).ToList()
        };

        _db.StockIns.Add(stockIn);
        await _db.SaveChangesAsync();
        return stockIn.Id;
    }

    public async Task<int> CreateStockOutAsync(CreateStockOutDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new ArgumentException("Items required");
        if (dto.Items.Any(p => p.ItemId <= 0 || p.Amount <= 0))
            throw new ArgumentException("Each item needs ItemId > 0 and Amount > 0.");

        var now = DateTimeOffset.UtcNow;

        // Items laden
        var itemIds = dto.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await _db.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();
        if (items.Count != itemIds.Count)
        {
            var missing = string.Join(", ", itemIds.Except(items.Select(i => i.Id)));
            throw new InvalidOperationException($"Items not found: {missing}");
        }

        // Für jeden Artikel: verfügbare Bestände pro Location (FIFO: ältester Eingang zuerst)
        async Task<List<(int LocationId, int Available)>> GetPerLocationBalancesAsync(int itemId)
        {
            var perLoc = await (
                    from t in _db.TransactionItems.AsNoTracking()
                    join l in _db.Locations.AsNoTracking() on t.LocationId equals l.Id
                    join si in _db.StockIns.AsNoTracking() on t.StockInId equals si.Id into siJoin
                    from si in siJoin.DefaultIfEmpty()
                    where t.ItemId == itemId && t.LocationId != null
                    group new { t, si } by t.LocationId
                    into g
                    select new
                    {
                        LocationId = g.Key!.Value,
                        In = g.Where(x => x.t.StockInId != null).Sum(x => (int?)x.t.Amount) ?? 0,
                        Out = g.Where(x => x.t.StockOutId != null).Sum(x => (int?)x.t.Amount) ?? 0,
                        FirstInAt = g.Where(x => x.si != null).Min(x => (DateTimeOffset?)x.si!.IncomeAt) ??
                                    DateTimeOffset.MinValue
                    }
                )
                .Where(x => (x.In - x.Out) > 0)
                .OrderBy(x => x.FirstInAt) // FIFO
                .ToListAsync();

            return perLoc.Select(x => (x.LocationId, x.In - x.Out)).ToList();
        }

        // Vorab: Gesamtverfügbarkeit prüfen
        foreach (var p in dto.Items)
        {
            var totalAvailable = (await GetPerLocationBalancesAsync(p.ItemId)).Sum(x => x.Available);
            if (totalAvailable < p.Amount)
            {
                var itemName = items.First(i => i.Id == p.ItemId).Name;
                throw new InvalidOperationException($"Zu wenig Bestand für {itemName}: {totalAvailable} < {p.Amount}");
            }
        }

        var stockOut = new StockOut
        {
            Name = "StockOut",
            Description = dto.Description ?? string.Empty,
            CreatedAt = now,
            OutcomeAt = dto.OutcomeAt,
            TransactionItems = new List<TransactionItem>()
        };

        // Verteilung je Position (FIFO über Locations)
        foreach (var p in dto.Items)
        {
            var remaining = p.Amount;
            var fifo = await GetPerLocationBalancesAsync(p.ItemId);

            foreach (var (locId, available) in fifo)
            {
                if (remaining <= 0) break;
                var take = Math.Min(available, remaining);

                stockOut.TransactionItems.Add(new TransactionItem
                {
                    ItemId = p.ItemId,
                    Amount = take,
                    LocationId = locId // ⬅️ hier wird der Abgang einem Raum/Fach zugeordnet
                });

                remaining -= take;
            }

            // Bestand am Artikel (gesamt) reduzieren
            var item = items.First(i => i.Id == p.ItemId);
            item.Amount -= p.Amount;
            item.AmountLastChangedAt = now;
        }

        _db.StockOuts.Add(stockOut);
        await _db.SaveChangesAsync();
        return stockOut.Id;
    }
}