# Application Management Plan

## Story Summary
As an administrator, I want to be able to manage applications in the system.

## Acceptance Criteria (Given-When-Then)

**Given** I am an administrator
**When** I create a new application via POST `/applications`
**Then** the application is created and returned with a unique identifier (201 Created)

**Given** I am an administrator
**When** I update an existing application via PUT `/applications/{id}`
**Then** the application is updated and the updated resource is returned (200 OK)

**Given** I am an administrator
**When** I retrieve an application via GET `/applications/{id}`
**Then** the application with the given id is returned (200 OK), or 404 if not found

**Given** I am an administrator
**When** I list applications via GET `/applications`
**Then** a list of all applications is returned (200 OK)

## Test Strategy and File Changes Identified

### Test Strategy
- Unit tests for FluentValidation validators (CreateApplication, UpdateApplication)
- Unit tests for Wolverine command/query handlers (mocked repository)
- Unit tests for Application entity (name validation, creation rules)
- Integration tests for API endpoints (in-memory EF Core or Testcontainers PostgreSQL)

### NuGet Packages to Install (Prerequisites)

| Project | Package | Version | Reason |
|---|---|---|---|
| Ai.Api.Domain | *none needed* | -- | Pure domain, no external deps |
| Ai.Api.Application | WolverineFx | 3.x | CQRS mediator per architecture rules (Section 3: "Always use wolverinefx") |
| Ai.Api.Application | FluentValidation | 12.x | Input validation per architecture rules |
| Ai.Api.Application | FluentValidation.DependencyInjectionExtensions | 12.x | DI registration for validators |
| Ai.Api.Infrastructure | Microsoft.EntityFrameworkCore | 10.x | EF Core data access |
| Ai.Api.Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL | 10.x | PostgreSQL provider |
| Ai.Api | Microsoft.EntityFrameworkCore.Design | 10.x | EF Core tooling (migrations) |

### File Change List

#### Domain Layer (`Ai.Api.Domain/`)
- `Entities/Application.cs` -- **NEW**: Application aggregate root entity

#### Application Layer (`Ai.Api.Application/`)
- `Features/Applications/DTOs/ApplicationDto.cs` -- **NEW**: Application output DTO (internal use-case result)
- `Features/Applications/Commands/CreateApplication/CreateApplicationCommand.cs` -- **NEW**
- `Features/Applications/Commands/CreateApplication/CreateApplicationCommandHandler.cs` -- **NEW**
- `Features/Applications/Commands/UpdateApplication/UpdateApplicationCommand.cs` -- **NEW**
- `Features/Applications/Commands/UpdateApplication/UpdateApplicationCommandHandler.cs` -- **NEW**
- `Features/Applications/Queries/GetApplication/GetApplicationQuery.cs` -- **NEW**
- `Features/Applications/Queries/GetApplication/GetApplicationQueryHandler.cs` -- **NEW**
- `Features/Applications/Queries/GetApplications/GetApplicationsQuery.cs` -- **NEW**
- `Features/Applications/Queries/GetApplications/GetApplicationsQueryHandler.cs` -- **NEW**
- `Interfaces/Repositories/IApplicationRepository.cs` -- **NEW**: Repository interface
- `Validators/CreateApplicationCommandValidator.cs` -- **NEW**
- `Validators/UpdateApplicationCommandValidator.cs` -- **NEW**
- `Mappings/ApplicationMappingExtensions.cs` -- **NEW**: Manual mapping extensions (Domain ↔ DTO, Command → Domain)
- `DependencyInjection.cs` -- **NEW**: DI registration extensions

#### Infrastructure Layer (`Ai.Api.Infrastructure/`)
- `Persistence/Context/ApplicationDbContext.cs` -- **NEW**: EF Core DbContext
- `Persistence/Configurations/ApplicationEntityTypeConfiguration.cs` -- **NEW**
- `Persistence/Repositories/ApplicationRepository.cs` -- **NEW**
- `DependencyInjection.cs` -- **NEW**: DI registration extensions

#### API Layer (`Ai.Api/`)
- `Models/Requests/CreateApplicationRequest.cs` -- **NEW**: POST body contract
- `Models/Requests/UpdateApplicationRequest.cs` -- **NEW**: PUT body contract
- `Controllers/ApplicationsController.cs` -- **NEW**: REST controller (maps Requests → Commands, DTOs → Responses)
- `Program.cs` -- **MODIFY**: Register EF Core, Wolverine, FluentValidation, Infrastructure/Application DI
- `appsettings.Development.json` -- **MODIFY**: Add connection string

## Implementation Details

### Application Entity (`Ai.Api.Domain/Entities/Application.cs`)
```csharp
public class Application
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = null!;
    public string? Comments { get; private set; }

    private Application() { } // EF Core

    public Application(string name, string? comments = null)
    {
        Name = name;
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        Name = name;
        Comments = comments;
    }
}
```

Key design decisions:
- `Guid.CreateVersion7()` for sequential GUID performance per architecture rules
- Private parameterless constructor for EF Core
- Encapsulated mutation via `Update()` method
- Validation in property setters (Name not null/whitespace)

### DTOs

**Application Layer** (`Ai.Api.Application/Features/Applications/DTOs/`):
- **`ApplicationDto`**: `Id` (Guid), `Name` (string), `Comments` (string?) — internal use-case output only

**API Layer** (`Ai.Api/Models/Requests/`):
- **`CreateApplicationRequest`**: `Name` (string, required), `Comments` (string?, optional) — public POST contract
- **`UpdateApplicationRequest`**: `Name` (string, required), `Comments` (string?, optional) — public PUT contract

All use C# `record` types per architecture rules (Section 6: "Use records for... DTOs, Commands, Queries").

### Data Flow

```
POST /api/applications
  CreateApplicationRequest  ──controller maps──→  CreateApplicationCommand  ──handler maps──→  Application (domain)
                                                                                                  │
                                                                                           IApplicationRepository
                                                                                                  │
GET  /api/applications/{id}                                                                       ▼
  ApplicationDto  ←──handler maps──  Application (domain)  ←──repository──  ApplicationEntity (infra)
      │
      ▼
  controller returns 200 OK + body (ApplicationDto serialized directly)
```

### Commands & Queries (Wolverine)
Commands (modify state):
- `CreateApplicationCommand(Name, Comments)` → returns `ApplicationDto`
- `UpdateApplicationCommand(Id, Name, Comments)` → returns `ApplicationDto`

Queries (return data):
- `GetApplicationQuery(Id)` → returns `ApplicationDto?`
- `GetApplicationsQuery()` → returns `List<ApplicationDto>`

All are records. Handlers are discovered automatically by Wolverine via convention.

### Validation Rules (FluentValidation)
**CreateApplicationCommandValidator** & **UpdateApplicationCommandValidator**:
- `Name`: Required, max length 256, trimmed
- `Comments`: Optional, max length 1024, trimmed if provided
- `Id` (Update only): Required, not empty

Note: Validation lives in the Application layer on Commands, not on API Request models. The controller maps Requests → Commands, and Wolverine/FluentValidation validates the Commands before handlers execute.

### Persistence Configuration (EF Core)
- Table: `Applications`
- `Id`: PK, Guid, clustered, generated by application (no DB generation)
- `Name`: Required, nvarchar(256), unique index
- `Comments`: Optional, nvarchar(1024)

### API Endpoints

| Method | Route | Request Body | Response |
|---|---|---|---|
| POST | `/api/applications` | CreateApplicationRequest | 201 Created + ApplicationDto |
| PUT | `/api/applications/{id}` | UpdateApplicationRequest | 200 OK + ApplicationDto |
| GET | `/api/applications/{id}` | -- | 200 OK + ApplicationDto / 404 |
| GET | `/api/applications` | -- | 200 OK + List\<ApplicationDto\> |

**Route prefix**: Uses `/api/applications` for consistency with existing `HealthController` which uses `[Route("api/[controller]")]`. The work item shows plain `/applications` but consistency within the codebase takes priority. See Questions section.

### Error Handling
- **400 Bad Request**: FluentValidation errors (via Wolverine middleware / manual validation in handler)
- **404 Not Found**: When application with given id does not exist
- **409 Conflict**: When attempting to create an application with a duplicate name
- **500 Internal Server Error**: Unexpected errors (via Problem Details per RFC 7807)

### Mapping Strategy
Manual extension methods:

**Application Layer** (`Ai.Api.Application/Mappings/ApplicationMappingExtensions.cs`):
- `Application` → `ApplicationDto`
- `CreateApplicationCommand` → `Application` (factory method)
- `UpdateApplicationCommand` → applies `Application.Update()` on existing entity

**API Layer** (inline in controller, or `Ai.Api/Mappings/`):
- `CreateApplicationRequest` → `CreateApplicationCommand`
- `UpdateApplicationRequest` + route `{id}` → `UpdateApplicationCommand`

**Infrastructure Layer** (inline in repository):
- `Application` ↔ `ApplicationEntity` (persistence entity)

Per architecture rules: "Generally favor manual mapping. Create extensions for mapping."

## Implementation Order

1. **Install NuGet packages** across all projects
2. **Domain**: Create `Application` entity (`Entities/Application.cs`)
3. **Application — Interfaces**: Create `IApplicationRepository`
4. **Application — DTOs**: Create `ApplicationDto`
5. **API — Models**: Create `CreateApplicationRequest`, `UpdateApplicationRequest`
6. **Application — Validators**: Create validators for create/update commands
7. **Application — Commands**: Create `CreateApplicationCommand` + handler, `UpdateApplicationCommand` + handler
8. **Application — Queries**: Create `GetApplicationQuery` + handler, `GetApplicationsQuery` + handler
9. **Application — Mappings**: Create mapping extension methods
10. **Application — DI**: Create `DependencyInjection` registration class
11. **Infrastructure — Context**: Create `ApplicationDbContext`
12. **Infrastructure — Configuration**: Create `ApplicationEntityTypeConfiguration`
13. **Infrastructure — Repository**: Create `ApplicationRepository`
14. **Infrastructure — DI**: Create `DependencyInjection` registration class
15. **API — Controller**: Create `ApplicationsController`
16. **API — Program.cs**: Wire up DI (EF Core, Wolverine, FluentValidation, Infrastructure/Application modules)
17. **API — Config**: Add connection string to `appsettings.Development.json`
18. **Generate initial migration**: `dotnet ef migrations add CreateApplicationsTable`

## Assumptions

| # | Assumption | Justification |
|---|---|---|
| 1 | **PostgreSQL is the database** | Derived from `about.md` (PostgreSQL expertise in persona), security rules (PostgreSQL-specific security guidance), and scaffold plan referencing Npgsql. |
| 2 | **WolverineFx for CQRS (NOT MediatR)** | Architecture rules Section 3: "Always use wolverinefx MediatR for handling commands and queries." The documentation URL points to Wolverine's mediator. The old plan incorrectly used the MediatR NuGet package. |
| 3 | **FluentValidation for validation** | Architecture rules: "Input validation in Application layer (Validators folder)... Uses FluentValidation." |
| 4 | **Manual mapping over AutoMapper** | Architecture rules: "Generally favor manual mapping. Create extensions for mapping." |
| 5 | **EF Core as ORM** | Architecture rules reference EF Core entity configurations and DbContext. |
| 6 | **Repository pattern** | Architecture rules: "Define repository interfaces in Application layer... Implement in Infrastructure layer." |
| 7 | **Route prefix `/api`** | Existing `HealthController` uses `[Route("api/[controller]")]`. Consistency across the codebase. Flagged as a question. |
| 8 | **No DELETE endpoint** | Work item explicitly lists only POST, PUT, GET/{id}, GET. DELETE is not mentioned. |
| 9 | **No authentication/authorization yet** | `Program.cs` calls `UseAuthorization()` but no auth is configured. `[Authorize]` from security rules should be added but won't enforce anything until auth is wired up. |
| 10 | **No pagination for GET** | Work item does not mention pagination. Simple list return is sufficient for now. |
| 11 | **Duplicate name → 409 Conflict** | Standard REST practice for uniqueness constraint violations. PostgreSQL unique index will enforce at DB level; handler catches `DbUpdateException` and returns 409. |
| 12 | **Features folder structure** | Architecture rules specify `Features/{FeatureName}/Commands/`, `Features/{FeatureName}/Queries/`, `Features/{FeatureName}/DTOs/`. This differs from the old plan's flat structure. |

## Questions Needing Clarification

| # | Question |
|---|---|
| 1 | **Route prefix**: Should endpoints be at `/applications` (as written in the work item) or `/api/applications` (consistent with existing `HealthController`)? |
| 2 | **"Associated with related configuration IDs"**: The acceptance criteria mention this, but the Application model shows no such property. Is this a future concern, or should the entity include a configuration relationship now? |
| 3 | **DELETE operation**: The work item shows CRU (no D). Is DELETE intentionally omitted or should it be included? |
| 4 | **Pagination**: Should GET `/applications` support pagination (page/pageSize) from the start, or is returning all records acceptable? |
| 5 | **Duplicate name response**: Is 409 Conflict appropriate, or would the team prefer a different status code (e.g., 422 Unprocessable Entity)? |
