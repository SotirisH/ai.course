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
- Unit tests for FluentValidation validators (CreateApplicationCommand, UpdateApplicationCommand)
- Unit tests for MediatR command/query handlers (mocked repository)
- Unit tests for Application entity (name uniqueness enforcement)
- Integration tests for API endpoints (in-memory EF Core or Testcontainers PostgreSQL)
### File Change List
#### NuGet Packages to Install (Prerequisites)
| Project | Package | Version | Reason |
|---|---|---|---|
| Ai.Api.Domain | *none needed* | -- | Pure domain, no external deps |
| Ai.Api.Application | MediatR | 12.x | CQRS per architecture rules |
| Ai.Api.Application | FluentValidation | 12.x | Input validation per architecture rules |
| Ai.Api.Application | FluentValidation.DependencyInjectionExtensions | 12.x | DI registration |
| Ai.Api.Infrastructure | Microsoft.EntityFrameworkCore | 10.x | EF Core data access |
| Ai.Api.Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL | 10.x | PostgreSQL provider |
| Ai.Api.Infrastructure | MediatR | 12.x | Required for handler resolution |
| Ai.Api | Microsoft.EntityFrameworkCore.Design | 10.x | EF Core tooling (migrations) |
#### Domain Layer
- `Ai.Api.Domain/Entities/Application.cs` -- NEW: Application aggregate root entity
#### Application Layer
- `Ai.Api.Application/DTOs/ApplicationDto.cs` -- NEW: Response DTO
- `Ai.Api.Application/DTOs/CreateApplicationRequest.cs` -- NEW: Create request DTO
- `Ai.Api.Application/DTOs/UpdateApplicationRequest.cs` -- NEW: Update request DTO
- `Ai.Api.Application/Commands/CreateApplication/CreateApplicationCommand.cs` -- NEW
- `Ai.Api.Application/Commands/CreateApplication/CreateApplicationCommandHandler.cs` -- NEW
- `Ai.Api.Application/Commands/UpdateApplication/UpdateApplicationCommand.cs` -- NEW
- `Ai.Api.Application/Commands/UpdateApplication/UpdateApplicationCommandHandler.cs` -- NEW
- `Ai.Api.Application/Queries/GetApplication/GetApplicationQuery.cs` -- NEW
- `Ai.Api.Application/Queries/GetApplication/GetApplicationQueryHandler.cs` -- NEW
- `Ai.Api.Application/Queries/GetApplications/GetApplicationsQuery.cs` -- NEW
- `Ai.Api.Application/Queries/GetApplications/GetApplicationsQueryHandler.cs` -- NEW
- `Ai.Api.Application/Interfaces/Repositories/IApplicationRepository.cs` -- NEW: Repository interface
- `Ai.Api.Application/Validators/CreateApplicationCommandValidator.cs` -- NEW
- `Ai.Api.Application/Validators/UpdateApplicationCommandValidator.cs` -- NEW
- `Ai.Api.Application/Mappings/ApplicationMappingExtensions.cs` -- NEW: Manual mapping extensions
- `Ai.Api.Application/DependencyInjection.cs` -- NEW: DI registration extensions
#### Infrastructure Layer
- `Ai.Api.Infrastructure/Persistence/Context/ApplicationDbContext.cs` -- NEW: EF Core DbContext
- `Ai.Api.Infrastructure/Persistence/Configurations/ApplicationEntityTypeConfiguration.cs` -- NEW
- `Ai.Api.Infrastructure/Persistence/Repositories/ApplicationRepository.cs` -- NEW
- `Ai.Api.Infrastructure/DependencyInjection.cs` -- NEW: DI registration extensions
#### API Layer
- `Ai.Api/Controllers/ApplicationsController.cs` -- NEW: REST controller
- `Ai.Api/Program.cs` -- MODIFY: Register EF Core, MediatR, FluentValidation
- `Ai.Api/appsettings.Development.json` -- MODIFY: Add connection string
## Implementation Details
### Application Entity
- `Id`: Guid, primary key, generated on creation
- `Name`: string(256), required, unique
- `Comments`: string(1024), optional
- Encapsulated with private setter/protected constructor pattern
- Factory method `Create(name, comments)` and `Update(name, comments)` method
### DTOs
- `ApplicationDto`: Id, Name, Comments -- response shape
- `CreateApplicationRequest`: Name (required), Comments (optional) -- POST body
- `UpdateApplicationRequest`: Name (required), Comments (optional) -- PUT body
### Validation Rules
- **Name**: required, max length 256, trimmed
- **Comments**: optional, max length 1024, trimmed if provided
### Persistence Configuration
- Table name: `Applications`
- `Id`: Primary key, clustered, default `Guid.NewGuid()` generation
- `Name`: Required, max length 256, unique index
- `Comments`: Optional, max length 1024
### API Endpoints
| Method | Route | Request Body | Response |
|---|---|---|---|
| POST | `/api/applications` | CreateApplicationRequest | 201 + ApplicationDto |
| PUT | `/api/applications/{id}` | UpdateApplicationRequest | 200 + ApplicationDto |
| GET | `/api/applications/{id}` | -- | 200 + ApplicationDto / 404 |
| GET | `/api/applications` | -- | 200 + List`<ApplicationDto>` |
> **Route prefix note**: Work item specifies `/applications` without `/api` prefix. However, the existing `HealthController` uses `[Route("api/[controller]")]`. For consistency across the codebase and standard REST conventions, the plan uses `/api/applications`. This can be adjusted if the user prefers the plain `/applications` route.
### Error Handling
- 400 Bad Request: FluentValidation errors (automatic via MediatR pipeline or manual validation)
- 404 Not Found: When application with given id does not exist
- 409 Conflict: When attempting to create an application with a duplicate name
- 500 Internal Server Error: Unexpected errors
## Implementation Order
1. **Install NuGet packages** -- EF Core, MediatR, FluentValidation across all projects
2. **Domain**: Create `Application` entity
3. **Application -- Interfaces**: Create `IApplicationRepository`
4. **Application -- DTOs**: Create `ApplicationDto`, `CreateApplicationRequest`, `UpdateApplicationRequest`
5. **Application -- Validators**: Create validators for create/update commands
6. **Application -- Commands**: Create command/handler pairs for Create and Update
7. **Application -- Queries**: Create query/handler pairs for GetById and GetAll
8. **Application -- Mappings**: Create mapping extension methods
9. **Application -- DI**: Create `DependencyInjection` registration class
10. **Infrastructure -- Context**: Create `ApplicationDbContext`
11. **Infrastructure -- Configuration**: Create `ApplicationEntityTypeConfiguration`
12. **Infrastructure -- Repository**: Create `ApplicationRepository`
13. **Infrastructure -- DI**: Create `DependencyInjection` registration class
14. **API -- Controller**: Create `ApplicationsController`
15. **API -- Program.cs**: Wire up DI registrations and EF Core
16. **API -- Config**: Add connection string to `appsettings.Development.json`
17. **Generate initial migration**: `dotnet ef migrations add CreateApplicationsTable`
## Assumptions
1. **PostgreSQL is the database** -- derived from `about.md` and security rules referencing PostgreSQL best practices, and the scaffolding plan referencing Npgsql.
2. **MediatR is required for CQRS** -- per architecture rules: "Always use MediatR for handling commands and queries in Application layer."
3. **FluentValidation is the validation framework** -- per architecture rules: "Input validation in Application layer (Validators folder)... Uses FluentValidation or similar."
4. **Manual mapping over AutoMapper** -- per architecture rules: "Generally favor manual mapping. Create extensions for mapping."
5. **EF Core is the ORM** -- per architecture rules referencing EF Core entity configurations and DbContext.
6. **Repository pattern is required** -- per architecture rules: "Define repository interfaces in Application layer... Implement in Infrastructure layer."
7. **Route prefix `/api`** -- existing `HealthController` already uses `[Route("api/[controller]")]`. Work item shows `/applications` without prefix but consistency across the codebase suggests using the same prefix. This is flagged as a question.
8. **No soft delete** -- work item does not mention delete operations at all.
9. **No authentication/authorization yet** -- `Program.cs` calls `UseAuthorization()` but no auth is configured. The `[Authorize]` attribute from security rules will be applied on the controller but won't enforce anything until auth is wired up.
10. **No pagination for GET `/applications`** -- work item does not specify pagination; a simple `List<ApplicationDto>` return is sufficient for now.
## Questions Needing Clarification
1. **Route prefix**: Should the endpoints be at `/applications` (as written in the work item) or `/api/applications` (consistent with existing `HealthController`)?
2. **"Associated with related configuration IDs"**: The acceptance criteria mention this, but the model shows no such property. Is this a future concern or a mistake in the criteria?
3. **Duplicate name handling**: Should the API return 409 Conflict when creating/updating an application with a duplicate name, or a different status code?
4. **DELETE operation**: The work item shows only CRU (no D). Is delete intentionally omitted, or should it be included?
5. **Pagination**: Should GET `/applications` support pagination (page/pageSize) from the start, or is returning all records acceptable?
