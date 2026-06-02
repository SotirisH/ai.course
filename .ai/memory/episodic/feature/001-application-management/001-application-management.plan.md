# Plan: Application Management Feature

**Ticket**: 001  
**Feature Name**: Application Management  
**Work Item Type**: feature  
**Date**: 2026-06-02

---

## Story Summary

As an administrator, I want to be able to manage applications in the system. The feature provides full CRUD operations for applications, each with a unique name identifier and optional comments.

---

## Acceptance Criteria (Given-When-Then)

### AC1: Create Application
- **Given** an administrator with valid application data
- **When** they send a POST request to `/applications`
- **Then** a new application is created, persisted, and a 201 Created response is returned with the application details

### AC2: Update Application
- **Given** an existing application
- **When** an administrator sends a PUT request to `/applications/{id}` with updated data
- **Then** the application is updated and a 200 OK response is returned with the updated details
- **When** the application does not exist
- **Then** a 404 Not Found response is returned

### AC3: Get Application by ID
- **Given** an existing application
- **When** a user sends a GET request to `/applications/{id}`
- **Then** a 200 OK response is returned with the application details
- **When** the application does not exist
- **Then** a 404 Not Found response is returned

### AC4: List All Applications
- **Given** one or more applications exist
- **When** a user sends a GET request to `/applications`
- **Then** a 200 OK response is returned with a list of all applications

### AC5: Unique Name Constraint
- **Given** an application with name "X" already exists
- **When** an administrator tries to create or rename another application to name "X"
- **Then** a 409 Conflict response is returned

### AC6: Input Validation
- **Given** invalid input (empty name, name exceeding 256 chars, comments exceeding 1024 chars)
- **When** a request is made
- **Then** a 400 Bad Request response is returned with validation error details (RFC 7807 Problem Details)

---

## Test Strategy

- **Unit Tests**: Domain entity construction/validation, command/query handlers (mocking repository), FluentValidation validators
- **Integration Tests**: API endpoints with in-memory test database, repository with test container
- **Test Framework**: xUnit with Shouldly for assertions
- **Coverage Target**: All handlers, validators, domain entity logic, and controller actions

---

## File Change List

### Domain Layer (`src/Ai.Api.Domain/`)
| File | Action | Purpose |
|------|--------|---------|
| `Entities/Application.cs` | CREATE | Domain entity with Id, Name, Comments |
| `Exceptions/DomainException.cs` | CREATE | Base domain exception class |

### Application Layer (`src/Ai.Api.Application/`)
| File | Action | Purpose |
|------|--------|---------|
| `Features/ApplicationManagement/Commands/CreateApplication.cs` | CREATE | Create command + Wolverine handler |
| `Features/ApplicationManagement/Commands/UpdateApplication.cs` | CREATE | Update command + Wolverine handler |
| `Features/ApplicationManagement/Queries/GetApplicationById.cs` | CREATE | Get-by-id query + Wolverine handler |
| `Features/ApplicationManagement/Queries/GetApplications.cs` | CREATE | List-all query + Wolverine handler |
| `Features/ApplicationManagement/DTOs/ApplicationDto.cs` | CREATE | Application DTO (record) |
| `Interfaces/Repositories/IApplicationRepository.cs` | CREATE | Repository interface |
| `Validators/CreateApplicationValidator.cs` | CREATE | FluentValidation for create |
| `Validators/UpdateApplicationValidator.cs` | CREATE | FluentValidation for update |
| `Mappings/ApplicationMappingExtensions.cs` | CREATE | Manual mapping extensions |
| `Ai.Api.Application.csproj` | MODIFY | Add WolverineFx, FluentValidation packages |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)
| File | Action | Purpose |
|------|--------|---------|
| `Persistence/Context/AppDbContext.cs` | CREATE | EF Core DbContext with Applications DbSet |
| `Persistence/Configurations/ApplicationConfiguration.cs` | CREATE | EF Core entity type configuration |
| `Persistence/Repositories/ApplicationRepository.cs` | CREATE | Repository implementation |
| `DependencyInjection.cs` | CREATE | Extension method for service registration |
| `Ai.Api.Infrastructure.csproj` | MODIFY | Add EF Core, Npgsql packages |
| `Migrations/` | CREATE | Initial migration (auto-generated) |

### API Layer (`src/Ai.Api/`)
| File | Action | Purpose |
|------|--------|---------|
| `Controllers/ApplicationsController.cs` | CREATE | CRUD controller |
| `Models/Requests/CreateApplicationRequest.cs` | CREATE | POST request model (record) |
| `Models/Requests/UpdateApplicationRequest.cs` | CREATE | PUT request model (record) |
| `Models/Responses/ApplicationResponse.cs` | CREATE | Response model (record) |
| `Program.cs` | MODIFY | Register DbContext, Wolverine, services |
| `appsettings.json` | MODIFY | Add connection string placeholder |

---

## Implementation Details

### 1. Domain Entity (`Application`)

```csharp
public class Application
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = null!;
    public string? Comments { get; private set; }

    private Application() { } // EF Core

    public Application(string name, string? comments = null)
    {
        // Validation in constructor per architecture guide
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Application name is required.");
        if (name.Length > 256)
            throw new DomainException("Application name must not exceed 256 characters.");
        if (comments?.Length > 1024)
            throw new DomainException("Comments must not exceed 1024 characters.");

        Name = name;
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        // Reuse validation logic
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Application name is required.");
        if (name.Length > 256)
            throw new DomainException("Application name must not exceed 256 characters.");
        if (comments?.Length > 1024)
            throw new DomainException("Comments must not exceed 1024 characters.");

        Name = name;
        Comments = comments;
    }
}
```

### 2. CQRS with WolverineFx

Commands and queries will follow the WolverineFx pattern — each command/query object lives in the same file as its handler.

**CreateApplication**:
- Command: `CreateApplication(string Name, string? Comments)`
- Handler: validates via FluentValidation middleware, maps to domain entity, calls repository, returns `ApplicationDto`

**UpdateApplication**:
- Command: `UpdateApplication(Guid Id, string Name, string? Comments)`
- Handler: retrieves entity, updates fields, save changes. Returns 404 if not found.

**GetApplicationById**:
- Query: `GetApplicationById(Guid Id)`
- Handler: retrieves entity, returns `ApplicationDto`. Returns null/throws if not found.

**GetApplications**:
- Query: `GetApplications()`
- Handler: retrieves all, returns `IReadOnlyList<ApplicationDto>`

### 3. Repository

`IApplicationRepository` in Application layer:
- `Task<ApplicationDto?> GetByIdAsync(Guid id)`
- `Task<IReadOnlyList<ApplicationDto>> GetAllAsync()`
- `Task<ApplicationDto> AddAsync(Domain.Entities.Application application)`
- `Task<ApplicationDto> UpdateAsync(Domain.Entities.Application application)`
- `Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)`

Implementation in Infrastructure returns DTOs (projecting from persistence entities).

### 4. Validation (FluentValidation)

- `CreateApplicationValidator`: Name required, max 256, Comments max 1024
- `UpdateApplicationValidator`: Id required (not empty), Name required, max 256, Comments max 1024
- Wolverine's FluentValidation middleware handles validation pipeline automatically

### 5. API Controller

```csharp
[ApiController]
[Route("applications")]
public class ApplicationsController : ControllerBase
{
    // POST /applications → 201 Created
    // PUT /applications/{id} → 200 OK / 404 Not Found
    // GET /applications/{id} → 200 OK / 404 Not Found
    // GET /applications → 200 OK
}
```

### 6. Database

- PostgreSQL via Npgsql.EntityFrameworkCore.PostgreSQL
- Connection string in `appsettings.json` (placeholder value), actual value from environment/user secrets
- Unique index on `Name` column for uniqueness constraint

---

## Implementation Order

1. **Domain Layer**: Create `DomainException`, then `Application` entity
2. **Infrastructure Layer**: Add EF Core/Npgsql packages, create `AppDbContext`, `ApplicationConfiguration`, generate initial migration
3. **Application Layer**: Add WolverineFx/FluentValidation packages, create DTOs, repository interface, commands, queries, validators, mappings
4. **Infrastructure Layer**: Implement `ApplicationRepository`, `DependencyInjection`
5. **API Layer**: Create request/response models, controller, update `Program.cs`
6. **End-to-end verification**: Build, run migration, test CRUD endpoints

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | PostgreSQL is the database | The tech-stack and security rules reference PostgreSQL and Npgsql; the about.md describes a configuration service backed by a database |
| A2 | EF Core Code-First with migrations | Architecture rules specify EF Core; migrations folder already exists in Infrastructure |
| A3 | WolverineFx is the CQRS mediator | Architecture explicitly mandates WolverineFx for commands/queries and FluentValidation middleware |
| A4 | No authentication yet | The work item says "administrator" but no auth mechanism is specified; security rules say add `[Authorize]` but that would block all endpoints without an auth system — deferred to a future auth work item |
| A5 | Name uniqueness is enforced at DB and app level | AC5 requires unique names; we check in app layer before create/update AND use a DB unique index |
| A6 | "associated with related configuration IDs" in AC is future scope | The current model (id, name, comments) has no configuration relationship field; this is likely a future feature |
| A7 | Responses use RFC 7807 Problem Details | Security rules mandate this; .NET 10 has built-in support via `AddProblemDetails()` |

---

## Questions Requiring Clarification

| # | Question |
|---|----------|
| Q1 | Should we add authentication now (the story says "As an administrator") or defer it? Security rules require `[Authorize]` on all non-public endpoints but no auth infrastructure exists yet. |
| Q2 | The acceptance criteria mentions "associated with related configuration IDs" but the model only has id/name/comments. Is the configuration relationship out of scope for this feature? |
| Q3 | Should we add a `DELETE /applications/{id}` endpoint? The work item doesn't list it, but CRUD is typically full. |
| Q4 | Connection string — should we use User Secrets for development or just a placeholder in `appsettings.Development.json`? |
| Q5 | Should the GET `/applications` endpoint support pagination, or return all applications? |
