# Implementation Plan: Application Management

**Ticket:** #001 | **Type:** feature | **Branch:** `feature/001-application-management`

---

## 1. Story Summary

As an administrator, I want to be able to manage applications in the system. This feature provides full CRUD (Create, Read, Update, List) operations on Application records via a RESTful API.

---

## 2. Acceptance Criteria

| # | Criteria |
|---|----------|
| AC1 | **Given** valid application data, **When** an admin sends `POST /applications`, **Then** the application is created and returned with a `201 Created` status |
| AC2 | **Given** an existing application ID, **When** an admin sends `PUT /applications/{id}` with updated data, **Then** the application is updated and returned |
| AC3 | **Given** an existing application ID, **When** an admin sends `GET /applications/{id}`, **Then** the application details are returned |
| AC4 | **Given** applications exist in the system, **When** an admin sends `GET /applications`, **Then** a list of all applications is returned |
| AC5 | **Given** a duplicate application name, **When** `POST /applications` is called, **Then** a `409 Conflict` response is returned |
| AC6 | **Given** a non-existent application ID, **When** `GET /applications/{id}` or `PUT /applications/{id}` is called, **Then** a `404 Not Found` response is returned |
| AC7 | **Given** invalid input (e.g., name too long, missing required fields), **When** any endpoint is called, **Then** a `400 Bad Request` with validation errors is returned |

---

## 3. Spec Issues

| # | Issue | Severity |
|---|-------|----------|
| SI-1 | Story text says "associated with related configuration IDs" but the Application model has no `configurationIds` field. This field is not in the model definition. | Medium — needs clarification |

---

## 4. Test Strategy

### Unit Tests (xUnit + Shouldly)
- **Domain:** `Application` entity construction, validation guards (empty name, max length), unique name invariant
- **Application:** Command/Query handlers with mocked repository, FluentValidation validators for each command/query
- **API:** Controller action methods with mocked mediator

### Integration Tests
- EF Core in-memory or testcontainer database
- End-to-end HTTP tests via `WebApplicationFactory`

### Test File Changes (estimated)

| Test Project | Files |
|---|---|
| `tests/Ai.Api.Domain.Tests/` | `ApplicationTests.cs` |
| `tests/Ai.Api.Application.Tests/` | `CreateApplicationCommandHandlerTests.cs`, `UpdateApplicationCommandHandlerTests.cs`, `GetApplicationByIdQueryHandlerTests.cs`, `GetApplicationsQueryHandlerTests.cs` |
| `tests/Ai.Api.Api.Tests/` | `ApplicationsControllerTests.cs` |
| `tests/Ai.Api.Integration.Tests/` | `ApplicationEndpointsTests.cs` |

---

## 5. File Change List

### 5.1 Domain Layer (`src/Ai.Api.Domain/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Entities/Application.cs` | Domain entity with guards and business rules |
| CREATE | `Exceptions/DomainException.cs` | Base domain exception for business rule violations |

### 5.2 Application Layer (`src/Ai.Api.Application/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Features/ApplicationManagement/Commands/CreateApplicationCommand.cs` | Command + handler for creating an application |
| CREATE | `Features/ApplicationManagement/Commands/UpdateApplicationCommand.cs` | Command + handler for updating an application |
| CREATE | `Features/ApplicationManagement/Queries/GetApplicationByIdQuery.cs` | Query + handler for retrieving by ID |
| CREATE | `Features/ApplicationManagement/Queries/GetApplicationsQuery.cs` | Query + handler for listing all applications |
| CREATE | `Features/ApplicationManagement/DTOs/ApplicationDto.cs` | Internal DTO for query results |
| CREATE | `Interfaces/Repositories/IApplicationRepository.cs` | Repository interface (defined in Application per architecture) |
| CREATE | `Validators/CreateApplicationCommandValidator.cs` | FluentValidation validator for create command |
| CREATE | `Validators/UpdateApplicationCommandValidator.cs` | FluentValidation validator for update command |
| CREATE | `Mappings/ApplicationMappingExtensions.cs` | Extension methods for entity ↔ DTO mapping |
| CREATE | `DependencyInjection.cs` | `AddApplication()` IHostBuilder extension (Wolverine + validators) |

### 5.3 Infrastructure Layer (`src/Ai.Api.Infrastructure/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Persistence/Context/AppDbContext.cs` | EF Core DbContext with `Applications` DbSet |
| CREATE | `Persistence/Entities/ApplicationEntity.cs` | Persistence entity (ORM mapping) |
| CREATE | `Persistence/Configurations/ApplicationEntityConfiguration.cs` | EF Core Fluent API configuration (unique index on name, max lengths) |
| CREATE | `Persistence/Repositories/ApplicationRepository.cs` | Repository implementation |
| CREATE | `DependencyInjection.cs` | `AddInfrastructure()` IServiceCollection extension (DbContext, repositories) |

### 5.4 API Layer (`src/Ai.Api/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Controllers/ApplicationsController.cs` | API controller with 4 endpoints |
| CREATE | `Models/Requests/CreateApplicationRequest.cs` | POST request model |
| CREATE | `Models/Requests/UpdateApplicationRequest.cs` | PUT request model |
| CREATE | `Models/Responses/ApplicationResponse.cs` | Response model for all endpoints |
| MODIFY | `Program.cs` | Register `AddApplication()` and `AddInfrastructure()`, configure Wolverine service location |

### 5.5 NuGet Packages (via `Directory.Packages.props`)

| Package | Layer | Version |
|---------|-------|---------|
| `WolverineFx` | Application | latest stable |
| `WolverineFx.FluentValidation` | Application | latest stable |
| `WolverineFx.RuntimeCompilation` | Application | latest stable |
| `FluentValidation` | Application | latest stable |
| `Microsoft.EntityFrameworkCore` | Infrastructure | latest stable |
| `Microsoft.EntityFrameworkCore.SqlServer` (or provider) | Infrastructure | latest stable |

---

## 6. Implementation Details

### 6.1 Domain Entity: `Application`

```csharp
// src/Ai.Api.Domain/Entities/Application.cs
// - Id: Guid, initialized with Guid.CreateVersion7()
// - Name: string, required, max 256, private set with validation guard
// - Comments: string?, max 1024
// - Private parameterless ctor for EF Core
// - Public ctor: Application(Guid id, string name, string? comments)
// - Update method for modifying name/comments
```

### 6.2 Commands (CQRS naming checkpoint)

| Command | Format Check | ✅/❌ |
|---------|-------------|------|
| `CreateApplicationCommand` | Verb + Noun + "Command" | ✅ |
| `UpdateApplicationCommand` | Verb + Noun + "Command" | ✅ |

### 6.3 Queries (CQRS naming checkpoint)

| Query | Format Check | ✅/❌ |
|-------|-------------|------|
| `GetApplicationByIdQuery` | "Get" + Noun + "Query" | ✅ |
| `GetApplicationsQuery` | "Get" + Noun + "Query" | ✅ |

### 6.4 Records (syntax check)

All DTOs, commands, queries, requests, responses must use **class-like syntax** — positional syntax is prohibited.

### 6.5 Wolverine Integration

```csharp
// Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IHostBuilder AddApplication(this IHostBuilder host)
    {
        host.UseWolverine(opts =>
        {
            opts.UseFluentValidation();
            opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly);
        });
        return host;
    }
}
```

```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => ...);
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        return services;
    }
}
```

```csharp
// Program.cs — call via builder.Host.AddApplication() and builder.Services.AddInfrastructure()
// configure AlwaysUseServiceLocationFor<AppDbContext>()
```

### 6.6 Error Handling Strategy

| Scenario | Layer | Exception | HTTP Status |
|----------|-------|-----------|-------------|
| Duplicate name | Infrastructure | `DbUpdateException` → caught in repository | 409 Conflict |
| Not found | Infrastructure/Repository | Returns null → handler throws | 404 Not Found |
| Validation failure | Application | `ValidationException` (FluentValidation) | 400 Bad Request |
| Invalid domain state | Domain | `DomainException` | 400 Bad Request |

### 6.7 Mapping Flow

```
Request: API Request → Command/Query → Domain Entity → Persistence Entity → DB
Response: DB → Persistence Entity → Domain Entity / DTO → API Response
```

---

## 7. Implementation Order

| Step | Layer | Task | Depends On |
|------|-------|------|------------|
| 1 | NuGet | Add all required packages to `Directory.Packages.props` and csproj files | — |
| 2 | Domain | Create `DomainException` class | — |
| 3 | Domain | Create `Application` entity | Step 2 |
| 4 | Application | Create `IApplicationRepository` interface | Step 3 |
| 5 | Application | Create `ApplicationDto` record | Step 3 |
| 6 | Application | Create mapping extensions | Steps 3, 5 |
| 7 | Application | Create validators | Step 3 |
| 8 | Application | Create `CreateApplicationCommand` + handler | Steps 4, 5, 6 |
| 9 | Application | Create `UpdateApplicationCommand` + handler | Steps 4, 5, 6 |
| 10 | Application | Create `GetApplicationByIdQuery` + handler | Steps 4, 5, 6 |
| 11 | Application | Create `GetApplicationsQuery` + handler | Steps 4, 5, 6 |
| 12 | Application | Create `DependencyInjection` (Wolverine setup) | Steps 7-11 |
| 13 | Infrastructure | Create `ApplicationEntity` persistence entity | Step 3 |
| 14 | Infrastructure | Create `ApplicationEntityConfiguration` (Fluent API) | Step 13 |
| 15 | Infrastructure | Create `AppDbContext` | Steps 13, 14 |
| 16 | Infrastructure | Create `ApplicationRepository` | Steps 4, 13, 15 |
| 17 | Infrastructure | Create `DependencyInjection` | Steps 15, 16 |
| 18 | API | Create request models (`CreateApplicationRequest`, `UpdateApplicationRequest`) | — |
| 19 | API | Create `ApplicationResponse` model | Step 5 |
| 20 | API | Create `ApplicationsController` | Steps 8-11, 18, 19 |
| 21 | API | Modify `Program.cs` (DI registration) | Steps 12, 17 |

---

## 8. Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | Database provider is SQL Server | Default for enterprise .NET apps; no provider specified in requirements; can be swapped via configuration |
| A2 | No authentication/authorization in initial implementation | Story says "administrator" but no auth requirements in acceptance criteria; can be added later as a cross-cutting concern |
| A3 | `GET /applications` returns all records without pagination | Simpler initial implementation; pagination can be added later if needed |
| A4 | Application name uniqueness is enforced at DB level via unique index | Most reliable way to guarantee uniqueness under concurrency |
| A5 | PUT performs a full update (not partial) | Standard REST PUT semantics; PATCH not mentioned in requirements |
| A6 | "configuration IDs" mentioned in story is intentionally excluded | Model definition has no such field; treating as a documentation discrepancy (see Spec Issue SI-1) |
| A7 | EF Core migrations will be generated after entity/configurations are in place | Standard EF Core workflow; separate step from code creation |
| A8 | Mapping uses manual extension methods (no AutoMapper) | Architecture doc favors manual mapping; AutoMapper only for dynamic objects |
| A9 | `IApplicationRepository` lives in Application layer (not Domain) | Per architecture doc: "Not repository interfaces, these should be in Application layer" |

---

## 9. Open Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should the Application model include `configurationIds` as stated in the story text? (See Spec Issue SI-1) | Model design, DB schema, API contract |
| Q2 | What database provider should be used? (SQL Server, PostgreSQL, etc.) | Infrastructure/DbContext setup, connection string |
| Q3 | Should `GET /applications` support sorting or filtering? | Query handler design |
| Q4 | Is soft delete required, or is hard delete out of scope? | The story does not mention DELETE — is this intentional? |
| Q5 | Should the `PUT` endpoint allow partial updates (PATCH semantics) or full replacement? | Command and handler design |
