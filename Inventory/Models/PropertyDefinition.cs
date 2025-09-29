namespace Inventory.Models; 

public class PropertyDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    
    public List<ItemTypeProperty> ItemTypeProperties { get; set; } = new();
}