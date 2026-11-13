using System.ComponentModel.DataAnnotations;

namespace SimpleShop.Api.Dtos;

// =============================================================================
//  CreateProductRequest  —  a DTO ("Data Transfer Object") for INPUT
// =============================================================================
//  This is the shape of the JSON the client sends to   POST /api/products
//
//      {
//        "name": "USB-C Cable",
//        "description": "1 meter",
//        "price": 9.99,
//        "stock": 100
//      }
//
//  ASP.NET Core automatically converts that JSON into this C# object
//  ("model binding"), then checks the [validation attributes] below.
//  If any rule fails, the controller is NEVER called — the client gets an
//  automatic "400 Bad Request" listing the errors.
//
//  Why not just use the Product entity here?
//   • The client must NOT be able to set "Id" or "CreatedAt" — the server does that.
//   • Validation rules for input belong here, not on the database entity.
// =============================================================================
public class CreateProductRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description can be at most 500 characters.")]
    public string? Description { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Price must be between 0.01 and 1,000,000.")]
    public decimal Price { get; set; }

    [Range(0, 10_000, ErrorMessage = "Stock must be between 0 and 10,000.")]
    public int Stock { get; set; }
}
