# SimpleShop API — a beginner's guide to .NET backend projects

This is a **small but realistic** .NET backend project. It is a REST API that manages the
**products** of an online shop. You can list, search, create, update and delete them (CRUD).

It is built the way most real .NET backends are built, just smaller. Once you understand this
project, you will recognize the same pieces in almost every .NET backend you see at work.

| Technology | What it does here |
|---|---|
| **.NET 10** | The platform (runtime + SDK + C# language) |
| **ASP.NET Core** | The web framework: receives HTTP requests, sends HTTP responses |
| **Entity Framework Core (EF Core)** | The ORM: lets us use the database with C# instead of SQL |
| **SQLite** | A tiny database stored in a single file (`simpleshop.db`), no installation needed |
| **Swagger UI** | A web page where you can click and test every endpoint |

---

## Table of contents

1. [What does a backend actually do?](#1-what-does-a-backend-actually-do)
2. [How to run the project](#2-how-to-run-the-project)
3. [The folder structure](#3-the-folder-structure)
4. [The big picture: layers](#4-the-big-picture-layers)
5. [Follow one request, step by step](#5-follow-one-request-step-by-step)
6. [Key concepts explained](#6-key-concepts-explained)
7. [API reference](#7-api-reference)
8. [Exercises: learn by doing](#8-exercises-learn-by-doing)
9. [What bigger, real-world projects add](#9-what-bigger-real-world-projects-add)
10. [Glossary](#10-glossary)
11. [Useful commands](#11-useful-commands)

---

## 1. What does a backend actually do?

A **client** (a website, a mobile app, another server) sends an **HTTP request**.
The **backend** receives it, applies rules, reads/writes the **database**, and sends back an
**HTTP response**, usually with **JSON** data.

```
   CLIENT                              BACKEND (this project)                DATABASE
 ┌─────────┐   GET /api/products/1    ┌──────────────────────┐   SELECT ...  ┌──────────┐
 │ browser │ ───────────────────────▶ │                      │ ────────────▶ │          │
 │ mobile  │                          │   SimpleShop.Api     │               │  SQLite  │
 │ Postman │ ◀─────────────────────── │                      │ ◀──────────── │          │
 └─────────┘   200 OK + JSON          └──────────────────────┘   rows        └──────────┘
```

An HTTP request has:
- a **method** (the action): `GET` = read, `POST` = create, `PUT` = update, `DELETE` = delete
- a **URL** (the resource): `/api/products/1`
- sometimes a **body** (the data, as JSON) — for `POST` and `PUT`

An HTTP response has:
- a **status code**: `200 OK`, `201 Created`, `404 Not Found`, `400 Bad Request`, ...
- usually a **body** (JSON)

That's it. Everything else in this project is just a clean way of organizing that work.

---

## 2. How to run the project

### Step 1 — Install the .NET 10 SDK (once)

Download it from <https://dotnet.microsoft.com/download/dotnet/10.0>, or on Windows run:

```bash
winget install Microsoft.DotNet.SDK.10
```

Check it works (it should print `10.0.xxx`):

```bash
dotnet --version
```

> **SDK vs Runtime:** the *Runtime* can only **run** .NET apps. The *SDK* can also **build**
> them (it includes the compiler and the `dotnet` CLI commands). Developers need the SDK.

### Step 2 — Run the API

```bash
cd SimpleShop.Api
dotnet run
```

You will see something like:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5080
```

### Step 3 — Try it

- Open **<http://localhost:5080/swagger>** in your browser. Click an endpoint → **Try it out** → **Execute**.
- Or open `SimpleShop.Api/SimpleShop.Api.http` in Visual Studio / Rider / VS Code (with the
  *REST Client* extension) and click **Send request** above any request.
- Watch the **console** while you do it: you'll see each request and the **SQL** that EF Core runs.
  For example, `GET /api/products/1` prints:

```
info: SimpleShop.Api.Middleware.RequestLoggingMiddleware[0]
      --> GET /api/products/1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@id='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."CreatedAt", "p"."Description", "p"."Name", "p"."Price", "p"."Stock"
      FROM "Products" AS "p"
      WHERE "p"."Id" = @id
      LIMIT 1
info: SimpleShop.Api.Middleware.RequestLoggingMiddleware[0]
      <-- GET /api/products/1 responded 200 in 42 ms
```

  The first line comes from our middleware (on the way **in**), the SQL comes from EF Core
  (inside the service), and the last line comes from our middleware again (on the way **out**).

Press **Ctrl+C** in the console to stop the server.

### Other ways to open the project

- **Visual Studio 2022/2026**: double-click `SimpleShop.slnx`, then press **F5**.
- **VS Code**: open the folder, install the **C# Dev Kit** extension, press **F5**.
- **JetBrains Rider**: open `SimpleShop.slnx`, click **Run**.

### Reset the database

Stop the app and delete `SimpleShop.Api/simpleshop.db`. The next start creates a fresh
database with the 3 sample products again.

---

## 3. The folder structure

```
.NET Backend Simple/
│
├── SimpleShop.slnx                  ← "Solution" file: groups one or more projects (open this in Visual Studio)
├── README.md                        ← this guide
├── .gitignore                       ← files Git should ignore (bin/, obj/, *.db)
│
└── SimpleShop.Api/                  ← THE PROJECT
    │
    ├── SimpleShop.Api.csproj        ← project file: .NET version + NuGet packages
    ├── Program.cs                   ← ⭐ ENTRY POINT: registers services + builds the request pipeline
    ├── appsettings.json             ← configuration (connection string, log levels)
    ├── appsettings.Development.json ← extra settings used only in Development
    ├── SimpleShop.Api.http          ← ready-made test requests
    │
    ├── Properties/
    │   └── launchSettings.json      ← how "dotnet run" starts the app (URL, environment)
    │
    ├── Controllers/                 ← HTTP LAYER: receives requests, returns responses
    │   └── ProductsController.cs
    │
    ├── Services/                    ← BUSINESS LOGIC LAYER: rules + database work
    │   ├── IProductService.cs       ←   the contract (interface)
    │   └── ProductService.cs        ←   the implementation
    │
    ├── Data/                        ← DATA ACCESS LAYER: EF Core database context
    │   └── AppDbContext.cs
    │
    ├── Models/                      ← ENTITIES: C# classes that map to database tables
    │   └── Product.cs
    │
    ├── Dtos/                        ← DATA TRANSFER OBJECTS: the JSON shapes going in/out of the API
    │   ├── CreateProductRequest.cs
    │   ├── UpdateProductRequest.cs
    │   └── ProductResponse.cs
    │
    ├── Middleware/                  ← code that runs for EVERY request
    │   ├── RequestLoggingMiddleware.cs
    │   └── ExceptionHandlingMiddleware.cs
    │
    └── Exceptions/                  ← custom exception types
        └── ConflictException.cs
```

Folders created automatically (never edit or commit them):
- `bin/` – the compiled program (`SimpleShop.Api.dll`)
- `obj/` – temporary build files

> **Suggested reading order:** `Program.cs` → `Controllers/ProductsController.cs` →
> `Services/ProductService.cs` → `Data/AppDbContext.cs` → `Models/Product.cs` → `Dtos/*` →
> `Middleware/*`. Every file has comments explaining what it does and why.

---

## 4. The big picture: layers

The code is split into **layers**. Each layer has **one job** and only talks to the layer
directly below it.

```
               Client (Swagger, browser, mobile app, Postman ...)
                         │                    ▲
                HTTP request             HTTP response (JSON)
                         ▼                    │
   ┌──────────────────────────────────────────────────────────┐
   │  MIDDLEWARE PIPELINE                       /Middleware    │  runs for EVERY request:
   │  RequestLogging → ExceptionHandling                       │  logging, error handling
   └──────────────────────────────────────────────────────────┘
                         │                    ▲
   ┌──────────────────────────────────────────────────────────┐
   │  CONTROLLER  (HTTP layer)                  /Controllers   │  reads URL + JSON,
   │  ProductsController                                       │  chooses the status code
   └──────────────────────────────────────────────────────────┘
                         │   DTOs             ▲   DTOs
   ┌──────────────────────────────────────────────────────────┐
   │  SERVICE  (business logic layer)           /Services      │  business rules,
   │  ProductService                                           │  Entity ⇄ DTO mapping
   └──────────────────────────────────────────────────────────┘
                         │   Entities         ▲   Entities
   ┌──────────────────────────────────────────────────────────┐
   │  DATA ACCESS  (EF Core)                    /Data          │  turns C# LINQ into SQL
   │  AppDbContext                                             │
   └──────────────────────────────────────────────────────────┘
                         │   SQL              ▲   rows
                   ┌────────────────────────────────┐
                   │  SQLite database: simpleshop.db │
                   └────────────────────────────────┘
```

**Why split it like this?**

| Layer | Knows about HTTP? | Knows about the database? | Contains business rules? |
|---|---|---|---|
| Controller | ✅ yes | ❌ no | ❌ no |
| Service | ❌ no | ✅ yes (through DbContext) | ✅ yes |
| DbContext | ❌ no | ✅ yes | ❌ no |

- When you need to change a **rule**, you go to the service. When you need to change a **URL**
  or a **status code**, you go to the controller. Every change has an obvious home.
- Each piece is small and easy to test on its own.

---

## 5. Follow one request, step by step

Let's follow what happens when a client creates a product:

```http
POST http://localhost:5080/api/products
Content-Type: application/json

{ "name": "USB-C Cable", "description": "1 meter", "price": 9.99, "stock": 100 }
```

| # | Where | What happens |
|---|---|---|
| 1 | **Kestrel** (the built-in web server) | Receives the raw HTTP request and creates an `HttpContext` object containing the request and an empty response. |
| 2 | `RequestLoggingMiddleware` | Logs `--> POST /api/products` and starts a stopwatch. Calls `_next(context)`. |
| 3 | `ExceptionHandlingMiddleware` | Enters a `try { ... }` block and calls `_next(context)`. |
| 4 | **Routing** (`app.MapControllers()`) | Matches `POST` + `/api/products` to `ProductsController.Create`, because of `[Route("api/[controller]")]` and `[HttpPost]`. |
| 5 | **Dependency Injection** | Creates a `ProductsController`. It needs an `IProductService`, so DI creates a `ProductService`, which needs an `AppDbContext` and an `ILogger` — DI creates those too. |
| 6 | **Model binding** | Converts the JSON body into a `CreateProductRequest` object (`[FromBody]`). |
| 7 | **Validation** (`[ApiController]`) | Checks the attributes `[Required]`, `[StringLength]`, `[Range]`. If anything is wrong → **400 Bad Request** is returned immediately and the controller is **never called**. |
| 8 | `ProductsController.Create` | Calls `_productService.CreateAsync(request)`. |
| 9 | `ProductService.EnsureNameIsUniqueAsync` | Asks the database whether the name is already used. If yes → `throw new ConflictException(...)`. That jumps straight back to the `catch` in step 3 → **409 Conflict**. |
| 10 | `ProductService.CreateAsync` | Creates a `Product` entity from the DTO, sets `CreatedAt`, calls `_db.Products.Add(product)` and `await _db.SaveChangesAsync()` → EF Core runs `INSERT INTO "Products" ...`. The database generates the new `Id`. |
| 11 | `ProductService.ToResponse` | Converts the entity into a `ProductResponse` DTO and returns it. |
| 12 | `ProductsController.Create` | Returns `CreatedAtAction(...)` → status **201 Created**, a `Location: /api/products/4` header, and the product. |
| 13 | **JSON serialization** | ASP.NET Core converts `ProductResponse` into JSON (property names become camelCase: `Name` → `name`). |
| 14 | Back up through the middleware | `ExceptionHandlingMiddleware` had nothing to catch. `RequestLoggingMiddleware` logs `<-- POST /api/products responded 201 in 35 ms`. |
| 15 | **End of request** | The response is sent. The controller, service and `AppDbContext` (all "scoped") are thrown away. |

The client receives:

```http
HTTP/1.1 201 Created
Location: http://localhost:5080/api/products/4
Content-Type: application/json

{
  "id": 4,
  "name": "USB-C Cable",
  "description": "1 meter",
  "price": 9.99,
  "stock": 100,
  "inStock": true,
  "createdAt": "2026-11-12T10:15:30.123Z"
}
```

> 💡 **Tip:** put a **breakpoint** (F9 in Visual Studio) on the first line of each method in the
> controller and service, start with **F5**, send a request from Swagger, and press **F10** to
> walk through these steps yourself. This is the fastest way to understand any backend.

---

## 6. Key concepts explained

### 6.1 `Program.cs` — the startup file

Every ASP.NET Core app starts in `Program.cs`. It always has the same two phases:

```csharp
var builder = WebApplication.CreateBuilder(args);

// PHASE 1 — register services ("what my app HAS")
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// PHASE 2 — build the pipeline ("what happens to EACH REQUEST, in order")
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();   // start the web server
```

When you open a new .NET project at work, **read `Program.cs` first**. It tells you which
database, authentication, logging and services the project uses.

### 6.2 Dependency Injection (DI)

**The problem:** the controller needs a service, the service needs a database context, the
database context needs a connection string... Who creates all these objects?

**Without DI** (bad) — every class creates its own dependencies:

```csharp
public class ProductsController
{
    private readonly ProductService _service = new ProductService(new AppDbContext(...), ...);
    // hard to change, hard to test, every class must know how to build everything
}
```

**With DI** (what .NET does) — a class just **asks** for what it needs in its constructor:

```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)   // "I need an IProductService"
    {
        _productService = productService;
    }
}
```

…and the **DI container** builds and passes it in automatically, following the registrations
in `Program.cs`:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
//                         ▲ when asked for this  ▲ create this
```

**Lifetimes** — how long an object lives:

| Lifetime | Registration | One instance per… | Typical use |
|---|---|---|---|
| **Singleton** | `AddSingleton<,>()` | the whole application | caches, configuration |
| **Scoped** | `AddScoped<,>()` | HTTP request | services, `DbContext` ← most common |
| **Transient** | `AddTransient<,>()` | every time it's requested | small, stateless helpers |

### 6.3 Middleware and the request pipeline

Middleware are like **layers of an onion** (or Russian dolls). Each request goes **in**
through every layer, reaches an endpoint, and the response comes back **out** through the
same layers in reverse:

```
 request  ──▶ [ RequestLogging ──▶ [ ExceptionHandling ──▶ [ Controller ] ] ]
 response ◀── [ RequestLogging ◀── [ ExceptionHandling ◀── [ Controller ] ] ]
```

A middleware looks like this (see `Middleware/RequestLoggingMiddleware.cs`):

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // code here runs BEFORE the controller (on the way in)
    await _next(context);          // ← run the rest of the pipeline
    // code here runs AFTER the controller (on the way out)
}
```

**Order matters.** `RequestLoggingMiddleware` is registered first so that it is the outermost
layer: it sees the **final** status code, even when `ExceptionHandlingMiddleware` turned a crash
into a `500`. Built-in middleware you'll see in real projects: `UseHttpsRedirection()`,
`UseCors()`, `UseAuthentication()`, `UseAuthorization()`.

### 6.4 Controllers, routing and model binding

**Routing** connects a URL to a C# method, using attributes:

```csharp
[Route("api/[controller]")]        // base URL: "api/products" ([controller] = class name minus "Controller")
public class ProductsController : ControllerBase
{
    [HttpGet]                      // GET    /api/products
    [HttpGet("{id:int}")]          // GET    /api/products/5      (only numbers match "id")
    [HttpPost]                     // POST   /api/products
    [HttpPut("{id:int}")]          // PUT    /api/products/5
    [HttpDelete("{id:int}")]       // DELETE /api/products/5
}
```

**Model binding** fills the method's parameters from the request automatically:

| Attribute | Value comes from | Example |
|---|---|---|
| `[FromRoute]` (default for `{id}`) | the URL path | `/api/products/`**`5`** |
| `[FromQuery]` | the query string | `/api/products?`**`search=mouse`** |
| `[FromBody]` | the JSON body | `{ "name": "..." }` |
| `[FromHeader]` | an HTTP header | `Authorization: Bearer ...` |

**Return helpers** set the status code for you:

| Method | Status code | When |
|---|---|---|
| `Ok(data)` | 200 OK | read or update succeeded |
| `CreatedAtAction(...)` | 201 Created | something new was created |
| `NoContent()` | 204 No Content | success, nothing to return (delete) |
| `BadRequest(...)` | 400 Bad Request | the client sent bad data |
| `NotFound()` | 404 Not Found | the item doesn't exist |

### 6.5 DTOs vs Entities

| | **Entity** (`Models/Product.cs`) | **DTO** (`Dtos/*.cs`) |
|---|---|---|
| Represents | a row in a database table | the JSON going in/out of the API |
| Used by | EF Core, the service | the controller, the client |
| Example difference | has `Id`, `CreatedAt` set by the server | `CreateProductRequest` has **no** `Id`; `ProductResponse` adds `inStock` |

**Why not send the entity directly?**
1. **Security** — the client can't set fields it shouldn't (like `Id` or, in real apps, `IsAdmin`).
2. **Freedom** — you can rename a database column without breaking every mobile app using your API.
3. **Shape** — you can add computed fields (`inStock`) or hide internal ones (password hashes!).

The service converts between them (`ToResponse` method in `ProductService.cs`).

### 6.6 Validation

Rules are written as **attributes** on the request DTO:

```csharp
[Required(ErrorMessage = "Name is required.")]
[StringLength(100, MinimumLength = 2)]
public string Name { get; set; }

[Range(0.01, 1_000_000)]
public decimal Price { get; set; }
```

Because the controller has `[ApiController]`, ASP.NET Core checks them **before** your method
runs. If any fail, the client automatically gets:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [ "Name must be between 2 and 100 characters." ],
    "Price": [ "Price must be between 0.01 and 1,000,000." ],
    "Stock": [ "Stock must be between 0 and 10,000." ]
  }
}
```

There are **two kinds** of checks:
- **Input validation** (format: "is the price positive?") → attributes on DTOs → `400`.
- **Business rules** (need the database: "is this name already taken?") → in the service → `409`.

### 6.7 Services and interfaces

The **service** (`ProductService`) holds the business logic. The **interface**
(`IProductService`) is a contract listing what the service can do.

The controller depends on the **interface**, not the class. So you could later replace
`ProductService` with, say, `CachedProductService`, by changing **one line** in `Program.cs`, and the
controller wouldn't change at all. In unit tests you can pass a fake implementation.

### 6.8 Entity Framework Core and the database

EF Core is an **ORM** (Object-Relational Mapper): you write C#, it writes SQL.

| You write (C#) | EF Core runs (SQL) |
|---|---|
| `_db.Products.ToListAsync()` | `SELECT * FROM "Products"` |
| `_db.Products.FirstOrDefaultAsync(p => p.Id == 5)` | `SELECT ... WHERE "Id" = 5 LIMIT 1` |
| `_db.Products.Add(p); await _db.SaveChangesAsync();` | `INSERT INTO "Products" ...` |
| change properties, then `SaveChangesAsync()` | `UPDATE "Products" SET ... WHERE "Id" = 5` |
| `_db.Products.Remove(p); await _db.SaveChangesAsync();` | `DELETE FROM "Products" WHERE "Id" = 5` |

Important ideas:
- **`DbContext`** (`AppDbContext`) = your session with the database.
- **`DbSet<Product>`** (`_db.Products`) = one table.
- **LINQ** (`Where`, `OrderBy`, `Select`, ...) builds a query; nothing is sent to the database
  until you call something like `ToListAsync()`, `FirstOrDefaultAsync()`, `AnyAsync()`.
- **Change tracking**: EF Core remembers the objects it loaded. When you change a property and
  call `SaveChangesAsync()`, it generates the `UPDATE` for you.
- **`AsNoTracking()`**: "I'm only reading" → faster, no tracking.
- **In Development you can see every SQL query in the console** (enabled in
  `appsettings.Development.json`). Watch it while you use Swagger!

**`EnsureCreated()` vs Migrations:** this project calls `db.Database.EnsureCreated()` in
`Program.cs`. It creates the database once, which is fine for learning. Real projects use
**migrations**: versioned C# files that describe each schema change (add a table, add a
column, ...) so the database can be updated safely without losing data:

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add AddCategoryToProduct
dotnet ef database update
```

### 6.9 `async` / `await`

Almost every method in a backend is `async`:

```csharp
public async Task<ProductResponse?> GetByIdAsync(int id)
{
    var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
    ...
}
```

**Analogy:** a waiter (a server *thread*) takes your order to the kitchen (the *database*).
- **Without `await`**, the waiter stands in the kitchen doing nothing until your food is ready.
- **With `await`**, the waiter goes and serves other tables, then comes back when the food is ready.

So the same server can handle **many more requests** at the same time.
Rules of thumb: methods that do I/O (database, files, HTTP calls) are `async`, return `Task` or
`Task<T>`, end with `Async` in their name, and you always `await` them.

### 6.10 Configuration and environments

Settings live in **`appsettings.json`**:

```json
"ConnectionStrings": { "DefaultConnection": "Data Source=simpleshop.db" }
```

and are read in code with `builder.Configuration`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

The app runs in an **environment** — `Development`, `Staging` or `Production` — set by the
`ASPNETCORE_ENVIRONMENT` variable (see `Properties/launchSettings.json`).
`appsettings.{Environment}.json` is loaded **on top of** `appsettings.json` and overrides it.
That's why in Development we see SQL logs and Swagger, and in Production we don't.

Later sources override earlier ones:
`appsettings.json` → `appsettings.Development.json` → environment variables → command-line args.

> ⚠ Never put real passwords or API keys in `appsettings.json` (it goes into Git). Use
> **User Secrets** (`dotnet user-secrets`) in development, and environment variables or a vault
> (e.g. Azure Key Vault) in production.

### 6.11 Logging

Any class can ask for an `ILogger<T>` through DI:

```csharp
_logger.LogInformation("Created product {ProductId} '{ProductName}'", product.Id, product.Name);
```

- **Levels:** `Trace` < `Debug` < `Information` < `Warning` < `Error` < `Critical`. The
  `Logging:LogLevel` section in `appsettings.json` sets the minimum level shown per category.
- **Structured logging:** use `{Placeholders}`, not `$"string interpolation"`. Tools like
  Seq, Elastic or Application Insights then let you search by `ProductId = 4`.

### 6.12 Error handling and status codes

All errors are handled in **one place**: `Middleware/ExceptionHandlingMiddleware.cs`.

```
Service throws ConflictException  ──▶ middleware catches it ──▶ 409 Conflict + clear message
Something unexpected crashes      ──▶ middleware catches it ──▶ 500 + generic message
                                                                 (full details go to the log only)
```

Status codes used by this API:

| Code | Meaning | When in this project |
|---|---|---|
| **200** OK | success | GET, PUT |
| **201** Created | new item created | POST |
| **204** No Content | success, empty body | DELETE |
| **400** Bad Request | invalid input | validation failed |
| **404** Not Found | item doesn't exist | unknown `id` |
| **409** Conflict | breaks a business rule | duplicate product name |
| **500** Internal Server Error | a bug on our side | any unhandled exception |

All error bodies use the standard **Problem Details** JSON format (`status`, `title`, `detail`).

---

## 7. API reference

Base URL: `http://localhost:5080`

| Method | URL | Body | Success | Possible errors |
|---|---|---|---|---|
| `GET` | `/` | – | 200 – info message | – |
| `GET` | `/api/products` | – | 200 – list of products | – |
| `GET` | `/api/products?search=mouse` | – | 200 – filtered list | – |
| `GET` | `/api/products/{id}` | – | 200 – one product | 404 |
| `POST` | `/api/products` | `CreateProductRequest` | 201 – created product | 400, 409 |
| `PUT` | `/api/products/{id}` | `UpdateProductRequest` | 200 – updated product | 400, 404, 409 |
| `DELETE` | `/api/products/{id}` | – | 204 – no body | 404 |

Request body example (`POST` and `PUT`):

```json
{
  "name": "USB-C Cable",
  "description": "1 meter, fast charging",
  "price": 9.99,
  "stock": 100
}
```

Interactive docs: <http://localhost:5080/swagger> · Raw OpenAPI JSON: <http://localhost:5080/openapi/v1.json>

---

## 8. Exercises: learn by doing

Do them in order. Each one touches more layers than the last.

1. **Add a new endpoint** `GET /api/products/in-stock` that returns only products with `Stock > 0`.
   *(Touches: interface, service, controller.)*
2. **Add a business rule:** a product that still has stock can't be deleted → return `409 Conflict`.
   *(Hint: throw `ConflictException` in `DeleteAsync`.)*
3. **Add a field** `Category` (string) to products.
   *(Touches: entity, both request DTOs, response DTO, `ToResponse`, the service's create/update,
   the seed data. Then delete `simpleshop.db` so it's recreated with the new column.)*
4. **Add a small endpoint** `PATCH /api/products/{id}/stock` with body `{ "quantity": -3 }` that
   adds/removes stock but never lets it go below 0.
5. **Add a second entity** `Category` with its own table and a one-to-many relationship
   (one category has many products). This is how most real data models look.
6. **Rewrite one endpoint as a Minimal API** in `Program.cs`
   (like the `app.MapGet("/", ...)` example) to see the other style of writing endpoints.

---

## 9. What bigger, real-world projects add

This project shows the core. Here is what you'll meet next in professional codebases:

| Topic | What it is |
|---|---|
| **EF Core Migrations** | Versioned database schema changes (instead of `EnsureCreated`) |
| **SQL Server / PostgreSQL** | Real production databases (just a different `UseSqlServer`/`UseNpgsql` line + package) |
| **Authentication & Authorization** | Who are you? (JWT tokens, `[Authorize]` attribute, ASP.NET Core Identity) |
| **Tests** | xUnit/NUnit unit tests for services, integration tests with `WebApplicationFactory` |
| **Pagination** | `?page=2&pageSize=20` so you never return 1 million rows |
| **Options pattern** | Binding config sections to typed classes (`IOptions<T>`) |
| **CancellationToken** | Stop database work when the client disconnects |
| **FluentValidation, Mapster/AutoMapper** | Libraries for validation and mapping |
| **Clean Architecture** | Splitting into several projects: `Api`, `Application`, `Domain`, `Infrastructure` — same layers as here, just in separate projects |
| **CORS** | Allowing a frontend on another domain to call your API |
| **Docker, CI/CD, health checks** | Packaging, automatic build/test/deploy, monitoring |

---

## 10. Glossary

| Term | Meaning |
|---|---|
| **.NET** | Microsoft's free, cross-platform developer platform (runtime + libraries + tools) |
| **ASP.NET Core** | The part of .NET for building web apps and APIs |
| **Kestrel** | The web server built into ASP.NET Core |
| **SDK** | Software Development Kit — tools to build .NET apps (`dotnet build`, `dotnet run`, ...) |
| **NuGet** | .NET's package manager (like npm for JavaScript or pip for Python) |
| **Solution (.sln / .slnx)** | A file that groups several projects together |
| **REST API** | An API where URLs are *resources* (`/api/products`) and HTTP methods are *actions* |
| **Endpoint** | One URL + HTTP method combination, e.g. `GET /api/products/{id}` |
| **CRUD** | Create, Read, Update, Delete — the four basic data operations |
| **JSON** | The text format used to send data: `{ "name": "Mouse", "price": 24.99 }` |
| **Controller** | A class that groups related endpoints |
| **Action** | One method in a controller that handles one endpoint |
| **Middleware** | Code that runs for every request, in a chain (the "pipeline") |
| **DI (Dependency Injection)** | The framework creates objects and passes them to constructors for you |
| **Entity** | A C# class that maps to a database table |
| **DTO** | Data Transfer Object — a class describing the JSON going in or out of the API |
| **ORM** | Object-Relational Mapper — converts between objects and database tables (EF Core) |
| **DbContext** | EF Core's class representing a session with the database |
| **LINQ** | C# query syntax (`Where`, `Select`, `OrderBy`, ...) that EF Core turns into SQL |
| **Migration** | A versioned, code-based change to the database schema |
| **OpenAPI / Swagger** | A standard description of an API, and the UI that displays it |
| **Problem Details** | The standard JSON format for API errors (RFC 9457) |

---

## 11. Useful commands

Run these from the `SimpleShop.Api` folder.

| Command | What it does |
|---|---|
| `dotnet run` | Build and start the app |
| `dotnet watch` | Start the app and **auto-restart on every code change** (great while learning) |
| `dotnet build` | Compile only (shows errors without running) |
| `dotnet clean` | Delete the build output |
| `dotnet add package <Name>` | Install a NuGet package |
| `dotnet list package` | Show installed packages |
| `dotnet new webapi -n MyApi --use-controllers` | Create a brand-new Web API project from the template |
