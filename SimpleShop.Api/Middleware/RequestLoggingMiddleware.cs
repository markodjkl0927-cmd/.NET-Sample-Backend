using System.Diagnostics;

namespace SimpleShop.Api.Middleware;

// =============================================================================
//  RequestLoggingMiddleware  —  runs for EVERY request
// =============================================================================
//  A middleware is a piece of code that sits in the request pipeline.
//  It receives "next" (= the rest of the pipeline) and decides:
//
//      • do something BEFORE   →  await _next(context)  →  do something AFTER
//
//  Code before "await _next" runs on the way IN  (request).
//  Code after  "await _next" runs on the way OUT (response).
//
//  This one prints two lines to the console for each request, e.g.:
//      --> GET /api/products/1
//      <-- GET /api/products/1 responded 200 in 14 ms
// =============================================================================
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // "next" is given to us by ASP.NET Core when the pipeline is built (app.UseMiddleware<...>).
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // ASP.NET Core calls InvokeAsync once per request.
    // HttpContext contains EVERYTHING about the current request and response.
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;                                   // GET, POST, ...
        var path = $"{context.Request.Path}{context.Request.QueryString}";     // /api/products?search=x

        // ---- ON THE WAY IN ----
        _logger.LogInformation("--> {Method} {Path}", method, path);
        var stopwatch = Stopwatch.StartNew();

        // Hand the request to the next middleware (and eventually the controller).
        await _next(context);

        // ---- ON THE WAY OUT ---- (the response has been produced by now)
        stopwatch.Stop();
        _logger.LogInformation("<-- {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
