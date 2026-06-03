# Plan: Application Management Feature

**Ticket**: 001  
**Feature Name**: Application Management  
**Work Item Type**: feature  
**Date**: 2026-06-02

---

## Story Summary

As an administrator, I want to manage applications in the system. The feature exposes RESTful CRUD endpoints for creating, updating, retrieving, and listing applications. Each application has a unique `name` (max 256 chars) and optional `comments` (max 1024 chars), identified by a GUID primary key.

---

## Acceptance Criteria (Given-When-Then)

### AC1: Create Application
- **Given** an administrator with valid application data (`name`, optional `comments`)
- **When** they send a `POST /applications` request
- **Then** a new application is created, persisted, and a `201 Created` response is returned with the application details
- **When** the `name` already exists → `409 Conflict`

### AC2: Update Application
- **Given** an existing application with `id`
- **When** an administrator sends a `PUT /applications/{id}` with updated `name` and/or `comments`
- **Then** the application is updated and a `200 OK` is returned with the updated details
- **When** the application does not exist → `404 Not Found`
- **When** the new `name` conflicts with another application → `409 Conflict`

### AC3: Get Application by ID
- **Given** an existing application with `id`
- **When** a user sends a `GET /applications/{id}`
- **Then** a `200 OK` is returned with the application details
- **When** the application does not exist → `404 Not Found`

### AC4: List All Applications
- **Given** applications exist in the system
- **When** a user sends a `GET /applications`
- **Then** a `200 OK` is returned with a list of all applications

### AC5: Input Validation
- **Given** invalid input (empty name, name > 256 chars, comments > 1024 chars)
- **When** a request is made
- **Then** a `400 Bad Request` is returned with RFC 7807 Problem Details

### AC6: Delete Application
- **Given** an existing application with `id`
- **When** an administrator sends a `DELETE /applications/{id}`
- **Then** the application is deleted and a `204 No Content` is returned
- **When** the application does not exist → `404 Not Found`

---

## Test Strategy

| Layer | Test Type | What to Test |
|-------|-----------|-------------|
| Domain | Unit (xUnit + Shouldly) | `Application` entity constructor validation, `Update()` method, `DomainException` |
| Application | Unit (xUnit + Shouldly) | Command/query handlers with mocked `IApplicationRepository`, FluentValidation validators |
| API | Integration (xUnit + WebApplicationFactory) | All 5 controller endpoints against in-memory EF Core |
| Infrastructure | Integration (Testcontainers PostgreSQL) | `ApplicationRepository` against real PostgreSQL |

**Coverage Target**: All handlers, validators, domain entity methods, and controller actions.

---

## File Change List

### Domain Layer (`src/Ai.Api.Domain/`)
| File | Action | Purpose |
|------|--------|---------|
| `Entities/Application.cs` | NO CHANGE | Already exists; Q4 resolved to leave second constructor as-is |
| `Exceptions/DomainException.cs` | NO CHANGE | Already exists and suffices |

### Application Layer (`src/Ai.Api.Application/`)
| File | Action | Purpose |
|------|--------|---------|
| `Features/ApplicationManagement/Commands/CreateApplicationHandler.cs` | CREATE | `CreateApplication` command record + handler |
| `Features/ApplicationManagement/Commands/UpdateApplicationHandler.cs` | CREATE | `UpdateApplication` command record + handler |
| `Features/ApplicationManagement/Commands/DeleteApplicationHandler.cs` | CREATE | `DeleteApplication` command record + handler |
| `Features/ApplicationManagement/Queries/GetApplicationByIdHandler.cs` | CREATE | `GetApplicationById` query record + handler |
| `Features/ApplicationManagement/Queries/GetApplicationsHandler.cs` | CREATE | `GetApplications` query record + handler |
| `Features/ApplicationManagement/DTOs/ApplicationDto.cs` | CREATE | Read-model DTO (record) |
| `Interfaces/Repositories/IApplicationRepository.cs` | CREATE | Repository contract (includes DeleteAsync) |
| `Mappings/ApplicationMappingExtensions.cs` | CREATE | Domain ↔ DTO mapping extension methods |
| `Validators/CreateApplicationValidator.cs` | CREATE | FluentValidation: name required, max 256, comments max 1024 |
| `Validators/UpdateApplicationValidator.cs` | CREATE | FluentValidation: id not empty, name required, max 256, comments max 1024 |
| `DependencyInjection.cs` | CREATE | `AddApplication()` extension — registers Wolverine with FluentValidation middleware |
| `Ai.Api.Application.csproj` | MODIFY | Add NuGet package references for WolverineFx, WolverineFx.FluentValidation, WolverineFx.RuntimeCompilation, FluentValidation |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)
| File | Action | Purpose |
|------|--------|---------|
| `Persistence/Context/AppDbContext.cs` | CREATE | EF Core DbContext with `Applications` DbSet |
| `Persistence/Configurations/ApplicationConfiguration.cs` | CREATE | `IEntityTypeConfiguration<ApplicationEntity>` — unique index on Name, column max lengths |
| `Persistence/Entities/ApplicationEntity.cs` | CREATE | Persistence entity (separate from domain entity per architecture rules) |
| `Persistence/Repositories/ApplicationRepository.cs` | CREATE | `IApplicationRepository` implementation with domain↔persistence mapping |
| `DependencyInjection.cs` | CREATE | `AddInfrastructure(connectionString)` extension — registers DbContext and repositories |
| `Ai.Api.Infrastructure.csproj` | MODIFY | Ensure EF Core / Npgsql package references present |

### API Layer (`src/Ai.Api/`)
| File | Action | Purpose |
|------|--------|---------|
| `Controllers/ApplicationsController.cs` | CREATE | CRUD controller: POST, PUT, DELETE, GET/{id}, GET |
| `Models/Requests/CreateApplicationRequest.cs` | CREATE | POST request body model (record) |
| `Models/Requests/UpdateApplicationRequest.cs` | CREATE | PUT request body model (record) |
| `Models/Responses/ApplicationResponse.cs` | CREATE | Response model (record) |
| `Program.cs` | MODIFY | Register `AddApplication()`, `AddInfrastructure()`, `AddProblemDetails()`, Wolverine service-location for DbContext |
| `Ai.Api.csproj` | MODIFY | Add `Microsoft.AspNetCore.OpenApi` package reference |

### Root
| File | Action | Purpose |
|------|--------|---------|
| `Directory.Packages.props` | REVIEW | Already exists with needed versions; verify versions are current and complete |

---

## Implementation Details

### 1. Domain Entity (EXISTING — NO CHANGE)

The `Application` entity at `src/Ai.Api.Domain/Entities/Application.cs` currently exists with:
- `Guid Id` initialized via `Guid.CreateVersion7()`
- `string Name` with validation in constructor and `Update()`
- `string? Comments`
- Private parameterless constructor for EF Core
- Two public constructors: `(string name, string? comments)` and `(Guid id, string name, string? comments)`
- `Update(string name, string? comments)` method

**No changes needed.** Q4 resolved: the second constructor `(Guid id, string name, string? comments)` is used for reconstitution from persistence and should remain as-is.

### 2. Persistence Entity (Infrastructure)

```csharp
public class ApplicationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Comments { get; set; }
}
```

### 3. EF Core Configuration

```csharp
public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Comments).HasMaxLength(1024);
        builder.HasIndex(e => e.Name).IsUnique();
    }
}
```

### 4. CQRS with WolverineFx

**CreateApplication** (`CreateApplicationHandler.cs`):
- Command: `record CreateApplication(string Name, string? Comments);`
- Handler flow: validated by FluentValidation middleware → checks name uniqueness via `IApplicationRepository.ExistsByNameAsync()` → creates domain entity → persists → maps to `ApplicationDto`

**UpdateApplication** (`UpdateApplicationHandler.cs`):
- Command: `record UpdateApplication(Guid Id, string Name, string? Comments);`
- Handler flow: fetches existing via `GetByIdAsync()` → 404 if null → checks name uniqueness (excluding self) → calls `entity.Update()` → persists → maps to `ApplicationDto`

**DeleteApplication** (`DeleteApplicationHandler.cs`):
- Command: `record DeleteApplication(Guid Id);`
- Handler flow: fetches existing via `GetByIdAsync()` → 404 if null → deletes via `IApplicationRepository.DeleteAsync()` → returns nothing

**GetApplicationById** (`GetApplicationByIdHandler.cs`):
- Query: `record GetApplicationById(Guid Id);`
- Handler flow: retrieves from repository → returns `ApplicationDto` or null

**GetApplications** (`GetApplicationsHandler.cs`):
- Query: `record GetApplications();`
- Handler flow: retrieves all → returns `IReadOnlyList<ApplicationDto>`

### 5. Repository Interface

```csharp
public interface IApplicationRepository
{
    Task<ApplicationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default);
    Task<ApplicationDto> AddAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task<ApplicationDto> UpdateAsync(Domain.Entities.Application application, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
```

Repository returns `ApplicationDto` to prevent persistence entities from leaking. Internal mapping between `ApplicationEntity` ↔ `Domain.Entities.Application` stays in Infrastructure.

### 6. Validation (FluentValidation + Wolverine Middleware)

- `CreateApplicationValidator`: Name not empty, max length 256; Comments max length 1024
- `UpdateApplicationValidator`: Id not empty; Name not empty, max length 256; Comments max length 1024
- Wolverine's `.UseFluentValidation()` auto-discovers and applies validators on the handler pipeline

### 7. API Controller

```csharp
[ApiController]
[Route("applications")]
public class ApplicationsController(IMessageBus bus) : ControllerBase
{
    // POST   /applications      → 201 Created (or 409 Conflict)
    // PUT    /applications/{id} → 200 OK (or 404 / 409)
    // DELETE /applications/{id} → 204 No Content (or 404)
    // GET    /applications/{id} → 200 OK (or 404)
    // GET    /applications      → 200 OK
}
```

Uses Wolverine's `IMessageBus` to dispatch commands/queries. Maps `Request` models → commands/queries, and `ApplicationDto` → `ApplicationResponse`.

### 8. Database

- PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Connection string already in `appsettings.Development.json`: `Host=localhost;Database=AiApi;Username=postgres;Password=postgres`
- EF Core migration generated after entity and DbContext are created

### 9. Dependency Injection Wiring

- **Application** (`DependencyInjection.cs`): `AddApplication()` → registers Wolverine with `.UseFluentValidation()` and `AlwaysUseServiceLocationFor<AppDbContext>()`
- **Infrastructure** (`DependencyInjection.cs`): `AddInfrastructure(connectionString)` → registers `AppDbContext` (Npgsql), `IApplicationRepository` → `ApplicationRepository`
- **API** (`Program.cs`): calls `builder.Services.AddApplication()`, `builder.Services.AddInfrastructure(connectionString)`, `builder.Services.AddProblemDetails()`

### 10. Central Package Management

`Directory.Packages.props` already exists with versions:
- `FluentValidation` 12.1.1
- `Microsoft.AspNetCore.OpenApi` 10.0.8
- `Microsoft.EntityFrameworkCore` 10.0.4
- `Microsoft.EntityFrameworkCore.Design` 10.0.4
- `Microsoft.EntityFrameworkCore.Relational` 10.0.4
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2
- `WolverineFx` 6.3.2
- `WolverineFx.FluentValidation` 6.3.2
- `WolverineFx.RuntimeCompilation` 6.3.2

Verify these are latest stable before implementing.

---

## Implementation Order

| Step | Layer | Tasks |
|------|-------|-------|
| 1 | Root | Verify `Directory.Packages.props` versions are current |
| 2 | Domain | SKIP — `Application.cs` entity is complete; no changes needed per Q4 |
| 3 | Application | Add package references to `.csproj` |
| 4 | Application | Create `ApplicationDto`, `IApplicationRepository`, mapping extensions |
| 5 | Application | Create validators: `CreateApplicationValidator`, `UpdateApplicationValidator` |
| 6 | Application | Create commands & handlers: `CreateApplicationHandler`, `UpdateApplicationHandler`, `DeleteApplicationHandler` |
| 7 | Application | Create queries & handlers: `GetApplicationByIdHandler`, `GetApplicationsHandler` |
| 8 | Application | Create `DependencyInjection.cs` |
| 9 | Infrastructure | Add package references to `.csproj` |
| 10 | Infrastructure | Create `ApplicationEntity`, `ApplicationConfiguration`, `AppDbContext` |
| 11 | Infrastructure | Implement `ApplicationRepository` (includes `DeleteAsync`) |
| 12 | Infrastructure | Create `DependencyInjection.cs` |
| 13 | Infrastructure | Generate initial EF Core migration |
| 14 | API | Create request/response models |
| 15 | API | Create `ApplicationsController` (POST, PUT, DELETE, GET/{id}, GET) |
| 16 | API | Update `Program.cs` — register all services |
| 17 | API | Add `Microsoft.AspNetCore.OpenApi` package reference |
| 18 | Verify | Build solution, run migration, test all 5 endpoints |

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | PostgreSQL is the database | `Npgsql.EntityFrameworkCore.PostgreSQL` already in `Directory.Packages.props`; connection strings pre-configured |
| A2 | EF Core Code-First with migrations | Architecture rules specify EF Core with `Migrations/` folder |
| A3 | WolverineFx is the CQRS mediator | Architecture rules explicitly mandate WolverineFx for commands/queries |
| A4 | No authentication/authorization in this feature | Auth infra doesn't exist; deferred to future work item |
| A5 | Name uniqueness enforced at both DB and app layers | DB unique index + application-layer existence check for better error messages |
| A6 | "associated with related configuration IDs" is deferred | Model definition only includes `id`, `name`, `comments`; no configuration relation field specified |
| A7 | RFC 7807 Problem Details for errors | .NET 10 has built-in `AddProblemDetails()` per architecture recommendations |
| A8 | Central Package Management already configured | `Directory.Build.props` enables it; `Directory.Packages.props` exists with versions |
| A9 | Separate persistence entity in Infrastructure | Architecture guide: persistence entities "Must never leak outside Infrastructure" |
| A10 | No pagination on GET /applications | Not specified in acceptance criteria; KISS/YAGNI |
| A11 | Connection string in `appsettings.Development.json` is sufficient | Already configured; production uses env vars/user secrets |
| A12 | DELETE endpoint included per Q2 resolution | User explicitly requested DELETE be added to scope |
| A13 | `IMessageBus` for controller dispatch | Wolverine mediator pattern per architecture CQRS guidance |

---

## Questions Requiring Clarification

| # | Question | Context | Resolution |
|---|----------|---------|------------|
| Q1 | **"associated with related configuration IDs"** — add now or defer? | Criteria vs model mismatch | **DEFERRED** |
| Q2 | **DELETE endpoint** — needed? | Scope | **INCLUDED** — add `DELETE /applications/{id}` |
| Q3 | **Pagination / filtering** on GET /applications? | Performance | **SIMPLE LIST** — no pagination |
| Q4 | **Domain entity second constructor** bypasses validation? | Architectural consistency | **LEAVE AS-IS** — no change to existing constructor
