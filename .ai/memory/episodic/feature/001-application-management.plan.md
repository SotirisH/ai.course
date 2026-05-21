# Feature Plan: Application Management (Ticket #001)

## Story Summary
As an administrator, I want to be able to manage applications in the system so that I can organize and track configuration data for various applications.

## Work Item Details
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Work Item File**: docs/01_Application_feature.md

## Acceptance Criteria (Given-When-Then)

### POST `/applications` - Create Application
**Given** I am an administrator
**When** I submit a valid application with name and comments
**Then** the system should create a new application with a unique GUID and return the created application with HTTP 201 status

### PUT `/applications/{id}` - Update Application
**Given** I am an administrator
**When** I submit valid updates for an existing application
**Then** the system should update the application and return the updated application with HTTP 200 status

### GET `/applications/{id}` - Get Application by ID
**Given** I am an administrator
**When** I request an application by its ID
**Then** the system should return the application details with HTTP 200 status
**And** return 404 if the application does not exist

### GET `/applications` - List Applications
**Given** I am an administrator
**When** I request the list of applications
**Then** the system should return all applications with HTTP 200 status

## Test Strategy
1. **Unit Tests**:
   - Domain entity tests (Application entity creation, validation)
   - Application layer tests (Command/Query handlers, mapping)
   - Repository interface tests (mocked)

2. **Integration Tests**:
   - API endpoint tests (Controller actions)
   - Database integration tests (Repository implementations)

3. **Manual Tests**:
   - Test all endpoints with Swagger UI
   - Verify unique constraint on name field
   - Verify GUID generation

## File Change List

### Domain Layer (`Ai.Api.Domain`)
1. **Entities/Application.cs** (NEW)
   - Application entity with Id (Guid), Name (string, max 256), Comments (string, max 1024)
   - Protected constructor for EF Core
   - Business logic methods if needed

2. **Interfaces/IApplicationRepository.cs** (NEW)
   - Repository interface with CRUD operations
   - GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, ExistsByNameAsync

### Application Layer (`Ai.Api.Application`)
1. **DTOs/ApplicationDto.cs** (NEW)
   - Id, Name, Comments properties

2. **DTOs/CreateApplicationRequest.cs** (NEW)
   - Name, Comments properties with validation

3. **DTOs/UpdateApplicationRequest.cs** (NEW)
   - Id, Name, Comments properties with validation

4. **Commands/CreateApplicationCommand.cs** (NEW)
   - MediatR command with Name and Comments

5. **Commands/CreateApplicationCommandHandler.cs** (NEW)
   - Handler that creates application via repository

6. **Commands/UpdateApplicationCommand.cs** (NEW)
   - MediatR command with Id, Name, Comments

7. **Commands/UpdateApplicationCommandHandler.cs** (NEW)
   - Handler that updates application via repository

8. **Queries/GetApplicationByIdQuery.cs** (NEW)
   - MediatR query with Id

9. **Queries/GetApplicationByIdQueryHandler.cs** (NEW)
   - Handler that retrieves application by ID

10. **Queries/GetApplicationsQuery.cs** (NEW)
    - MediatR query (no parameters for listing all)

11. **Queries/GetApplicationsQueryHandler.cs** (NEW)
    - Handler that retrieves all applications

12. **Mappings/ApplicationMappingExtensions.cs** (NEW)
    - Extension methods for mapping between Entity and DTOs

13. **Interfaces/IApplicationService.cs** (NEW) - OPTIONAL
    - Service interface if needed (may use MediatR directly)

14. **Validators/CreateApplicationRequestValidator.cs** (NEW)
    - FluentValidation for create request

15. **Validators/UpdateApplicationRequestValidator.cs** (NEW)
    - FluentValidation for update request

### Infrastructure Layer (`Ai.Api.Infrastructure`)
1. **Persistence/Context/ApplicationDbContext.cs** (NEW)
   - DbContext with Applications DbSet
   - Model configuration

2. **Persistence/Configurations/ApplicationConfiguration.cs** (NEW)
   - EF Core entity configuration
   - Unique constraint on Name

3. **Persistence/Repositories/ApplicationRepository.cs** (NEW)
   - Implementation of IApplicationRepository

4. **DependencyInjection.cs** (NEW)
   - Extension method to register infrastructure services
   - Register DbContext, Repositories

### API Layer (`Ai.Api`)
1. **Controllers/ApplicationsController.cs** (NEW)
   - POST, PUT, GET endpoints
   - Use MediatR to send commands/queries

2. **Requests/CreateApplicationRequest.cs** (NEW) - OPTIONAL
   - Can reuse Application layer DTOs

3. **Requests/UpdateApplicationRequest.cs** (NEW) - OPTIONAL
   - Can reuse Application layer DTOs

4. **Program.cs** (MODIFY)
   - Add MediatR registration
   - Add DbContext registration
   - Add Infrastructure services
   - Add FluentValidation if used

5. **Ai.Api.csproj** (MODIFY)
   - Add package references: MediatR, EF Core, Npgsql, FluentValidation

## Implementation Details

### Domain Entity Design
```csharp
public class Application
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Comments { get; private set; }

    private Application() { } // For EF Core

    public Application(string name, string? comments)
    {
        Id = Guid.NewGuid();
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

### Repository Interface
```csharp
public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<IEnumerable<Application>> GetAllAsync();
    Task AddAsync(Application application);
    Task UpdateAsync(Application application);
    Task<bool> ExistsByNameAsync(string name);
}
```

### MediatR Pattern
- Commands: CreateApplicationCommand, UpdateApplicationCommand
- Queries: GetApplicationByIdQuery, GetApplicationsQuery
- Handlers contain business logic and call repository

### Controller Endpoints
- POST `/api/applications` → CreateApplicationCommand
- PUT `/api/applications/{id}` → UpdateApplicationCommand
- GET `/api/applications/{id}` → GetApplicationByIdQuery
- GET `/api/applications` → GetApplicationsQuery

## Implementation Order

1. **Domain Layer** (no dependencies)
   - Create Application entity
   - Create IApplicationRepository interface

2. **Application Layer** (depends on Domain)
   - Create DTOs
   - Create Commands and Queries
   - Create Handlers
   - Create Mapping extensions
   - Create Validators

3. **Infrastructure Layer** (depends on Domain and Application)
   - Create ApplicationDbContext
   - Create ApplicationConfiguration
   - Create ApplicationRepository
   - Create DependencyInjection registration

4. **API Layer** (depends on all layers)
   - Create ApplicationsController
   - Update Program.cs with all registrations
   - Update Ai.Api.csproj with packages

5. **Testing** (throughout)
   - Unit tests for each layer
   - Integration tests for API

## Assumptions

1. **Database**: Using PostgreSQL as indicated in persona.md
   - **Justification**: The persona.md specifies PostgreSQL expertise and the project appears to be set up for it.

2. **ORM**: Using Entity Framework Core with Npgsql provider
   - **Justification**: Standard for .NET projects and aligns with Clean Architecture patterns.

3. **CQRS Pattern**: Using MediatR for command/query separation
   - **Justification**: Explicitly mentioned in architecture.md as a requirement ("Always use MediatR").

4. **No authentication yet**: Endpoints are public for now
   - **Justification**: Security.md mentions adding `[Authorize]` but the story doesn't specify authentication requirements. Will add later per security guidelines.

5. **No pagination for list endpoint**: Get all applications without pagination
   - **Justification**: Acceptance criteria doesn't mention pagination. Can be added later as an enhancement.

6. **FluentValidation for validation**: Using FluentValidation library
   - **Justification**: Mentioned in architecture.md Validators folder description and security.md for input validation.

7. **Manual mapping**: Using extension methods, not AutoMapper
   - **Justification**: Architecture.md states "favor manual mapping" and "default to use extension classes for mapping instead of automapper".

8. **Unique name constraint**: Enforced at database level
   - **Justification**: Acceptance criteria states name should be unique. EF Core configuration will add unique index.

## Questions to Resolve Before Implementation

1. **Database connection**: What is the PostgreSQL connection string? Should it be stored in appsettings.json or environment variables?
   - *Recommendation*: Use appsettings.Development.json for dev and environment variables for production (per security.md).

2. **Error handling**: Should we implement RFC 7807 Problem Details now or use custom error responses?
   - *Recommendation*: Implement Problem Details as mentioned in security.md for standards compliance.

3. **API versioning**: Should we add API versioning from the start?
   - *Recommendation*: Add basic API versioning as mentioned in persona.md API Design section.

4. **Configuration IDs**: The acceptance criteria mentions "associated with related configuration IDs" but the model doesn't include this. Should we add a relationship?
   - *Recommendation*: Skip for now as the model doesn't define it. Can be added in a future ticket.

5. **Swagger/OpenAPI**: Should we add XML comments and Swagger response types?
   - *Recommendation*: Yes, add basic OpenAPI documentation for all endpoints.

