using System.ComponentModel.DataAnnotations;

namespace SimpleShop.Api.Dtos;

// =============================================================================
//  UpdateProductRequest  —  DTO for   PUT /api/products/{id}
// =============================================================================
//  PUT means "replace the whole product", so the client sends ALL fields again.
//
//  It looks identical to CreateProductRequest today, but keeping them separate
//  is common practice: create and update rules often become different later
//  (e.g. "you can't change the Name after creation").
// =============================================================================
public class UpdateProductRequest
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
