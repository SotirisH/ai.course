# Plan: Application Management Feature

**Ticket:** 001  
**Feature:** Application Management  
**Type:** feature  
**Date:** 2026-06-01

---

## Story Summary

As an administrator, I want to be able to manage applications in the system. This involves full CRUD-lite operations (Create, Update, Retrieve, List) on an `Application` resource with fields: unique identifier (Guid), unique name (string up to 256 chars), and optional comments (string up to 1024 chars).

---

## Acceptance Criteria (Given-When-Then)

### AC1: Create Application
- **Given** an administrator provides a valid application name and optional comments
- **When** they submit a POST request to `/applications`
- **Then** a new application is created with a unique GUID, persisted to the database, and a 201 Created response is returned with the application resource

### AC2: Update Application
- **Given** an existing application identified by its id
- **When** an administrator submits a PUT request to `/applications/{id}` with updated name and/or comments
- **Then** the application is updated, persisted, and a 200 OK response returns the updated resource

### AC3: Retrieve Application
- **Given** an existing application identified by its id
- **When** an administrator submits a GET request to `/applications/{id}`
- **Then** the application details are returned with a 200 OK response; or 404 if not found

### AC4: List Applications
- **Given** one or more applications exist in the system
- **When** an administrator submits a GET request to `/applications`
- **Then** a 200 OK response returns a list of all applications

### AC5: Unique Name Enforcement
- **Given** an application with name "MyApp" already exists
- **When** an administrator attempts to create or update an application with the same name
- **Then** a 409 Conflict response is returned with an appropriate error message

### AC6: Input Validation
- **Given** invalid input (empty name, name exceeding 256 chars, comments exceeding 1024 chars)
- **When** a create or update request is submitted
- **Then** a 400 Bad Request response is returned with validation error details (RFC 7807 Problem Details)

---

## Test Strategy

### Unit Tests
- Domain entity construction and validation logic
- Command/Query handlers (with mocked repository)
- FluentValidation validators
- Mapping extensions (domain ↔ application DTO ↔ API response)

### Integration Tests
- Controller endpoints with in-memory test database (EF Core InMemory or Testcontainers PostgreSQL)
- Repository implementation against real database
- Full request pipeline (DTO validation → handler → repository → response)

### Manual Testing
- Swagger UI in Development environment for exploratory testing

---

## File Changes

### New Files

| Layer | File | Purpose |
|-------|------|---------|
| **Domain** | `src/Ai.Api.Domain/Entities/Application.cs` | Application domain entity with encapsulated business rules |
| **Domain** | `src/Ai.Api.Domain/Exceptions/DomainException.cs` | Base domain exception class |
| **Application** | `src/Ai.Api.Application/Interfaces/Repositories/IApplicationRepository.cs` | Repository interface |
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/Commands/CreateApplication.cs` | Create command + handler |
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/Commands/UpdateApplication.cs` | Update command + handler |
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/Queries/GetApplication.cs` | Get-by-id query + handler |
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/Queries/ListApplications.cs` | List-all query + handler |
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/DTOs/ApplicationDto.cs` | Application DTO for internal use |
| **Application** | `src/Ai.Api.Application/Validators/CreateApplicationValidator.cs` | FluentValidation validator for create |
| **Application** | `src/Ai.Api.Application/Validators/UpdateApplicationValidator.cs` | FluentValidation validator for update |
| **Application** | `src/Ai.Api.Application/Mappings/ApplicationMappingExtensions.cs` | Manual mapping extension methods |
| **Infrastructure** | `src/Ai.Api.Infrastructure/Persistence/Context/AppDbContext.cs` | EF Core DbContext |
| **Infrastructure** | `src/Ai.Api.Infrastructure/Persistence/Configurations/ApplicationConfiguration.cs` | EF Core entity type configuration |
| **Infrastructure** | `src/Ai.Api.Infrastructure/Persistence/Repositories/ApplicationRepository.cs` | Repository implementation |
| **API** | `src/Ai.Api/Controllers/ApplicationsController.cs` | REST controller for /applications |
| **API** | `src/Ai.Api/Models/Requests/CreateApplicationRequest.cs` | API request model (POST) |
| **API** | `src/Ai.Api/Models/Requests/UpdateApplicationRequest.cs` | API request model (PUT) |
| **API** | `src/Ai.Api/Models/Responses/ApplicationResponse.cs` | API response model |
| **API** | `src/Ai.Api/Models/Responses/ApplicationListResponse.cs` | API list response model |

### Modified Files

| Layer | File | Change |
|-------|------|--------|
| **API** | `src/Ai.Api/Program.cs` | Register DbContext, repositories, Wolverine mediator, FluentValidation, ProblemDetails |
| **API** | `src/Ai.Api/Ai.Api.csproj` | Add WolverineFx package reference |
| **Application** | `src/Ai.Api.Application/Ai.Api.Application.csproj` | Add WolverineFx, FluentValidation packages |
| **Infrastructure** | `src/Ai.Api.Infrastructure/Ai.Api.Infrastructure.csproj` | Add EF Core PostgreSQL packages |

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
        SetName(name);
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        SetName(name);
        Comments = comments;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Application name is required.");
        if (name.Length > 256)
            throw new DomainException("Application name must not exceed 256 characters.");
        Name = name;
    }
}
```

### 2. CQRS with Wolverine
- **Commands**: `CreateApplicationCommand`, `UpdateApplicationCommand` → return `ApplicationDto`
- **Queries**: `GetApplicationQuery` → `ApplicationDto?`, `ListApplicationsQuery` → `IReadOnlyList<ApplicationDto>`
- Handlers follow Wolverine conventions (classes ending in `Handler` or implementing Wolverine's handler pattern)

### 3. Validation (FluentValidation)
- `CreateApplicationValidator`: Name required, max 256; Comments max 1024
- `UpdateApplicationValidator`: Same as create + Id required
- Registered as Wolverine middleware / pipeline behavior

### 4. Persistence (EF Core + PostgreSQL)
- `AppDbContext` with `DbSet<Application> Applications`
- `ApplicationConfiguration` uses `IEntityTypeConfiguration<Application>` for:
  - Table name: `applications`
  - PK on `Id`
  - Unique index on `Name`
  - Max length constraints on `Name` (256) and `Comments` (1024)
- Repository translates domain entity ↔ persistence entity (same class, no separate ORM entity needed since domain entity can serve as EF entity when using private setters)

### 5. API Controller
- `ApplicationsController` with route `[Route("applications")]` (aligned with work item spec, not prefixed with `api/`)
- Returns `ApplicationResponse` / `ApplicationListResponse` records
- Maps API request → Wolverine command/query → invokes via `IMessageBus`
- Returns appropriate HTTP status codes: 200, 201, 400, 404, 409

### 6. DI Registration
- Infrastructure: `AddInfrastructure(connectionString)` extension method
- Application: `AddApplication()` extension method
- API: Call both in `Program.cs`

---

## Implementation Order

1. **Domain layer**: Create `Application` entity + `DomainException`
2. **Application layer**: Create DTOs, repository interface, commands, queries, validators, mappings
3. **Infrastructure layer**: Install EF Core packages, create DbContext, configuration, repository
4. **API layer**: Create request/response models, controller, wire up DI in Program.cs
5. **Generate migration**: `dotnet ef migrations add InitialCreate`
6. **Test manually** via Swagger UI

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | "associated with related configuration IDs" in the acceptance criteria refers to a future relationship (not in scope for this work item) | The model definition only includes id, name, and comments — no foreign key to configurations |
| A2 | Route prefix for the new controller is `/applications` (not `/api/applications`) | The work item explicitly states `POST /applications`, `GET /applications/{id}`, etc. without an `/api` prefix |
| A3 | EF Core with PostgreSQL is the database (not InMemory or SQL Server) | The `about.md` and `persona.md` specify PostgreSQL expertise; the scaffolded project includes `Infrastructure/Persistence/` structure consistent with EF Core |
| A4 | WolverineFx will be used as the CQRS mediator | `architecture.md` states "Always use wolverinefx MediatR for handling commands and queries" |
| A5 | The domain `Application` entity doubles as the EF Core entity (no separate persistence entity needed) | Private setters + parameterless constructor pattern enables EF Core materialization without a separate ORM entity class; this follows KISS principle |
| A6 | The name uniqueness constraint is enforced at both the domain level (repository check) and database level (unique index) | Defense in depth — domain check gives a meaningful error, DB index prevents race conditions |
| A7 | No authentication/authorization is implemented at this stage | The work item mentions "administrator" but there's no auth infrastructure yet; `[Authorize]` can be added when auth is implemented |
| A8 | Connection string will be read from `appsettings.json` / environment variables | Per security.md: "Store connection strings in environment variables or secret managers; never hardcode" |

---

## Questions for Clarification

| # | Question |
|---|----------|
| Q1 | What does "associated with related configuration IDs" mean? The model only has id/name/comments — should we add a `ConfigurationIds` collection or is this for a future work item? |
| Q2 | Should the route be `/applications` or `/api/applications`? The existing `HealthController` uses `api/[controller]` pattern but the work item specifies `/applications` directly. Which convention should we follow? |
| Q3 | Is PostgreSQL the confirmed database? Should we use a specific connection string or set it up later? |
| Q4 | Should the list endpoint support pagination/filtering, or is a simple "return all" sufficient for now? |
| Q5 | For the PUT (update) endpoint: should it be a full replace (all fields required) or a partial update (PATCH semantics)? |
