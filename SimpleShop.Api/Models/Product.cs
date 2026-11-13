namespace SimpleShop.Api.Models;

// =============================================================================
//  Product  —  an ENTITY
// =============================================================================
//  An "entity" is a C# class that maps 1-to-1 to a DATABASE TABLE.
//
//      C# class  Product        ⇄   database table  "Products"
//      each property            ⇄   a column
//      each Product object      ⇄   a row
//
//  Entity Framework Core (EF Core) reads this class and creates the table for us.
//
//  ⚠ Entities are INTERNAL to the backend. We never send them directly to the
//    client — we convert them into DTOs first (see the /Dtos folder).
// =============================================================================
public class Product
{
    // EF Core convention: a property called "Id" becomes the PRIMARY KEY,
    // and because it's an int, the database auto-generates it (1, 2, 3, ...).
    public int Id { get; set; }

    // "= string.Empty" gives it a non-null default value (we enabled <Nullable> in the .csproj).
    public string Name { get; set; } = string.Empty;

    // "string?" (with a question mark) means "may be null" → the column allows NULL.
    public string? Description { get; set; }

    // Always use decimal (not double/float) for money — it has no rounding errors.
    public decimal Price { get; set; }

    // How many items are in the warehouse.
    public int Stock { get; set; }

    // When the product was created. We always store times in UTC on the server.
    public DateTime CreatedAt { get; set; }
}
