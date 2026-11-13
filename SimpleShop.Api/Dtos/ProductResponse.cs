namespace SimpleShop.Api.Dtos;

// =============================================================================
//  ProductResponse  —  a DTO for OUTPUT
// =============================================================================
//  This is the shape of the JSON the API sends BACK to the client:
//
//      {
//        "id": 1,
//        "name": "Wireless Mouse",
//        "description": "...",
//        "price": 24.99,
//        "stock": 50,
//        "inStock": true,
//        "createdAt": "2026-01-01T00:00:00Z"
//      }
//
//  Notice "inStock" — it does NOT exist in the database. A response DTO can
//  add, rename or hide fields. Your database design and your public API
//  can change independently. That's the main reason DTOs exist.
//
//  "record" is a short way to write a simple, read-only data class.
//  This one line creates a class with 7 properties and a constructor.
// =============================================================================
public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool InStock,
    DateTime CreatedAt);
