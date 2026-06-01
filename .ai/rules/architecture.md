# Clean Architecture Principles for .NET Microservices

This document outlines the Clean Architecture principles tailored for .NET microservices,
emphasizing separation of concerns, maintainability, and testability.
It serves as a guide for structuring projects to enhance scalability and adaptability.

## High level Solution Structure Overview

### Project Structure

1. **Domain Layer**: Contains business logic and domain entities.
2. **Application Layer**: Manages application logic and orchestrates operations.
3. **Infrastructure Layer**: Handles data access, external services, and frameworks.
4. **API Layer**: Exposes endpoints and handles HTTP requests.

### Key Principles

- **Separation of Concerns**: Each layer has distinct responsibilities.
- **Dependency Inversion**: High-level modules should not depend on low-level modules.
- **Repository Pattern**: Abstracts data access to promote testability.
- **Service Layer**: Encapsulates business logic, ensuring clear separation from API controllers.
- **Middleware Integration**: Implements consistent error management and logging.
- **Testing Practices**: Emphasizes unit and integration tests for code quality.

## Core Layers

### 1. Domain Layer (Core)

**Purpose:** The Domain layer contains pure business logic and represents the ubiquitous language of the business.

**Key Characteristics:**

- No dependencies on other layers or external frameworks
- Pure business objects with no infrastructure concerns
- Contains entities, value objects, domain events, and business rules

**Standard Folder Structure:**

```
Domain/
├── Entities/           # Business entities (plural folder name)
├── ValueObjects/       # Immutable value objects
├── Aggregates/			# Aggreggate roots 	
├── Enums/              # Domain-specific enumerations
├── Exceptions/         # Domain-specific exceptions
├── Events/             # Domain events (if using event-driven architecture)
├── Interfaces/         # Cross cutting Domain interfaces (if used). Not repository interfaces, these should be in Application layer.
└── Specifications/     # Business rules specifications (if used)
```

**Naming Conventions:**

- Entities: Singular nouns (e.g., `Product`, `Customer`)
- ValueObjects: Descriptive names (e.g., `Money`, `EmailAddress`)
- Interfaces: Prefixed with `I` (e.g., `IProductRepository`)
- Events: Past tense verbs (e.g., `ProductCreatedEvent`)

#### Example
```csharp
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { } // EF Core

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
		{
            throw new DomainException("CustomerId is required");
		}
        CustomerId = customerId;
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");

        _items.Add(new OrderItem(productId, quantity));
    }
}

```

### 2. Application Layer

**Purpose:** Contains application-specific business rules, use cases, and orchestrates data flow.

**Key Characteristics:**

- Depends only on Domain layer
- Contains use cases (application business rules)
- Defines interfaces for external concerns (repositories, services)
- Uses DTOs for data transfer between layers

**Standard Folder Structure:**

```
Application/
├── Features/
│ 	├──{FeatureName}/
│ 	│ 	├── Commands/           # Write operations (CQRS)
│ 	│ 	├── Queries/            # Read operations (CQRS)
│ 	│ 	└── DTOs/               # Data Transfer Objects (plural)
├── Interfaces/         # Application service interfaces
│   ├── Repositories/   # Repository interfaces
│   └── Services/       # Service interfaces
├── Profiles/           # AutoMapper profiles (only if you need to use automapper)
├── Mappings/           # Extension classes fro mappings (default to use extension classes for mapping instead of automapper)
├── Validators/         # Input validation (FluentValidation)
├── EventHandlers/      # Domain event handlers
└── Pipiline/           # Wolverine pipelines (logging, validation, etc.)
```

**Naming Conventions:**

- Commands: Verb + noun + "Command" (e.g., `CreateProductCommand`)
- Queries: Verb + noun + "Query" or "Get" + noun + "Query" (e.g., `GetProductQuery`)
- DTOs: Descriptive names ending with "Dto" or "Response"/"Request" (e.g., `ProductDto`, `CreateProductRequest`)
- Interfaces: Descriptive names with "I" prefix (e.g., `IProductService`)

### 3. Infrastructure Layer

**Purpose:** Implements interfaces from Application layer and handles external concerns.

**Key Characteristics:**

- Depends on Domain and Application layers
- Contains technical details (databases, APIs, file systems)
- Implements repository interfaces and external service integrations
- No business logic - only technical implementation details

**Standard Folder Structure:**

```
Infrastructure/
├── Persistence/        # Data access implementations
│   ├── Context/        # DbContext and related configurations
│   ├── Migrations/     # Database migrations
│   ├── Repositories/   # Repository implementations
│   └── Configurations/ # Entity configurations (EF Core, etc.)
├── ExternalServices/   # Third-party service integrations
├── Logging/            # Logging implementations
├── Caching/            # Caching implementations
├── Configuration/      # Configuration providers
└── Mail/               # Email service implementations
```

**Naming Conventions:**

- Repository classes: Entity name + "Repository" (e.g., `ProductRepository`)
- DbContext: Solution-specific name + "DbContext" (e.g., `RefactorDbContext`)
- Service implementations: Interface name without "I" prefix (e.g., `EmailService` for `IEmailService`)
- Configuration classes: Descriptive names ending with "Options" or "Settings" (e.g., `SmtpSettings`)


#### Example
```csharp
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Order order)
    {
        var entity = order.ToEntity(); // mapping extension
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        var entity = await _db.Orders.Include(o => o.Items)
                                     .FirstOrDefaultAsync(o => o.Id == id);
        return entity?.ToDomain();
    }
}

```

### 4. Presentation Layer

**Purpose:** Handles user interaction and system entry points (API, UI, etc.).

**Key Characteristics:**

- Depends on application and domain layers.
- It has a dependency on the infrastructure layer, but it is used only for the DI  setup.  No concepts or classes should leak from the infrastructure layer here.
- Contains only presentation concerns (controllers, views, middleware)
- Translates external requests to application layer commands/queries
- Handles cross-cutting concerns like authentication, authorization, and validation at the boundary



**Standard Folder Structure (Web API):**

```
Presentation/ or Api/
├── Controllers/        # API controllers (plural)
├── Middleware/         # Custom middleware
├── Filters/            # Action filters, exception filters
├── Hubs/               # SignalR hubs (if used)
├── Models/             # View models or input models specific to presentation
│	├── Requests
│	└── Responses
├── Properties/         # Launch settings, profiles
├── Hubs/               # SignalR hubs
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
Presentation Layer
        ↓
Application Layer
        ↓
Domain Layer
        ↑
Infrastructure Layer
```

**Key Principles:**

1. **Dependency Direction:** All dependencies point inward toward the Domain layer
2. **Abstractions Ownership:** Inner layers define abstractions; outer layers implement them
3. **No Circular Dependencies:** Layers cannot depend on each other laterally
4. **Stable Dependencies:** Depend on more stable things (Domain) rather than volatile ones (Infrastructure)

## Folder Naming Conventions

### General Rules

- **Plural Folder Names:** Use plural for collections of similar items (`Entities`, `Commands`, `Queries`)
- **Singular for Specific:** Use singular for unique items or when representing a concept (`Middleware`, `Filter`)
- **Group by Feature:** When appropriate, group by business feature rather than technical type (alternative structure)
- **Consistent Casing:** Use PascalCase for all folder and file names
- **Descriptive Names:** Folders should clearly indicate their purpose

## Other Concerns

### 1. DTO Clarification Across Layers
DTOs appear in three layers, each with a different purpose.
---
3.1 API DTOs (Request/Response Models)
Location: Presentation layer
Purpose: Define the public API contract  
Notes: May flatten or reshape data for clients

Example:
```csharp
public record CreateOrderRequest(Guid CustomerId, List<OrderItemRequest> Items);
public record OrderResponse(Guid Id, decimal Total, string CustomerName);
```
---
3.2 Application DTOs
Location: Application layer
Purpose: Represent use-case outputs  
Notes: Internal only — not exposed to API

Example:
```csharp
public record OrderDto(Guid Id, decimal Total, CustomerDto Customer);
```

3.3 Persistence DTOs (ORM Entities)
Location: Infrastructure layer
Purpose: Represent database tables
Notes: Must never leak outside Infrastructure

Example:
```csharp
public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItemEntity> Items { get; set; } = new();
}

```

### 2. Mapping

**Use autommapper only if you have to map dymanic objects!**

- Generally favor manuall mapping. Create extenions for mapping. These extenisions should be in a separate folder in Application layer.

Request flow
```
API Layer
   ↓ (maps request → command)
Application Layer
   ↓ (maps command → domain entity)
Domain Layer
   ↓ (business logic)
Application Layer
   ↓ (calls repository interface)
Infrastructure Layer
   ↓ (maps domain entity → persistence entity)
Database
```

Response flow
```
Infrastructure Layer
   ↓ (maps persistence entity → to applicationDto) 	If projection OR
   ↓ (maps persistence entity → to domain entity) 	If simple case
Application Layer
   ↓ (passes applicationDto or domain entity to API Layer)
API Layer
   ↓ (maps applicationDto or domain entity to response model)
```

- In case you have to use Autommaper,  create profiles in Application layer


### 3. Validation

* Input validation in Application layer (Validators folder)

- Uses FluentValidation
- Can be implemented as  pipeline behavior

### 4. Error Handling

- Domain layer throws domain-specific exceptions
- Application layer handles validation errors
- Presentation layer maps exceptions to appropriate HTTP responses
- Consider using problem details specification for APIs

### 5. Logging

- Used across all layers via dependency injection

## Project Dependencies Template

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

## Implementation Guidelines

### 1. Entity Design

- Entities should have protected/setterless constructors
- Collections should be initialized as `ICollection<T>` or `IReadOnlyCollection<T>`
- Use private setters for properties
- Encapsulate business logic within entities when possible
- Use Guid.CreateVersion7() for generating IDs instead of int or Guid.NewGuid() to avoid performance issues with sequential GUIDs in databases
Example:

```csharp
public class Application
{
    public  Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name
    {
        get;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Application name is required.", nameof(Name));
            }

            field = value;
        }
    } = null!;

    public string? Comments { get; private set; }

    // Private constructor for EF Core
    private Application() { }

    public Application(Guid id, string name, string? comments = null)
    {
        Id = id;
        Name = name;
        Comments = comments;
    }
}
```


### 2. Repository Pattern

- Define repository interfaces in Application layer
- Implement in Infrastructure layer
- Methods should return domain entities or DTOs (never expose EF Core entities directly)
- Consider generic base repository for common operations

### 3. CQRS with wolverinefx

**Always** use wolverinefx MediatR for handling commands and queries in Application layer. This promotes separation of concerns and keeps controllers thin.
Documentation: https://wolverinefx.net/guide/http/mediator.html
- Commands modify state (returns void or entity ID)
- Queries return data (never modify state)
- Handlers contain single use case logic
- Use pipeline behaviors for cross-cutting concerns (validation, logging, caching)

### 4. Dependency Injection

- Register services in each layer's DependencyInjection class
- Use extension methods for clean registration (`AddInfrastructure`, `AddApplication`)
- Follow lifetime scoping principles (Transient, Scoped, Singleton)

### 5. Configuration
- Strongly-typed options pattern
- Validation of configuration at startup
- Separate configuration files per environment

### 6. Records
Use records for:
- Value objects
- Complex types
- DTOs
- Query projections
- Commands
- Queries


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

## Conclusion

Following these principles will help create a maintainable and scalable architecture for .NET microservices.
