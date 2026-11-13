using Microsoft.EntityFrameworkCore;
using SimpleShop.Api.Models;

namespace SimpleShop.Api.Data;

// =============================================================================
//  AppDbContext  —  YOUR CONNECTION TO THE DATABASE (Entity Framework Core)
// =============================================================================
//  EF Core is an "ORM" (Object-Relational Mapper). It lets you work with the
//  database using C# objects and LINQ instead of writing SQL by hand:
//
//      C# you write:   _db.Products.Where(p => p.Stock > 0).ToListAsync()
//      SQL EF runs:    SELECT * FROM "Products" WHERE "Stock" > 0
//
//  (Run the app in Development and watch the console — you'll see every SQL query!)
//
//  A DbContext:
//    • holds one DbSet<T> per table          (Products → the "Products" table)
//    • tracks changes you make to objects    (change tracking)
//    • writes them to the database when you call SaveChangesAsync()
//
//  It is registered in Program.cs with AddDbContext(...) → lifetime "Scoped",
//  so every HTTP request gets its own fresh AppDbContext.
// =============================================================================
public class AppDbContext : DbContext
{
    // The options (which database, which connection string) are passed in by the
    // DI container — they come from the AddDbContext(...) call in Program.cs.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // A DbSet represents a table. You query it with LINQ and add/remove rows.
    public DbSet<Product> Products => Set<Product>();

    // OnModelCreating = the place to fine-tune how C# classes map to tables
    // (lengths, indexes, relationships, seed data, ...). This is called the "Fluent API".
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Column rules (these become constraints in the database itself)
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);

            // A UNIQUE index: the database itself refuses two products with the same name.
            // (Our service also checks this first, to return a friendly error message.)
            entity.HasIndex(p => p.Name).IsUnique();

            // SQLite saves dates as plain text and "forgets" that they were UTC.
            // This VALUE CONVERTER says: when reading CreatedAt back, mark it as UTC,
            // so the JSON shows "2026-01-01T00:00:00Z" (the "Z" means UTC).
            entity.Property(p => p.CreatedAt)
                  .HasConversion(
                      toDb => toDb,                                                // C# → database: unchanged
                      fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));   // database → C#: mark as UTC

            // SEED DATA: rows inserted automatically when the database is created,
            // so you have something to look at on the very first run.
            // (Seed values must be fixed — so no DateTime.UtcNow here.)
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            entity.HasData(
                new Product { Id = 1, Name = "Wireless Mouse",      Description = "Ergonomic 2.4 GHz mouse",       Price = 24.99m,  Stock = 50, CreatedAt = seedDate },
                new Product { Id = 2, Name = "Mechanical Keyboard", Description = "RGB, blue switches",            Price = 89.90m,  Stock = 20, CreatedAt = seedDate },
                new Product { Id = 3, Name = "27-inch Monitor",     Description = "1440p, 144 Hz IPS panel",       Price = 299.00m, Stock = 0,  CreatedAt = seedDate }
            );
        });
    }
}
