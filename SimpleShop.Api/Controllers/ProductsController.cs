using Microsoft.AspNetCore.Mvc;
using SimpleShop.Api.Dtos;
using SimpleShop.Api.Services;

namespace SimpleShop.Api.Controllers;

// =============================================================================
//  ProductsController  —  THE HTTP LAYER (the "front door" of the API)
// =============================================================================
//  A controller's ONLY job is to translate between HTTP and C#:
//
//    1. Receive the HTTP request  (URL, query string, JSON body)
//    2. Call the service          (the service does the real work)
//    3. Return an HTTP response   (status code + JSON)
//
//  Keep controllers THIN — no business rules, no database code here.
//
//  The attributes on top configure it:
//   [ApiController]               → turns on API conveniences, most importantly:
//                                   automatic "400 Bad Request" when validation fails.
//   [Route("api/[controller]")]   → base URL. [controller] = class name minus
//                                   "Controller" → "api/products"
// =============================================================================
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    // Constructor injection again: the DI container gives us an IProductService
    // (which, according to Program.cs, is a ProductService).
    // A NEW controller object is created for EVERY request.
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // -------------------------------------------------------------------------
    // GET /api/products
    // GET /api/products?search=mouse
    // -------------------------------------------------------------------------
    // [FromQuery] → the value comes from the query string (?search=...).
    // ActionResult<T> → "returns an HTTP result, normally containing a T".
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll([FromQuery] string? search)
    {
        var products = await _productService.GetAllAsync(search);
        return Ok(products);                                   // 200 OK + JSON array
    }

    // -------------------------------------------------------------------------
    // GET /api/products/5
    // -------------------------------------------------------------------------
    // "{id:int}" is a ROUTE PARAMETER with a constraint: only numbers match.
    // The "5" from the URL is put into the "id" parameter automatically.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();                                 // 404 Not Found
        }

        return Ok(product);                                    // 200 OK + JSON object
    }

    // -------------------------------------------------------------------------
    // POST /api/products        (body: JSON)
    // -------------------------------------------------------------------------
    // [FromBody] → the JSON body is converted into a CreateProductRequest.
    // By the time this method runs, [ApiController] has ALREADY validated it.
    // If validation failed, we never get here (client receives 400 automatically).
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
    {
        var created = await _productService.CreateAsync(request);

        // 201 Created + a "Location" header pointing to the new item
        // (e.g. Location: http://localhost:5080/api/products/4) + the JSON body.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // -------------------------------------------------------------------------
    // PUT /api/products/5       (body: JSON)
    // -------------------------------------------------------------------------
    // Here "id" comes from the URL and "request" from the body — both at once.
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(id, request);

        if (updated is null)
        {
            return NotFound();                                 // 404 Not Found
        }

        return Ok(updated);                                    // 200 OK + updated JSON
    }

    // -------------------------------------------------------------------------
    // DELETE /api/products/5
    // -------------------------------------------------------------------------
    // IActionResult (no <T>) because a successful delete returns no body.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();                                 // 404 Not Found
        }

        return NoContent();                                    // 204 No Content
    }
}
