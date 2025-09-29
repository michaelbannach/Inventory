using Microsoft.EntityFrameworkCore;
using Inventory.Models;

namespace Inventory.Data;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    public DbSet<PropertyValue> PropertyValues => Set<PropertyValue>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<StockOut> StockOuts => Set<StockOut>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        var loc = modelBuilder.Entity<Location>();
        loc.Property(x => x.Room).HasMaxLength(64).IsRequired();
        loc.Property(x => x.RackNo).HasMaxLength(32).IsRequired();
        loc.Property(x => x.Bin).HasMaxLength(32).IsRequired();
        loc.HasIndex(x => new { x.Room, x.RackNo, x.Bin }).IsUnique();

       
        var ti = modelBuilder.Entity<TransactionItem>();
        ti.HasKey(t => t.Id);

        ti.Property(t => t.Amount).IsRequired();

        ti.HasOne(t => t.Item)
          .WithMany()
          .HasForeignKey(t => t.ItemId)
          .OnDelete(DeleteBehavior.Restrict);

        ti.HasOne(t => t.StockIn)
          .WithMany(s => s.Items)
          .HasForeignKey(t => t.StockInId)
          .OnDelete(DeleteBehavior.Cascade);

        ti.HasOne(t => t.StockOut)
          .WithMany(s => s.TransactionItems)
          .HasForeignKey(t => t.StockOutId)
          .OnDelete(DeleteBehavior.Cascade);

        ti.HasOne(t => t.Location)
          .WithMany()
          .HasForeignKey(t => t.LocationId)
          .OnDelete(DeleteBehavior.SetNull);

        
        ti.HasIndex(t => t.ItemId);
        ti.HasIndex(t => t.StockInId);
        ti.HasIndex(t => t.StockOutId);
        ti.HasIndex(t => t.LocationId);

       
        ti.HasIndex(t => new { t.ItemId, t.LocationId });
        ti.HasIndex(t => new { t.ItemId, t.StockInId });

        
        ti.ToTable(tb => tb.HasCheckConstraint(
            "CK_TransactionItem_ExactlyOneParent",
            "((CASE WHEN StockInId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN StockOutId IS NOT NULL THEN 1 ELSE 0 END)) = 1"
        ));

        
        modelBuilder.Entity<StockIn>()
            .Property(p => p.IncomeAt)
            .IsRequired();

        modelBuilder.Entity<StockOut>()
            .Property(p => p.OutcomeAt)
            .IsRequired();

     
        modelBuilder.Entity<ItemTypeProperty>(itp =>
        {
            itp.HasKey(x => x.Id);

            // Ein PropertyDefinition pro ItemType nur einmal
            itp.HasIndex(x => new { x.ItemTypeId, x.PropertyDefinitionId }).IsUnique();

            itp.HasOne(x => x.ItemType)
               .WithMany(t => t.ItemTypeProperties)
               .HasForeignKey(x => x.ItemTypeId)
               .OnDelete(DeleteBehavior.Cascade);

            itp.HasOne(x => x.PropertyDefinition)
               .WithMany(p => p.ItemTypeProperties)
               .HasForeignKey(x => x.PropertyDefinitionId)
               .OnDelete(DeleteBehavior.Cascade);
        });

       
        modelBuilder.Entity<PropertyValue>(pv =>
        {
            pv.HasKey(x => x.Id);

            pv.Property(x => x.Value).IsRequired();

            pv.HasOne(x => x.Item)
              .WithMany(i => i.Properties)
              .HasForeignKey(x => x.ItemId)
              .OnDelete(DeleteBehavior.Cascade);

            pv.HasOne(x => x.PropertyDefinition)
              .WithMany() 
              .HasForeignKey(x => x.PropertyDefinitionId)
              .OnDelete(DeleteBehavior.Restrict);

            
            pv.HasIndex(x => new { x.ItemId, x.PropertyDefinitionId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
