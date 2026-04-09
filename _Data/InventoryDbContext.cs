using Microsoft.EntityFrameworkCore;
using Models.Entities;

namespace Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserCategory> UserCategories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryType> InventoryTypes { get; set; }
    
    // THE FIX: Register the License table
    public DbSet<UserLicense> UserLicenses { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- SEED DATA ---
        modelBuilder.Entity<InventoryType>().HasData(
            new InventoryType { Id = 1, TypeName = "Food" },
            new InventoryType { Id = 2, TypeName = "Material" },
            new InventoryType { Id = 3, TypeName = "Equipment" },
            new InventoryType { Id = 4, TypeName = "Consumable" }
        );

        // --- SAAS RELATIONSHIP (The License Bridge) ---
        // A User has ONE License, and a License belongs to ONE User.
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.License)
            .WithOne(l => l.User)
            .HasForeignKey<UserLicense>(l => l.UserId);

        // --- OPTIMIZATIONS ---
        modelBuilder.Entity<InventoryItem>()
            .HasIndex(i => new { i.UserId, i.BatchName });

        modelBuilder.Entity<InventoryItem>()
            .Property(i => i.Quantity)
            .HasPrecision(18, 2);

        // --- RELATIONSHIPS ---
        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.InventoryType)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.InventoryTypeId);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.User)
            .WithMany(u => u.Inventory)
            .HasForeignKey(i => i.UserId);

        modelBuilder.Entity<UserCategory>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId);
    }
}