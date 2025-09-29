namespace Inventory.Models;

public class TransactionItem {
   
    public int Id { get; set; }
   
    public int ItemId { get; set; }
    
    public Item Item { get; set; } = null!;
    
    public int Amount { get; set; }
    
    public int? StockInId { get; set; }
    
    public StockIn? StockIn { get; set; }
    
    public int? StockOutId { get; set; }
    
    public StockOut? StockOut { get; set; }
    
    public int? LocationId { get; set; }   
    
    public Location? Location { get; set; }
}