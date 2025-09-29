namespace Inventory.Models
{
    public class ItemTypeProperty
    {
        public int Id { get; set; }  

        public int ItemTypeId { get; set; }
        public int PropertyDefinitionId { get; set; }

        public ItemType ItemType { get; set; } = null!;
        public PropertyDefinition PropertyDefinition { get; set; } = null!;
    }
}
