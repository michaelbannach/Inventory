namespace Inventory.Models;

public class ItemType : Entity
{
    public ItemType() : base(EntityType.ItemType)
    {
        
    }
    
    public int AreaId { get; set; }
    
    public List<ItemTypeProperty> ItemTypeProperties { get; set; } = [];  
}