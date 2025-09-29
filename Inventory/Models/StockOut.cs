namespace Inventory.Models;

public class StockOut : Entity
{
    public StockOut() : base(EntityType.StockOut)
    {
        TransactionItems = new List<TransactionItem>();
    }
    
    public string? CreatedBy { get; set; }
    
    public DateTimeOffset LatestModifiedAt { get; set; }
    
    public string? ModifiedBy { get; set; }
    
    public DateTimeOffset OutcomeAt { get; set; }
    
    public List<TransactionItem> TransactionItems { get; set; }
}