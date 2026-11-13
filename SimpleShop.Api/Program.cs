// =============================================================================
//  Program.cs  —  THE ENTRY POINT OF THE APPLICATION
// =============================================================================
//  When you type `dotnet run`, execution starts at the top of this file.
//
//  This file does its work in 2 phases:
//
//   PHASE 1 – REGISTER SERVICES  (builder.Services.Add...)
//             "Here is the list of things my app can use."
//             This fills the Dependency Injection (DI) container.
//
//   PHASE 2 – BUILD THE PIPELINE (app.Use... / app.Map...)
//             "Here is what happens, in order, to EVERY incoming HTTP request."
//
//  Finally, app.Run() starts the web server (called "Kestrel") and waits
//  for requests forever (until you press Ctrl+C).
// =============================================================================

using Microsoft.EntityFrameworkCore;
using SimpleShop.Api.Data;
using SimpleShop.Api.Middleware;
using SimpleShop.Api.Services;

// The "builder" loads configuration (appsettings.json, appsettings.Development.json,
// environment variables, command-line args) and sets up logging for us.
var builder = WebApplication.CreateBuilder(args);


// -----------------------------------------------------------------------------
// PHASE 1: REGISTER SERVICES IN THE DEPENDENCY INJECTION (DI) CONTAINER
// -----------------------------------------------------------------------------
// Think of the DI container as a "factory". Later, when a class asks for
// something in its constructor (e.g. ProductsController asks for IProductService),
// the container creates it automatically. You never write "new ProductService(...)".

// 1) Controllers — ASP.NET Core scans the project for classes that inherit
//    from ControllerBase (see the /Controllers folder).
builder.Services.AddControllers();

//    Generate lowercase URLs (e.g. in the "Location" header: /api/products/4, not /api/Products/4).
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// 2) OpenAPI — builds a machine-readable description of every endpoint.
//    Swagger UI (below) reads it to show a nice test page.
builder.Services.AddOpenApi();

// 3) Database — register our AppDbContext and tell EF Core to use SQLite.
//    The connection string is read from appsettings.json:
//       "ConnectionStrings": { "DefaultConnection": "Data Source=simpleshop.db" }
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// 4) Our own business-logic service.
//    Read it as: "Whenever someone asks for IProductService, give them a ProductService."
//    "Scoped" means: ONE instance per HTTP request (a new one for the next request).
//    (Other lifetimes: AddSingleton = one for the whole app, AddTransient = new every time.)
builder.Services.AddScoped<IProductService, ProductService>();


// Everything is registered — now build the actual application object.
var app = builder.Build();


// -----------------------------------------------------------------------------
// CREATE THE DATABASE ON STARTUP  (fine for learning — see README for "migrations")
// -----------------------------------------------------------------------------
// EnsureCreated() creates simpleshop.db with the "Products" table and the seed
// data if the file doesn't exist yet. If it already exists, it does nothing.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}


// -----------------------------------------------------------------------------
// PHASE 2: THE MIDDLEWARE PIPELINE  —  ORDER MATTERS!
// -----------------------------------------------------------------------------
// Each request flows DOWN through these in order, reaches an endpoint,
// and then the response flows back UP through them in reverse order.
//
//   Request  →  RequestLogging → ExceptionHandling → Routing → Controller
//   Response ←  RequestLogging ← ExceptionHandling ← ........ ← Controller

// (a) Logs every request and how long it took. It's first (outermost), so it
//     sees the FINAL status code — even a 500 produced by the error handler below.
app.UseMiddleware<RequestLoggingMiddleware>();

// (b) Catches any exception thrown further down and turns it into a clean
//     JSON error response (409 for business conflicts, 500 for real bugs).
app.UseMiddleware<ExceptionHandlingMiddleware>();

// (c) Only in Development: expose the API description and the Swagger UI page.
//     (In Production you usually don't want to publish your API docs to the world.)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // → http://localhost:5080/openapi/v1.json
    app.UseSwaggerUI(options =>       // → http://localhost:5080/swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SimpleShop API v1");
    });
}

// (d) Connect URLs to controller actions, using the [Route]/[HttpGet]... attributes.
app.MapControllers();

// (e) A tiny "Minimal API" endpoint — the other (shorter) way to write endpoints in .NET.
//     Real projects pick one style; we show both so you'll recognize them.
app.MapGet("/", () => Results.Ok(new
{
    message = "SimpleShop API is running!",
    swagger = "/swagger",
    products = "/api/products"
}));


// Start the web server and listen for requests (blocks until Ctrl+C).
app.Run();
