namespace Inventory.Dtos
{
    public sealed class MovementItemInDto
    {
        public int ItemId { get; set; }
        public int Amount { get; set; }
        public int LocationId { get; set; }   
    }

    public sealed class MovementItemOutDto
    {
        public int ItemId { get; set; }
        public int Amount { get; set; }       
    }

    public sealed class CreateStockInDto
    {
        public string? OrderNumber { get; set; }
        public string? DeliveryNumber { get; set; }
        public DateTimeOffset IncomeAt { get; set; }
        public List<MovementItemInDto> Items { get; set; } = new();
    }

    public sealed class CreateStockOutDto
    {
        public DateTimeOffset OutcomeAt { get; set; }
        public string? Description { get; set; }
        public List<MovementItemOutDto> Items { get; set; } = new();
    }
}