using Microsoft.AspNetCore.Mvc;

namespace Inventory.Models;

public class Entity
{
    public Entity(EntityType entityType)
    {
        this.EntityType = entityType;
    }
    
    public int Id { get; set; }
    
    public  string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public EntityType EntityType { get; set; }
}