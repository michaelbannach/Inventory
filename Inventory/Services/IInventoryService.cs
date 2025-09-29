using Inventory.Dtos;

namespace Inventory.Services;

public interface IInventoryService
{
    Task<int> CreateStockInAsync(CreateStockInDto dto);
    Task<int> CreateStockOutAsync(CreateStockOutDto dto);
}