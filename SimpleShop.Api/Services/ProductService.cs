using Microsoft.EntityFrameworkCore;
using SimpleShop.Api.Data;
using SimpleShop.Api.Dtos;
using SimpleShop.Api.Exceptions;
using SimpleShop.Api.Models;

namespace SimpleShop.Api.Services;

// =============================================================================
//  ProductService  —  THE BUSINESS LOGIC LAYER
// =============================================================================
//  This is where the "real work" and the business RULES live:
//    • talking to the database (through AppDbContext)
//    • rules like "product names must be unique"
//    • converting Entities ⇄ DTOs
//
//  It knows NOTHING about HTTP (no status codes, no requests/responses).
//  That's the controller's job. Keeping them apart makes each one simpler
//  and lets you reuse this logic elsewhere (a background job, a CLI tool, tests...).
// =============================================================================
public class ProductService : IProductService
{
    // "private readonly" fields: set once in the constructor, never changed.
    // The "_" prefix is the usual C# naming convention for private fields.
    private readonly AppDbContext _db;
    private readonly ILogger<ProductService> _logger;

    // CONSTRUCTOR INJECTION:
    // We never call "new ProductService(...)" ourselves. The DI container sees
    // that this constructor needs an AppDbContext and an ILogger, creates them
    // (because they're registered in Program.cs) and passes them in.
    public ProductService(AppDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // READ (all)
    // -------------------------------------------------------------------------
    public async Task<List<ProductResponse>> GetAllAsync(string? search)
    {
        // IQueryable = a query that is BUILT but NOT EXECUTED yet.
        // AsNoTracking() = "I'm only reading" → faster, EF won't watch for changes.
        IQueryable<Product> query = _db.Products.AsNoTracking();

        // We can keep adding conditions to the query...
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        // ...and only now, at ToListAsync(), the SQL is sent to the database.
        var products = await query.OrderBy(p => p.Id).ToListAsync();

        // Convert each entity into a response DTO before returning it.
        return products.Select(ToResponse).ToList();
    }

    // -------------------------------------------------------------------------
    // READ (one)
    // -------------------------------------------------------------------------
    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);   // returns null if not found

        // "condition ? a : b" is the ternary operator: if not found → null, else → DTO.
        return product is null ? null : ToResponse(product);
    }

    // -------------------------------------------------------------------------
    // CREATE
    // -------------------------------------------------------------------------
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        // Business rule: no two products with the same name.
        await EnsureNameIsUniqueAsync(request.Name);

        // Map: request DTO → entity. The SERVER decides Id and CreatedAt, not the client.
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);      // 1) EF now "tracks" it as Added (nothing in the DB yet)
        await _db.SaveChangesAsync();   // 2) EF runs the INSERT; product.Id is now filled in

        // Structured logging: {ProductId} becomes a searchable field in log tools.
        _logger.LogInformation("Created product {ProductId} '{ProductName}'", product.Id, product.Name);

        return ToResponse(product);
    }

    // -------------------------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------------------------
    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        // No AsNoTracking() here: we WANT EF to track this object so it can detect our changes.
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return null;   // the controller will turn this into "404 Not Found"
        }

        await EnsureNameIsUniqueAsync(request.Name, excludeId: id);

        // Just change the properties of the C# object...
        product.Name = request.Name.Trim();
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        // ...and EF figures out which columns changed and runs the UPDATE for us.
        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated product {ProductId}", product.Id);
        return ToResponse(product);
    }

    // -------------------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------------------
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        _db.Products.Remove(product);   // mark as Deleted
        await _db.SaveChangesAsync();   // run the DELETE

        _logger.LogInformation("Deleted product {ProductId}", id);
        return true;
    }

    // -------------------------------------------------------------------------
    // PRIVATE HELPERS
    // -------------------------------------------------------------------------

    // Throws ConflictException if another product already uses this name (case-insensitive).
    // "excludeId" lets an update keep its own name without conflicting with itself.
    private async Task EnsureNameIsUniqueAsync(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();

        bool nameTaken = await _db.Products.AnyAsync(p =>
            p.Name.ToLower() == normalized &&
            (excludeId == null || p.Id != excludeId));

        if (nameTaken)
        {
            throw new ConflictException($"A product named '{name.Trim()}' already exists.");
        }
    }

    // MAPPING: Entity → Response DTO. (Big projects often use a library like
    // AutoMapper or Mapster for this; writing it by hand is clearer for learning.)
    private static ProductResponse ToResponse(Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.Stock,
        InStock: p.Stock > 0,
        p.CreatedAt);
}
