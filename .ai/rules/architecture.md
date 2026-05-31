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

**Purpose:** Contains enterprise-wide business rules and entities that are independent of any specific application.

**Key Characteristics:**

- No dependencies on other layers or external frameworks
- Pure business objects with no infrastructure concerns
- Contains entities, value objects, domain events, and business rules

**Standard Folder Structure:**

```
Domain/
├── Entities/           # Business entities (plural folder name)
├── ValueObjects/       # Immutable value objects
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
├── Commands/           # Write operations (CQRS)
├── Queries/            # Read operations (CQRS)
├── DTOs/               # Data Transfer Objects (plural)
├── Interfaces/         # Application service interfaces
│   ├── Repositories/   # Repository interfaces
│   └── Services/       # Service interfaces
├── Profiles/           # AutoMapper profiles (only if you need to use automapper)
├── Mappings/           # Extension classes fro mappings (default to use extension classes for mapping instead of automapper)
├── Validators/         # Input validation (FluentValidation)
├── EventHandlers/      # Domain event handlers
└── Behaviors/          # MediatR pipeline behaviors (logging, validation, etc.)
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

### 4. Presentation Layer

**Purpose:** Handles user interaction and system entry points (API, UI, etc.).

**Key Characteristics:**

- Depends on all inner layers
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
├── Properties/         # Launch settings, profiles
├── Hubs/               # SignalR hubs
└── HealthChecks/       # Health check endpoints
```

**Naming Conventions:**

- Controllers: Entity name + "Controller" (e.g., `ProductsController`)
- Actions: HTTP verb + descriptive name (e.g., `GetById`, `CreateProduct`)
- Middleware: Descriptive name + "Middleware" (e.g., `LoggingMiddleware`)
- Filters: Descriptive name + "Filter" (e.g., `ValidationFilter`)
- Models: Descriptive names indicating purpose (e.g., `ProductViewModel`, `LoginRequest`)

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

### Alternative Feature-Based Structure

For larger applications, consider grouping by feature within layers:

```
Application/
├── Features/
│   ├── Products/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── DTOs/
│   └── Orders/
│       ├── Commands/
│       ├── Queries/
│       └── DTOs/
└── Shared/             # Cross-cutting concerns
```

## Cross-Layer Concerns

### 1. Data Transfer Objects (DTOs)

- Located in Application layer
- Used for data exchange between layers
- Never expose domain entities directly to outer layers
- Naming: Descriptive + "Dto" or context-specific ("Request"/"Response")

### 2. Mapping

**Use autommapper only if you have to map dymanic obkects!**

- Generally favor manuall mapping. Create extenions for mapping. These extenisions should be in a separate folder in Application layer.

- In case you have to use Autommaper,  create profiles in Application layer

### 3. Validation

* Input validation in Application layer (Validators folder)

- Uses FluentValidation or similar
- Can be implemented as MediatR pipeline behaviors

### 4. Error Handling

- Domain layer throws domain-specific exceptions
- Application layer handles validation errors
- Presentation layer maps exceptions to appropriate HTTP responses
- Consider using problem details specification for APIs

### 5. Logging

- Abstraction (ILogger) defined in Application or Domain
- Implementation in Infrastructure layer
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
    → [SolutionName].Infrastructure
```

## Implementation Guidelines

### 1. Entity Design

- Entities should have protected/setterless constructors
- Collections should be initialized as `ICollection<T>` or `IReadOnlyCollection<T>`
- Use private setters for properties that should only be changed through methods
- Encapsulate business logic within entities when possible

### 2. Repository Pattern

- Define repository interfaces in Application layer
- Implement in Infrastructure layer
- Methods should return domain entities or DTOs (never expose EF Core entities directly)
- Consider generic base repository for common operations

### 3. CQRS with MediatR
**Always** use MediatR for handling commands and queries in Application layer. This promotes separation of concerns and keeps controllers thin.
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
