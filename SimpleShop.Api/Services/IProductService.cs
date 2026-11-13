using SimpleShop.Api.Dtos;

namespace SimpleShop.Api.Services;

// =============================================================================
//  IProductService  —  an INTERFACE (a "contract")
// =============================================================================
//  An interface lists WHAT a service can do, without saying HOW.
//  ProductService (the next file) provides the "how".
//
//  The controller only knows about this interface, never about ProductService
//  directly. Benefits:
//    • Loose coupling: you can swap the implementation without touching the controller.
//    • Testing: in unit tests you can pass a fake IProductService to the controller.
//
//  The connection between the two is made in Program.cs:
//      builder.Services.AddScoped<IProductService, ProductService>();
//
//  Every method returns a Task<...> because it is ASYNC: while we wait for the
//  database, the server thread is free to handle other requests.
// =============================================================================
public interface IProductService
{
    /// <summary>Returns all products, optionally filtered by a search text in the name.</summary>
    Task<List<ProductResponse>> GetAllAsync(string? search);

    /// <summary>Returns one product, or null if it doesn't exist.</summary>
    Task<ProductResponse?> GetByIdAsync(int id);

    /// <summary>Creates a product. Throws ConflictException if the name is already used.</summary>
    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    /// <summary>Updates a product. Returns null if it doesn't exist.</summary>
    Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request);

    /// <summary>Deletes a product. Returns false if it doesn't exist.</summary>
    Task<bool> DeleteAsync(int id);
}
