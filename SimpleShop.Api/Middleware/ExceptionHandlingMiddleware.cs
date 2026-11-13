using Microsoft.AspNetCore.Mvc;
using SimpleShop.Api.Exceptions;

namespace SimpleShop.Api.Middleware;

// =============================================================================
//  ExceptionHandlingMiddleware  —  ONE place to handle ALL errors
// =============================================================================
//  Without this, an unhandled exception would give the client an ugly error
//  (or, worse, leak internal details like stack traces).
//
//  It wraps the rest of the pipeline in a try/catch:
//    • ConflictException (a business rule)  → 409 Conflict  + the rule's message
//    • any other Exception (a real bug)     → 500 Internal Server Error
//                                              + a generic message (details go to the LOG only)
//
//  The error body uses the standard "Problem Details" format (RFC 9457),
//  the same format ASP.NET Core uses for 400 and 404 responses:
//      {
//        "status": 409,
//        "title": "Conflict",
//        "detail": "A product named 'Wireless Mouse' already exists.",
//        "instance": "/api/products"
//      }
// =============================================================================
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Run the rest of the pipeline (routing → controller → service → database).
            await _next(context);
        }
        catch (ConflictException ex)
        {
            // Expected situation caused by the client's data → a Warning, not an Error.
            _logger.LogWarning("Business rule violated: {Message}", ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected bug → log the FULL exception (with stack trace) for developers...
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);

            // ...but show the client only a safe, generic message.
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Server error", "Something went wrong. Please try again later.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
