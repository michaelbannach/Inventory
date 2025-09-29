namespace Inventory.Models;

public class Item :Entity
{
    public Item() : base(EntityType.Item)
    {
        
    }

    public int ItemTypeId { get; set; }
    
    public int Amount { get; set; }
    
    public int? CriticalAmount { get; set; }

    public List<PropertyValue> Properties { get; set; } = [];

    public DateTimeOffset AmountLastChangedAt { get; set; }
}


