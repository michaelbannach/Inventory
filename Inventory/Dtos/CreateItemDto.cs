namespace Inventory.Dtos;

public class CreateItemDto
{
    public string Name { get; set; } = null!;
    public int ItemTypeId { get; set; }
    public int Amount { get; set; }
    public string? Description { get; set; }
    public int? CriticalAmount { get; set; }
    public List<PropertyValueDto>? Properties { get; set; }
}