# Ticket 001 — Application Management (PLAN)

## Story Summary
As an administrator, I need to manage applications in the system by creating, updating, retrieving, and listing them.

## Extracted Metadata
- work_item_type: `feature`
- ticket_num: `001`
- feature_name: `Application Management`

## Acceptance Criteria (Given / When / Then)
1. **Create application**
   - Given I am an administrator
   - When I send `POST /applications`
   - Then a new application is created with a unique identifier
2. **Update application**
   - Given an existing application
   - When I send `PUT /applications/{id}`
   - Then the application data is updated
3. **Get by id**
   - Given an existing application id
   - When I send `GET /applications/{id}`
   - Then the matching application is returned
4. **List applications**
   - Given applications exist in the system
   - When I send `GET /applications`
   - Then the full list of applications is returned

## Existing Plan Check
- Existing matching plan artifacts were detected.
- User decision: **Overwrite completely**.
- This plan is recreated from the current work item and codebase state.

## Test Strategy and File Changes Identified

### Test Strategy
- **Domain unit tests**
  - Validate entity creation/update invariants (name required, max lengths).
- **Application unit tests**
  - Command/query handler behavior with mocked repository.
  - Validator rules for create/update requests.
- **Infrastructure integration tests**
  - EF Core repository behavior against PostgreSQL test database.
  - Unique index enforcement on application name.
- **API integration tests**
  - Endpoint contracts for POST/PUT/GET by id/GET list.
  - Expected HTTP status codes for success/validation/not found/conflict.

### File Change List

#### Domain layer (`src/Ai.Api.Domain`)
- `Entities/Application.cs` (new)
  - Domain entity with `Id`, `Name`, `Comments`, constructors/factory/update methods.

#### Application layer (`src/Ai.Api.Application`)
- `DTOs/ApplicationDto.cs` (new)
- `DTOs/CreateApplicationRequest.cs` (new)
- `DTOs/UpdateApplicationRequest.cs` (new)
- `Interfaces/Repositories/IApplicationRepository.cs` (new)
- `Commands/CreateApplication/CreateApplicationCommand.cs` (new)
- `Commands/CreateApplication/CreateApplicationCommandHandler.cs` (new)
- `Commands/UpdateApplication/UpdateApplicationCommand.cs` (new)
- `Commands/UpdateApplication/UpdateApplicationCommandHandler.cs` (new)
- `Queries/GetApplicationById/GetApplicationByIdQuery.cs` (new)
- `Queries/GetApplicationById/GetApplicationByIdQueryHandler.cs` (new)
- `Queries/GetApplications/GetApplicationsQuery.cs` (new)
- `Queries/GetApplications/GetApplicationsQueryHandler.cs` (new)
- `Validators/CreateApplicationCommandValidator.cs` (new)
- `Validators/UpdateApplicationCommandValidator.cs` (new)
- `Mappings/ApplicationMappings.cs` (new)
- `DependencyInjection.cs` (new)

#### Infrastructure layer (`src/Ai.Api.Infrastructure`)
- `Persistence/Context/AiApiDbContext.cs` (new)
- `Persistence/Configurations/ApplicationEntityConfiguration.cs` (new)
- `Persistence/Repositories/ApplicationRepository.cs` (new)
- `DependencyInjection.cs` (new)
- `Migrations/*` (new, generated)

#### API layer (`src/Ai.Api`)
- `Controllers/ApplicationsController.cs` (new)
- `Program.cs` (modify)
  - Register Application/Infrastructure services
  - Add EF Core DbContext
  - Add MediatR + FluentValidation pipeline
- `appsettings.Development.json` (modify)
  - Add development connection string

#### Project dependencies (.csproj)
- Add required package references (pinned versions):
  - `MediatR`
  - `FluentValidation`
  - `FluentValidation.DependencyInjectionExtensions`
  - `Microsoft.EntityFrameworkCore`
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Microsoft.EntityFrameworkCore.Design` (API project for tooling)

## Implementation Details
- Implement clean architecture flow: Controller → MediatR Command/Query → Handler → Repository.
- Keep Domain free of infrastructure dependencies.
- Use manual mapping extension methods (per architecture guidance).
- Enforce database uniqueness on `Application.Name`.
- Return ProblemDetails-compatible validation responses.

## Implementation Order
1. Add NuGet packages in relevant projects.
2. Create Domain `Application` entity.
3. Add Application repository contract.
4. Add DTOs and mapping extensions.
5. Add create/update commands + validators + handlers.
6. Add get-by-id/get-all queries + handlers.
7. Add Application layer DI registration.
8. Add Infrastructure DbContext + configuration + repository.
9. Add Infrastructure DI registration.
10. Add `ApplicationsController` with CRUD(read/list) endpoints.
11. Wire service registrations in `Program.cs`.
12. Add connection string in `appsettings.Development.json`.
13. Generate and review EF migration.
14. Execute tests and endpoint verification.

## Assumptions (with justification)
1. **PostgreSQL is the target database.**
   - Justification: Global context and security guidelines reference PostgreSQL as the primary database standard.
2. **MediatR must be used for commands/queries.**
   - Justification: Architecture rules explicitly require MediatR for CQRS handlers.
3. **FluentValidation will be used for input validation.**
   - Justification: Architecture and security guidance emphasize validator-based input checks.
4. **Manual mappings are preferred over AutoMapper.**
   - Justification: Architecture guidance recommends mapping extensions by default.
5. **Controller-based API style is mandatory.**
   - Justification: Coding style explicitly forbids Minimal APIs.
6. **Delete endpoint is out of scope for this ticket.**
   - Justification: Acceptance criteria include create/update/get/list only.

## Open Questions Before Implementation
1. Should the final routes be exactly `/applications` or `/api/applications` to align with the current controller routing convention?
2. Acceptance criteria mentions association with related configuration IDs, but the current model omits this field. Should it be included now?
3. For duplicate application names, is `409 Conflict` the expected API behavior?
4. Should `GET /applications` include pagination from the start or return all records?

## PLAN Completion Checklist (Edit Mode)
- [x] Test strategy and file changes identified
- [x] Existing plan check completed
- [x] Feature branch active
- [x] Plan saved to `.ai/memory/episodic/feature/001-application-management.plan.md`
- [ ] Plan committed to feature branch

