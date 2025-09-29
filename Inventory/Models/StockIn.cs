namespace Inventory.Models;

public class StockIn : Entity
{
    public StockIn() : base(EntityType.StockIn)
    {
        Items = new List<TransactionItem>();
    }

    public string? CreatedBy { get; set; }
    public string? OrderNumber { get; set; }
    public string? DeliveryNumber { get; set; }
    public DateTimeOffset LatestModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTimeOffset IncomeAt { get; set; }

    public List<TransactionItem> Items { get; set; }
    public int? LocationId { get; set; }
}
