namespace Inventory.Dtos;

public record LocationDto(
    int Id,
    string Room,
    string RackNo,
    string Bin,
    string Label
);

public record CreateLocationDto(
    string Room,
    string RackNo,
    string Bin
);

public record UpdateLocationDto(
    string Room,
    string RackNo,
    string Bin
);