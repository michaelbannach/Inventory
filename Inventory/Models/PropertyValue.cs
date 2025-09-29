// PropertyValue.cs
namespace Inventory.Models;

public class PropertyValue
{
    public int Id { get; set; }

    
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

  
    public int PropertyDefinitionId { get; set; }
    public PropertyDefinition PropertyDefinition { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}