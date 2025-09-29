using System.ComponentModel.DataAnnotations;

namespace Inventory.Models;

public class Location : Entity
{
    public Location() : base(EntityType.Location) {}

    [Required, MaxLength(64)]
    public string Room { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string RackNo { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Bin { get; set; } = string.Empty;

    public string Label => $"{Room}-{RackNo}-{Bin}";
}