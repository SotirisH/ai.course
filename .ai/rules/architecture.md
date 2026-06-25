# Clean Architecture Principles for .NET Microservices

**Target Framework:** .NET 10+ / C# 14+

This document outlines the Clean Architecture principles tailored for .NET microservices,
emphasizing separation of concerns, maintainability, and testability.
It serves as a guide for structuring projects to enhance scalability and adaptability.

## High level Solution Structure Overview

### Project Structure

1. **Domain Layer**: Contains domain exceptions, enums, and domain events.
2. **Application Layer**: Manages application logic, orchestrates operations, and defines DTOs for cross-layer communication.
3. **Infrastructure Layer**: Handles data access, external services, and frameworks. Communicates with the Application layer exclusively via DTOs.
4. **API Layer**: Exposes endpoints and handles HTTP requests.

### Key Principles

- **Separation of Concerns**: Each layer has distinct responsibilities.
- **Dependency Inversion**: High-level modules should not depend on low-level modules.
- **Repository Pattern**: Abstracts data access to promote testability. Repositories accept and return DTOs — never domain entities.
- **Service Layer**: Encapsulates business logic, ensuring clear separation from API controllers.
- **Middleware Integration**: Implements consistent error management and logging.
- **Testing Practices**: Emphasizes unit, integration, and E2E tests for code quality.

### Testing Structure

Tests are organized in a separate `tests/` folder at the solution level, with clear separation by test type.
See [testing-strategy.md](testing-strategy.md) for the canonical testing reference, including test layer definitions, folder structure, tools, and naming conventions.

**Testing Tools:**

- **xUnit**: Primary testing framework for all test types
- **Shouldly**: Fluent assertion library for readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing**: For E2E API integration tests
- **Testcontainers**: For integration tests requiring real PostgreSQL instances
- **MockHttp**: For mocking external HTTP API calls in unit tests

## Core Layers

### 1. Domain Layer (Core)

**Purpose:** The Domain layer contains foundational domain concepts such as exceptions, enums, and domain events.

**Key Characteristics:**

- No dependencies on other layers or external frameworks
- Contains domain exceptions, enums, and domain events
- Does **not** contain entities, value objects, or aggregates — cross-layer communication uses DTOs defined in the Application layer

**Standard Folder Structure:**

```
Domain/
├── Enums/              # Domain-specific enumerations
├── Exceptions/         # Domain-specific exceptions
├── Events/             # Domain events (if using event-driven architecture)
├── Interfaces/         # Cross cutting Domain interfaces (if used). Not repository interfaces, these should be in Application layer.
└── Specifications/     # Business rules specifications (if used)
```

**Naming Conventions:**

- Exceptions: Descriptive name ending with "Exception" (e.g., `DomainException`, `InvalidStateException`)
- Enums: Singular nouns (e.g., `ApplicationStatus`, `OrderState`)
- Interfaces: Prefixed with `I` (e.g., `IBizzRule`, `IDomainService`)
- Events: Past tense verbs (e.g., `ApplicationCreatedEvent`)

### 2. Application Layer

**Purpose:** Contains application-specific business rules, use cases, and orchestrates data flow.

**Key Characteristics:**

- Depends only on Domain layer
- Contains use cases (application business rules)
- Defines interfaces for external concerns (repositories, services)
- All communication with the Infrastructure layer uses DTOs — domain entities, value objects, and aggregates are never passed across layer boundaries
- DTOs are the contract between Application and Infrastructure

**Standard Folder Structure:**

```
Application/
├── Features/
│   ├──{FeatureName}/
│   │   ├── Commands/          # Write operations (CQRS)
│   │   ├── Queries/           # Read operations (CQRS)
│   │   └── DTOs/              # DTOs used only by this feature
├── Interfaces/         # Application service interfaces
│   ├── Repositories/   # Repository interfaces (accept and return DTOs)
│   └── Services/       # Service interfaces
├── Mappers/            # Extension classes for mappings (default to use extension classes for mapping instead of automapper)
├── Profiles/           # AutoMapper profiles (only if you need to use automapper)
├── Validators/         # Input validation (FluentValidation)
├── DTOs/
│   ├── Internal/       # DTOs shared across multiple features (internal to the application)
│   └── External/       # Request/response models for calling external APIs from the Application layer
└── Pipeline/           # Wolverine pipelines (logging, validation, etc.)
```

**Naming Conventions:**

- Commands: Verb + noun + "Command" (e.g., `CreateProductCommand`)
- Queries: Verb + noun + "Query" or "Get" + noun + "Query" (e.g., `GetProductQuery`)
- Handlers: Command/Query name + "Handler" (e.g., `CreateProductCommandHandler`)
- DTOs: Descriptive names ending with "Dto" or "Response"/"Request" (e.g., `ProductDto`)
- Interfaces: Descriptive names with "I" prefix (e.g., `IProductService`)

### 3. Infrastructure Layer

**Purpose:** Implements interfaces from Application layer and handles external concerns.

**Key Characteristics:**

- Depends on Domain and Application layers
- Contains technical details (databases, APIs, file systems)
- Implements repository interfaces and external service integrations
- No business logic - only technical implementation details
- Receives DTOs from and returns DTOs to the Application layer. Internally maps between persistence entities and DTOs.
- Uses `Fluent API configuration` instead of DataAnnotations for the EF Core entity models

**Standard Folder Structure:**

```
Infrastructure/
├── Persistence/        # Data access implementations
│   ├── Context/        # DbContext and related configurations
│   ├── Entities/       # EF Core entity models (database table representations)
│   ├── Migrations/     # Database migrations
│   ├── Repositories/   # Repository implementations
│   └── Configurations/ # Entity configurations (EF Core, etc.)
├── ExternalServices/   # Third-party service integrations
├── Mappers/            # Extension classes for mappings
├── Logging/            # Logging implementations
├── Caching/            # Caching implementations
├── Configuration/      # Configuration providers
└── Mail/               # Email service implementations
```

**Naming Conventions:**

- Repository classes: Entity name + "Repository" (e.g., `ProductRepository`)
- DbContext: Solution-specific name + "DbContext" (e.g., `RefactorDbContext`)
- Entity classes: Entity name (e.g., `Orders`, `Products`) . It should match the name of the database table.
- Service implementations: Interface name without "I" prefix (e.g., `EmailService` for `IEmailService`)
- Configuration classes: Descriptive names ending with "Options" or "Settings" (e.g., `SmtpSettings`)

#### Example

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<OrderDto> AddAsync(CreateOrderDto dto)
    {
        var entity = dto.ToEntity(); // mapping extension
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();
        return entity.ToDto(); // map persistence entity back to DTO
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var entity = await _db.Orders.Include(o => o.Items)
                                     .FirstOrDefaultAsync(o => o.Id == id);
        return entity?.ToDto(); // map persistence entity to DTO
    }
}
```

### 4. Presentation Layer

**Purpose:** Handles user interaction and system entry points (API, UI, etc.).

**Key Characteristics:**

- Depends on application and domain layers.
- It has a dependency on the infrastructure layer, but it is used only for the DI setup. No concepts or classes should leak from the infrastructure layer here.
- Contains only presentation concerns (controllers, views, middleware)
- Translates external requests to application layer commands/queries
- Handles cross-cutting concerns like authentication, authorization, and validation at the boundary
- All the requests and responses should be defined in this layer.
- The API should not expose application DTOs directly. Instead, it should define its own request and response models that are specific to the API contract.

**Standard Folder Structure (Web API):**

```
Presentation/ or Api/
├── Controllers/        # API controllers (plural)
├── Middleware/         # Custom middleware
├── Filters/            # Action filters, exception filters
├── Hubs/               # SignalR hubs (if used)
├── Models/             # View models or input models specific to presentation
│   ├── Requests
│   └── Responses
├── Properties/         # Launch settings, profiles
├── Mappers/            # Extension classes for mappings 
└── HealthChecks/       # Health check endpoints
```

**Naming Conventions:**

- Controllers: Entity name + "Controller" (e.g., `ProductsController`)
- Actions: HTTP verb + descriptive name (e.g., `GetById`, `CreateProduct`)
- Middleware: Descriptive name + "Middleware" (e.g., `LoggingMiddleware`)
- Filters: Descriptive name + "Filter" (e.g., `ValidationFilter`)
- Models: Descriptive names indicating purpose (e.g., `ProductCreateRequest`, `ProductCreateResponse`)

## Dependency Rules

```
Presentation Layer -> Domain Layer
        ↓
Application Layer  -> Domain Layer
        ↑
Infrastructure Layer  -> Domain Layer
```

**Project Dependencies Template**

```
[SolutionName].Domain
    → No dependencies

[SolutionName].Application
    → [SolutionName].Domain

[SolutionName].Infrastructure
    → [SolutionName].Domain
    → [SolutionName].Application

[SolutionName].Presentation
    → [SolutionName].Domain
    → [SolutionName].Application
    → [SolutionName].Infrastructure *ONLY for DI purposes
```

**Key Principles:**

1. **Dependency Direction:** All dependencies point inward toward the Domain layer
2. **Abstractions Ownership:** Inner layers define abstractions; outer layers implement them
3. **No Circular Dependencies:** Layers cannot depend on each other laterally
4. **Stable Dependencies:** Depend on more stable things (Domain) rather than volatile ones (Infrastructure)

---
## Modeling and Mapping
### DTO Design
- DTOs are plain data containers — they carry data, not behavior
- Use `record` types for immutability and value-based equality
- Keep DTOs focused on a single use case or data transfer need
- Do not put validation logic in DTOs — use FluentValidation validators instead
- Use `Guid.CreateVersion7()` for generating IDs to avoid performance issues with sequential GUIDs in databases

Example:

```csharp
public sealed record ApplicationDto
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Name { get; init; }
    public string? Comments { get; init; }
}
```

### API Models (Request/Response Models)

Location: Presentation layer
Purpose: Define the public API contract
Notes: May flatten or reshape data for clients

Example:

```csharp
public sealed record CreateApplicationRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

### Application DTOs

Location: Application layer
Purpose: Represent use-case inputs and outputs. DTOs are the sole contract between the Application and Infrastructure layers.
Notes: Internal only — not exposed to API. Repositories accept and return DTOs, never domain entities.

**DTO placement rules:**

- **Feature DTOs** (`Features/{FeatureName}/DTOs/`) — DTOs used exclusively by a single feature.
- **Internal DTOs** (`DTOs/Internal/`) — DTOs shared across multiple features within the application. Move a DTO here when more than one feature needs it.
- **External DTOs** (`DTOs/External/`) — Request/response models for calls to external APIs made from the Application layer.

Example:

```csharp
public record OrderDto 
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
});
```

### Persistence DTOs (ORM Entities)

Location: Infrastructure layer
Purpose: Represent database tables
Notes: Must never leak outside Infrastructure. Mapped to/from application DTOs at the repository boundary.

Example:

```csharp
public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItemEntity> Items { get; set; } = new();
}

```

## Mapping
Generally favor manual mapping. **Always** create and use extensions for mapping. These extensions should be in a separate folder in each layer.
**Note:** Use AutoMapper only if you have to map dynamic objects!

---
Request flow
```
API Layer
   ↓ (maps request → command)
Application Layer
   ↓ (handler logic, prepares DTO)
   ↓ (calls repository interface with DTO)
Infrastructure Layer
   ↓ (maps DTO → persistence entity)
Database
```
---
Response flow

```
Infrastructure Layer
   ↓ (maps persistence entity → DTO)
Application Layer
   ↓ (passes DTO to API Layer)
API Layer
   ↓ (maps DTO to response model)
```

- In case you have to use AutoMapper, create profiles in Application layer.

---
# Imlementation Guidelines
## API
1. Always use `ActionResult<T>` instead of `IActionResult` on API controller methods
Example:
```csharp
 public async Task<ActionResult<ApplicationResponse>> Create([FromBody] CreateApplicationRequest request, CancellationToken cancellationToken)
```
2. Use route constraints hey prevent unnecessary 404s and improve routing performance.
Example:
```csharp
    [HttpGet("{id:int:min(1)}")]
```
3. Use FromBody, FromQuery, FromRoute explicitly when unclear
Example:
```csharp
public async Task<ActionResult> Search([FromQuery] ProductSearchQuery query)
```

4. Use CancellationToken in all async endpoints
5. Avoid returning raw strings or anonymous objects. Always return typed DTOs or ProblemDetails.
Bad:
```csharp
  return BadRequest("Invalid input");
````
Good:
```csharp
  return Problem("Invalid input", statusCode: 400);
```
6. Use [Microsoft.AspNetCore.Mvc.DefaultApiConventions](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/conventions?view=aspnetcore-10.0#apply-web-api-conventions) 
whenever is possible. They exist to reduce boilerplate like [ProducesResponseType] and to improve Swagger/OpenAPI documentation consistency.
Example:
```csharp
   [HttpPut("{id}")]
   [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
   public IActionResult Update(int id, ProductDto dto)
   {
   ...
   }
````
7. DO NOT USE Minimal APIs! Use Controllers instead.

## Validation
- Input validation in Application layer (Validators folder)
- Uses FluentValidation
- Can be implemented as a pipeline behavior

## Error Handling
- Domain layer throws domain-specific exceptions
- Application layer handles validation errors
- Presentation layer maps exceptions to appropriate HTTP responses
- Consider using problem details specification for APIs

## Logging
- Used across all layers via dependency injection

## Repository Pattern

- Define repository interfaces in Application layer
- Implement in Infrastructure layer
- Repository methods accept and return DTOs — **never** domain entities
- Infrastructure layer internally maps between persistence entities and DTOs
- Consider generic base repository for common operations

## CQRS with wolverinefx

**Always** use Wolverine's built-in mediator functionality for handling commands and queries in the Application layer. This promotes separation of concerns and keeps controllers thin.
Documentation: https://wolverinefx.net/guide/http/mediator.html

- Commands modify state (returns void or entity ID)
- Queries return data (never modify state)
- Handlers contain single use case logic
- Place the command or query object in the same file as the handler. The name of the file should be the same as the name of the command or query handler. Example: `CreateProductCommandHandler.cs` contains both the `CreateProductCommand` class and the `CreateProductCommandHandler` class.
- For validations use `Fluent Validation Middleware`. Info: https://wolverinefx.net/guide/handlers/fluent-validation.html. Example:

```csharp
    // NOTE: WolverineFx 6.x requires UseWolverine() on IHostBuilder, NOT IServiceCollection.
    // Call from an IHostBuilder extension (e.g., in Application layer's AddApplication() method):
    host.UseWolverine(opts =>
    {
        // Apply the validation middleware *and* discover and register
        // Fluent Validation validators
        opts.UseFluentValidation();

        // Discover handlers in this assembly
        opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly);
    });
```

- Initialize wolverine in the Application layer via an `IHostBuilder` extension method (e.g., `AddApplication(this IHostBuilder host)`). This is where handler discovery, middleware (e.g., FluentValidation), and all application-level Wolverine configuration go. Register infrastructure-level Wolverine settings (e.g., service location for DbContext) in the Infrastructure layer via `ConfigureWolverine`:
  Example:

```csharp
host.ConfigureWolverine(options =>
{
    options.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
});
```

- Wolverine by default runs in TypeLoadMode.Dynamic, which compiles handler/middleware code at runtime and WolverineFx no longer ships the runtime compiler. Always include the 'WolverineFx.RuntimeCompilation' NuGet package.

## Dependency Injection
- Register services in each layer's DependencyInjection class
- Use extension methods for clean registration (`AddInfrastructure`, `AddApplication`)
- Follow lifetime scoping principles (Transient, Scoped, Singleton)

## Configuration
- Strongly-typed options pattern
- Validation of configuration at startup
- Separate configuration files per environment

## Records
Use records for:
- DTOs
- Commands
- Queries
- Query projections
- API request/response models


----
# Other
## Benefits of This Structure
1. **Maintainability:** Clear separation makes it easier to locate and modify code
2. **Testability:** Business logic can be tested without UI, database, or web server
3. **Flexibility:** Frameworks and technologies can be swapped with minimal impact
4. **Scalability:** Teams can work on different layers with minimal conflicts
5. **Clarity:** Explicit boundaries prevent accidental coupling

## Adaptation Guidelines
### For Different Project Types
- **Web API:** Use the Presentation layer structure shown above
- **Web MVC:** Replace Controllers with Controllers and Views folders
- **Desktop/WPF/Blazor:** Presentation layer contains Views, ViewModels, Services
- **Mobile:** Presentation layer contains platform-specific UI and ViewModels
- **Background Services:** Presentation layer contains Workers/Hosted Services
- **Microservices:** Each service follows this structure independently

### Technology Stack Variations
- **ORM:** Replace EF Core with Dapper, NHibernate, etc. in Infrastructure/Persistence
- **Messaging:** Add MessageHandlers folder in Application for event-driven architecture
- **Caching:** Implement ICacheService in Infrastructure/Caching
- **Security:** Add Authorization folder in Application/Interfaces for policies

# Conclusion
Following these principles will help create a maintainable and scalable architecture for .NET microservices.
