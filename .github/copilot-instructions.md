# WebApiShop Copilot Instructions

## Project Overview
WebApiShop is an e-commerce REST API built with ASP.NET Core 9.0 and SQL Server. It provides endpoints for managing users, products, categories, orders, and ratings with role-based access control and comprehensive logging.

## Tech Stack
- **Framework:** ASP.NET Core 9.0
- **Database:** SQL Server with Entity Framework Core 9.0.11
- **API Documentation:** OpenAPI/Swagger 9.0.6
- **Dependency Injection:** Built-in Microsoft DI
- **Object Mapping:** AutoMapper 12.0.1
- **Password Validation:** zxcvbn-core 8.0.0
- **Logging:** NLog 6.1.0 with ASP.NET Core integration
- **Testing:** xUnit 2.9.2 with Moq 4.20.72 and Moq.EntityFrameworkCore 9.0.0

## Architecture & Project Structure

### Layered Architecture (Bottom to Top)
```
DTOs (Request/Response objects)
    ↓
Controllers (HTTP endpoints & validation)
    ↓
Services (Business logic, validation, transformation)
    ↓
Repositories (Data access layer)
    ↓
Entities (Domain models)
    ↓
ApiDBContext (EF Core DbContext)
    ↓
SQL Server Database
```

### Folder Organization
- **DTOs/** - Data Transfer Objects (UserDTO, ProductDTO, etc.) as C# records
- **Entities/** - EF Core entity models representing database tables
- **Repositories/** - Data access implementations with CRUD operations
- **Services/** - Business logic layer handling validation and DTO transformations
- **WebApiShop/Controllers/** - API endpoints following REST conventions
- **WebApiShop/Middleware/** - Custom middleware (ErrorHandlingMiddleware, RatingMiddleware)
- **TestProject/** - Unit and integration tests

## Build & Run Instructions

### Prerequisites
- .NET 9.0 SDK installed
- SQL Server with database "ApiDB"
- Connection string in `appsettings.json`: `"Home": "Data Source = ATARA; Initial Catalog = ApiDB; Integrated Security = True; Trust Server Certificate=True"`

### Build the Solution
```powershell
dotnet build WebApiShop-AI.sln
```

### Run the API
```powershell
cd WebApiShop
dotnet run
```
API will start on `https://localhost:5001` (or configured port). Swagger UI available at `/swagger/ui`.

### Run Tests
```powershell
dotnet test TestProject/TestProject.csproj
```

## Key Design Patterns & Conventions

### 1. Async/Await Throughout
- All data access and I/O operations use `async Task`
- Controllers and services are async
- Never use `.Result` or `.Wait()`

### 2. Repository Pattern
- Each entity has an `IXRepository` interface and `XRepository` implementation
- Repositories handle all database queries
- Include related data with `.Include()` and `.ThenInclude()` for eager loading

### 3. Dependency Injection (DI)
- All dependencies injected via constructors
- Registered in `Program.cs` as scoped services
- Example: `builder.Services.AddScoped<IUserRepository, UserRepository>();`

### 4. Validation
- **Email uniqueness:** Use `UserWithSameEmail(email, id)` - returns `true` if valid (no duplicate)
- **Password strength:** `IsPasswordStrong(password)` returns `true` if score >= 2
- Validation occurs in both services and controllers

### 5. AutoMapper
- Configure all mappings in `WebApiShop/AutoMapper.cs`
- Use `.ReverseMap()` for bidirectional mapping
- Map at service layer, not in controllers

### 6. Logging
- Use injected `ILogger<T>` in controllers
- Log important actions: logins, modifications, errors
- NLog configured for file output and email alerts (see `nlog.config`)

### 7. Error Handling
- `ErrorHandlingMiddleware` catches unhandled exceptions
- Returns HTTP 500 and logs full exception with stack trace
- Controllers return appropriate HTTP status codes (400, 401, 404, 500)

## HTTP Response Patterns

| Scenario | Status Code | Pattern |
|----------|-------------|---------|
| Success with data | 200 OK | `return Ok(data);` |
| Resource created | 201 Created | `return CreatedAtAction(nameof(Get), new { id = item.Id }, item);` |
| Success, no content | 204 No Content | `return NoContent();` |
| Bad input/validation | 400 Bad Request | `return BadRequest("error message");` |
| Unauthorized | 401 Unauthorized | `return Unauthorized();` |
| Not found | 404 Not Found | `return NotFound();` |
| Server error | 500 | Caught by middleware, no explicit return needed |

## Common Mistakes to Avoid
- ❌ Don't create new DbContext instances; always inject and use singleton instance
- ❌ Don't forget to `.SaveChangesAsync()` after updates
- ❌ Don't map DTOs in repositories; keep repositories entity-only
- ❌ Don't skip AutoMapper configuration; manually map in services, not controllers
- ❌ Don't call `ToListAsync()` prematurely; eager load with EF's `.Include()` instead
