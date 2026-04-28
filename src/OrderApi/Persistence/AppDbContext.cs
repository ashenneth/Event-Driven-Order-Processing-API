using Microsoft.EntityFrameworkCore;
using OrderApi.Persistence.Entities;

namespace OrderApi.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(b =>
        {
            b.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(320);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.HasMany(x => x.Items)
             .WithOne(x => x.Order!)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.Sku).IsUnique();
        });
    }
}