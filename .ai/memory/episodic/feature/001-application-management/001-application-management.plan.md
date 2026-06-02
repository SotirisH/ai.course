# Plan: Application Management Feature

**Ticket**: 001  
**Feature Name**: Application Management  
**Work Item Type**: feature  
**Date**: 2026-06-02

---

## Story Summary

As an administrator, I want to manage applications in the system. The feature provides CRUD operations (Create, Update, Read, List) for applications. Each application has a globally unique name and optional comments.

---

## Acceptance Criteria (Given-When-Then)

### AC1: Create Application
- **Given** an administrator with valid application data (`name`, optional `comments`)
- **When** they send a `POST /applications` request
- **Then** a new application is created, persisted, and a `201 Created` response is returned with the application details
- **When** the name already exists
- **Then** a `409 Conflict` response is returned

### AC2: Update Application
- **Given** an existing application
- **When** an administrator sends a `PUT /applications/{id}` with updated `name` and `comments`
- **Then** the application is updated and a `200 OK` response is returned with updated details
- **When** the application does not exist → `404 Not Found`
- **When** the new name conflicts with another application → `409 Conflict`

### AC3: Get Application by ID
- **Given** an existing application
- **When** a user sends a `GET /applications/{id}`
- **Then** a `200 OK` response is returned with the application details
- **When** the application does not exist → `404 Not Found`

### AC4: List All Applications
- **Given** applications exist in the system
- **When** a user sends a `GET /applications`
- **Then** a `200 OK` response is returned with a list of all applications

### AC5: Input Validation
- **Given** invalid input (empty name, name > 256 chars, comments > 1024 chars)
- **When** a request is made
- **Then** a `400 Bad Request` response is returned with RFC 7807 Problem Details

---

## Test Strategy

| Layer | Test Type | What to Test |
|-------|-----------|-------------|
| Domain | Unit (xUnit + Shouldly) | `Application` entity constructor validation, `Update()` method |
| Application | Unit (xUnit + Shouldly) | Command/query handlers (mocked repository), FluentValidation validators |
| API | Integration (xUnit + WebApplicationFactory) | Controller endpoints with in-memory EF Core |
| Infrastructure | Integration (Testcontainers) | Repository against real PostgreSQL |

**Coverage Target**: All handlers, validators, domain entity methods, and controller actions.

---

## File Change List

### Domain Layer (`src/Ai.Api.Domain/`)
| File | Action | Purpose |
|------|--------|---------|
| `Entities/Application.cs` | CREATE | Domain entity: Id, Name, Comments with validation |
| `Exceptions/DomainException.cs` | CREATE | Base domain exception for business rule violations |

### Application Layer (`src/Ai.Api.Application/`)
| File | Action | Purpose |
|------|--------|---------|
| `Features/ApplicationManagement/Commands/CreateApplicationHandler.cs` | CREATE | `CreateApplication` command + handler |
| `Features/ApplicationManagement/Commands/UpdateApplicationHandler.cs` | CREATE | `UpdateApplication` command + handler |
| `Features/ApplicationManagement/Queries/GetApplicationByIdHandler.cs` | CREATE | `GetApplicationById` query + handler |
| `Features/ApplicationManagement/Queries/GetApplicationsHandler.cs` | CREATE | `GetApplications` query + handler |
| `Features/ApplicationManagement/DTOs/ApplicationDto.cs` | CREATE | Read-model DTO (record) |
| `Interfaces/Repositories/IApplicationRepository.cs` | CREATE | Repository contract |
| `Mappings/ApplicationMappingExtensions.cs` | CREATE | Domain ↔ DTO mapping extensions |
| `Validators/CreateApplicationValidator.cs` | CREATE | FluentValidation: name required, max 256, comments max 1024 |
| `Validators/UpdateApplicationValidator.cs` | CREATE | FluentValidation: id not empty, name required, max 256, comments max 1024 |
| `DependencyInjection.cs` | CREATE | `AddApplication()` extension method (registers Wolverine, validators) |
| `Ai.Api.Application.csproj` | MODIFY | Add WolverineFx, WolverineFx.FluentValidation, FluentValidation packages |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)
| File | Action | Purpose |
|------|--------|---------|
| `Persistence/Context/AppDbContext.cs` | CREATE | EF Core DbContext with `Applications` DbSet |
| `Persistence/Configurations/ApplicationConfiguration.cs` | CREATE | `IEntityTypeConfiguration<ApplicationEntity>` — unique index on Name |
| `Persistence/Entities/ApplicationEntity.cs` | CREATE | Persistence entity (EF Core entity) |
| `Persistence/Repositories/ApplicationRepository.cs` | CREATE | `IApplicationRepository` implementation |
| `DependencyInjection.cs` | CREATE | `AddInfrastructure(connectionString)` extension method |
| `Ai.Api.Infrastructure.csproj` | MODIFY | Add Npgsql.EntityFrameworkCore.PostgreSQL, EF Core Design packages |

### API Layer (`src/Ai.Api/`)
| File | Action | Purpose |
|------|--------|---------|
| `Controllers/ApplicationsController.cs` | CREATE | CRUD controller: POST, PUT, GET/{id}, GET |
| `Models/Requests/CreateApplicationRequest.cs` | CREATE | POST request model (record) |
| `Models/Requests/UpdateApplicationRequest.cs` | CREATE | PUT request model (record) |
| `Models/Responses/ApplicationResponse.cs` | CREATE | Response model (record) |
| `Program.cs` | MODIFY | Register DbContext, Wolverine, Infrastructure & Application services, ProblemDetails |
| `appsettings.json` | MODIFY | Add `ConnectionStrings.Default` placeholder |
| `appsettings.Development.json` | MODIFY | Add `ConnectionStrings.Default` with local PostgreSQL connection string |

### Root
| File | Action | Purpose |
|------|--------|---------|
| `Directory.Packages.props` | CREATE | Central Package Management — all NuGet version definitions |

---

## Implementation Details

### 1. Domain Entity

```csharp
// src/Ai.Api.Domain/Entities/Application.cs
public class Application
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = null!;
    public string? Comments { get; private set; }

    private Application() { } // EF Core

    public Application(string name, string? comments = null)
    {
        Validate(name, comments);
        Name = name;
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        Validate(name, comments);
        Name = name;
        Comments = comments;
    }

    private static void Validate(string name, string? comments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Application name is required.");
        if (name.Length > 256)
            throw new DomainException("Application name must not exceed 256 characters.");
        if (comments?.Length > 1024)
            throw new DomainException("Comments must not exceed 1024 characters.");
    }
}
```

### 2. Persistence Entity (Infrastructure)

A separate persistence entity (`ApplicationEntity`) lives in Infrastructure to avoid leaking EF Core concerns into Domain. This follows the architecture guide which states persistence entities "Must never leak outside Infrastructure."

### 3. CQRS with WolverineFx

Commands and queries follow WolverineFx conventions — the command/query record lives in the same file as its handler.

- **CreateApplication** (`CreateApplicationHandler.cs`):
  - Record: `CreateApplication(string Name, string? Comments)`
  - Handler: Validated by FluentValidation middleware → checks uniqueness via repository → creates domain entity → persists → maps & returns `ApplicationDto`

- **UpdateApplication** (`UpdateApplicationHandler.cs`):
  - Record: `UpdateApplication(Guid Id, string Name, string? Comments)`
  - Handler: Fetches existing entity → 404 if missing → checks uniqueness → calls `entity.Update()` → persists → maps & returns `ApplicationDto`

- **GetApplicationById** (`GetApplicationByIdHandler.cs`):
  - Record: `GetApplicationById(Guid Id)`
  - Handler: Retrieves from repository → returns `ApplicationDto` or null

- **GetApplications** (`GetApplicationsHandler.cs`):
  - Record: `GetApplications()`
  - Handler: Retrieves all → returns `IReadOnlyList<ApplicationDto>`

### 4. Repository Interface

```csharp
public interface IApplicationRepository
{
    Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default);
    Task<ApplicationDto> AddAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task<ApplicationDto> UpdateAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
```

The repository returns `ApplicationDto` to keep persistence concerns internal. The infrastructure implementation maps between `ApplicationEntity` and `Domain.Entities.Application`.

### 5. Validation (FluentValidation + Wolverine Middleware)

- `CreateApplicationValidator`: Name required, max length 256; Comments max length 1024
- `UpdateApplicationValidator`: Id not empty; Name required, max length 256; Comments max length 1024
- Wolverine's `.UseFluentValidation()` middleware auto-discovers and applies validators on the handler pipeline.

### 6. API Controller

```csharp
[ApiController]
[Route("applications")]
public class ApplicationsController : ControllerBase
{
    // POST   /applications      → 201 Created (or 409 Conflict)
    // PUT    /applications/{id} → 200 OK (or 404 Not Found / 409 Conflict)
    // GET    /applications/{id} → 200 OK (or 404 Not Found)
    // GET    /applications      → 200 OK
}
```

Controller uses `IMessageBus` from Wolverine to dispatch commands/queries. Maps request models → commands/queries, and DTOs → responses.

### 7. Database

- PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Connection string stored in `appsettings.Development.json` (dev) and environment variables/user secrets (prod)
- `ApplicationConfiguration` enforces a unique index on `Name`:
  ```csharp
  builder.HasIndex(e => e.Name).IsUnique();
  ```
- Name max length 256 at column level: `builder.Property(e => e.Name).HasMaxLength(256);`

### 8. Central Package Management

Tech-stack rules require Central Package Management. Since `Directory.Packages.props` does not exist, it will be created in the repo root with version definitions for all packages.

### 9. Dependency Injection Wiring

- **Application layer** (`DependencyInjection.cs`): `AddApplication()` → registers Wolverine with FluentValidation middleware
- **Infrastructure layer** (`DependencyInjection.cs`): `AddInfrastructure(connectionString)` → registers `AppDbContext`, `IApplicationRepository`
- **API layer** (`Program.cs`): Calls `AddApplication()`, `AddInfrastructure()`, `AddProblemDetails()`

---

## Implementation Order

| Step | Layer | Tasks |
|------|-------|-------|
| 1 | Root | Create `Directory.Packages.props` with all NuGet package versions |
| 2 | Domain | Create `DomainException`, then `Application` entity |
| 3 | Infrastructure | Add EF Core/Npgsql packages, create `ApplicationEntity`, `AppDbContext`, `ApplicationConfiguration` |
| 4 | Infrastructure | Generate initial EF Core migration |
| 5 | Application | Add WolverineFx/FluentValidation packages, create `ApplicationDto`, `IApplicationRepository`, commands, queries, handlers, validators, mapping extensions |
| 6 | Application | Create `DependencyInjection.cs` |
| 7 | Infrastructure | Implement `ApplicationRepository`, create `DependencyInjection.cs` |
| 8 | API | Create request/response models, `ApplicationsController` |
| 9 | API | Update `Program.cs` — register all services, add ProblemDetails |
| 10 | API | Update `appsettings.json` and `appsettings.Development.json` with connection strings |
| 11 | Verify | Build solution, run migration, test endpoints |

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | PostgreSQL is the database | Architecture rules and security rules reference PostgreSQL/Npgsql; `about.md` describes a configuration service backed by PostgreSQL |
| A2 | EF Core Code-First with migrations | Architecture rules specify EF Core; `Migrations/` folder already exists in Infrastructure |
| A3 | WolverineFx is the CQRS mediator | Architecture rules explicitly mandate WolverineFx for commands/queries with FluentValidation middleware |
| A4 | No authentication yet | The story mentions "administrator" but no auth infrastructure exists; security rules require `[Authorize]` but adding it now would block all endpoints — deferred to a future auth work item |
| A5 | Name uniqueness enforced at DB + app layer | AC5 specifies uniqueness; enforced with DB unique index AND application-layer existence check |
| A6 | "associated with related configuration IDs" is future scope | The current model (id, name, comments) has no configuration relation field; likely a later feature |
| A7 | RFC 7807 Problem Details for errors | Security rules mandate it; .NET 10 has built-in `AddProblemDetails()` |
| A8 | Central Package Management required | Tech-stack rules require it but no `Directory.Packages.props` exists; created as part of this feature |
| A9 | Separate persistence entity in Infrastructure | Architecture guide states persistence entities "Must never leak outside Infrastructure"; separate `ApplicationEntity` class |
| A10 | No DELETE endpoint | Work item only specifies POST, PUT, GET/{id}, GET; DELETE not listed |
| A11 | No pagination on GET /applications | Work item doesn't mention pagination; return all applications for now per KISS/YAGNI |

---

## Questions Requiring Clarification

| # | Question |
|---|----------|
| Q1 | Should authentication be added now or deferred? The story says "As an administrator" but no auth infrastructure exists yet. Security rules require `[Authorize]` on non-public endpoints. |
| Q2 | The AC mentions "associated with related configuration IDs" but the model only has id/name/comments. Should this relationship be added now or is it out of scope? |
| Q3 | Should we add a `DELETE /applications/{id}` endpoint? The work item doesn't list it, but full CRUD is typical for management features. |
| Q4 | What PostgreSQL connection string should be used for local development? Should we use User Secrets or just `appsettings.Development.json`? |
| Q5 | Should the `GET /applications` endpoint support pagination/filtering, or return all applications? |
