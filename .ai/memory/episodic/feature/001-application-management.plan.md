# Implementation Plan: Application Management (Ticket #001)

## Story Summary
As an administrator, I want to be able to manage applications in the system. The system should allow administrators to create, update, retrieve, and list applications. Each application should have a unique identifier (GUID) and a unique name with optional comments.

## Acceptance Criteria (Given-When-Then)

### Scenario 1: Create a new application
**Given** an administrator provides valid application data (name, comments)
**When** they submit a POST request to `/applications`
**Then** a new application is created with a unique GUID identifier
**And** the application is persisted in the database
**And** a 201 Created response is returned with the application data

### Scenario 2: Update an existing application
**Given** an application exists with a specific ID
**And** an administrator provides valid updated data (name, comments)
**When** they submit a PUT request to `/applications/{id}`
**Then** the application is updated with the new data
**And** the changes are persisted in the database
**And** a 200 OK response is returned with the updated application data

### Scenario 3: Retrieve a specific application
**Given** an application exists with a specific ID
**When** an administrator submits a GET request to `/applications/{id}`
**Then** the application data is returned
**And** a 200 OK response is returned

### Scenario 4: List all applications
**When** an administrator submits a GET request to `/applications`
**Then** a list of all applications is returned
**And** a 200 OK response is returned

### Scenario 5: Application name uniqueness
**Given** an application with a specific name already exists
**When** an administrator tries to create another application with the same name
**Then** a 400 Bad Request response is returned with a validation error

### Scenario 6: Application not found
**Given** no application exists with a specific ID
**When** an administrator submits a GET or PUT request to `/applications/{id}`
**Then** a 404 Not Found response is returned

## Test Strategy and File Changes

### Test Strategy
Since this is the initial implementation of the Application feature, the following testing approach will be used:

1. **Unit Tests** (to be implemented in a separate test project):
   - Domain entity tests: Validate entity creation, business rules
   - Application layer tests: Validate command/query handlers, validators
   - Mapping tests: Ensure DTO mappings work correctly

2. **Integration Tests** (to be implemented in a separate test project):
   - API endpoint tests: Test HTTP requests/responses
   - Database integration tests: Test repository implementations with test database

3. **Manual Testing**:
   - Use Swagger UI to test all endpoints
   - Verify database state after operations

### File Changes Identified

#### Domain Layer (Ai.Api.Domain)
| File | Action | Description |
|------|--------|-------------|
| `Entities/Application.cs` | Create | Domain entity representing an Application with Id (Guid), Name (string, unique), Comments (string) |
| `Interfaces/IApplicationRepository.cs` | Create | Repository interface for application data access |

#### Application Layer (Ai.Api.Application)
| File | Action | Description |
|------|--------|-------------|
| `DTOs/ApplicationDto.cs` | Create | DTO for returning application data |
| `DTOs/CreateApplicationRequest.cs` | Create | DTO for creating a new application |
| `DTOs/UpdateApplicationRequest.cs` | Create | DTO for updating an existing application |
| `Mappings/ApplicationMappings.cs` | Create | Extension methods for mapping between Entity and DTOs |
| `Commands/CreateApplication/CreateApplicationCommand.cs` | Create | MediatR command for creating an application |
| `Commands/CreateApplication/CreateApplicationCommandHandler.cs` | Create | Handler for create application command |
| `Commands/UpdateApplication/UpdateApplicationCommand.cs` | Create | MediatR command for updating an application |
| `Commands/UpdateApplication/UpdateApplicationCommandHandler.cs` | Create | Handler for update application command |
| `Queries/GetApplicationById/GetApplicationByIdQuery.cs` | Create | MediatR query for getting application by ID |
| `Queries/GetApplicationById/GetApplicationByIdQueryHandler.cs` | Create | Handler for get application by ID query |
| `Queries/ListApplications/ListApplicationsQuery.cs` | Create | MediatR query for listing all applications |
| `Queries/ListApplications/ListApplicationsQueryHandler.cs` | Create | Handler for list applications query |
| `Validators/CreateApplicationValidator.cs` | Create | FluentValidation validator for create request |
| `Validators/UpdateApplicationValidator.cs` | Create | FluentValidation validator for update request |

#### Infrastructure Layer (Ai.Api.Infrastructure)
| File | Action | Description |
|------|--------|-------------|
| `Persistence/Context/ApplicationDbContext.cs` | Create | EF Core DbContext for the application |
| `Persistence/Configurations/ApplicationConfiguration.cs` | Create | EF Core entity configuration for Application |
| `Persistence/Repositories/ApplicationRepository.cs` | Create | Repository implementation for Application |
| `DependencyInjection.cs` | Create | Service registration for Infrastructure layer |
| `Migrations/` | Generate | EF Core migrations for Application entity |

#### API Layer (Ai.Api)
| File | Action | Description |
|------|--------|-------------|
| `Controllers/ApplicationsController.cs` | Create | API controller with endpoints for CRUD operations |
| `Program.cs` | Update | Register MediatR, FluentValidation, DbContext, and repositories |

#### Project Files (Update Package References)
| File | Action | Description |
|------|--------|-------------|
| `Ai.Api.slnx` | Verify | Ensure all projects are properly referenced |
| `src/Ai.Api.Infrastructure/Ai.Api.Infrastructure.csproj` | Update | Add EF Core and Npgsql packages |
| `src/Ai.Api.Application/Ai.Api.Application.csproj` | Update | Add MediatR and FluentValidation packages |
| `src/Ai.Api/Ai.Api.csproj` | Update | Add MediatR registration |

## Implementation Details

### Domain Entity: Application
```csharp
namespace Ai.Api.Domain.Entities;

public class Application
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }  // Unique, max 256 chars
    public string Comments { get; private set; }  // Max 1024 chars
    
    // Private constructor for EF Core
    private Application() { }
    
    // Factory method for creation
    public static Application Create(string name, string? comments)
    {
        ValidateName(name);
        ValidateComments(comments);
            
        return new Application
        {
            Id = Guid.NewGuid(),
            Name = name,
            Comments = comments ?? string.Empty
        };
    }
    
    // Update method
    public void Update(string name, string? comments)
    {
        ValidateName(name);
        ValidateComments(comments);
            
        Name = name;
        Comments = comments ?? string.Empty;
    }
    
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name is required", nameof(name));
        if (name.Length > 256)
            throw new ArgumentException("Application name cannot exceed 256 characters", nameof(name));
    }
    
    private static void ValidateComments(string? comments)
    {
        if (comments?.Length > 1024)
            throw new ArgumentException("Comments cannot exceed 1024 characters", nameof(comments));
    }
}
```

### Database Schema
```sql
CREATE TABLE Applications (
    Id UUID PRIMARY KEY,
    Name VARCHAR(256) NOT NULL UNIQUE,
    Comments VARCHAR(1024) NOT NULL DEFAULT ''
);
```

### API Endpoints
- `POST /api/applications` - Create application (returns 201 Created)
- `PUT /api/applications/{id}` - Update application (returns 200 OK)
- `GET /api/applications/{id}` - Get application by ID (returns 200 OK or 404 Not Found)
- `GET /api/applications` - List all applications (returns 200 OK with array)

### CQRS Pattern
Following the architecture rules, MediatR will be used for CQRS:
- **Commands**: CreateApplicationCommand, UpdateApplicationCommand
- **Queries**: GetApplicationByIdQuery, ListApplicationsQuery

### Error Handling
Following security.md recommendations, RFC 7807 Problem Details will be used for error responses:
- Validation errors: 400 Bad Request with Problem Details
- Not found: 404 Not Found with Problem Details
- Duplicate name: 400 Bad Request with Problem Details

## Implementation Order

1. **Domain Layer**
   - Create `Application` entity
   - Create `IApplicationRepository` interface

2. **Infrastructure Layer**
   - Create `ApplicationDbContext`
   - Create `ApplicationConfiguration`
   - Create `ApplicationRepository`
   - Create `DependencyInjection` for Infrastructure
   - Update `Ai.Api.Infrastructure.csproj` with packages
   - Generate and apply EF Core migration

3. **Application Layer**
   - Create DTOs (ApplicationDto, CreateApplicationRequest, UpdateApplicationRequest)
   - Create Commands (CreateApplicationCommand + Handler)
   - Create Commands (UpdateApplicationCommand + Handler)
   - Create Queries (GetApplicationByIdQuery + Handler)
   - Create Queries (ListApplicationsQuery + Handler)
   - Create Validators (CreateApplicationValidator, UpdateApplicationValidator)
   - Create Mappings (ApplicationMappings)
   - Update `Ai.Api.Application.csproj` with packages

4. **API Layer**
   - Create `ApplicationsController`
   - Update `Program.cs` with service registrations
   - Update `Ai.Api.csproj` with packages

5. **Build & Test**
   - Build solution and verify compilation
   - Test endpoints via Swagger UI

## Assumptions

1. **EF Core with PostgreSQL will be used**
   - Justification: The architecture.md specifies PostgreSQL as the database, and the persona.md mentions EF Core with clean persistence boundaries. The Infrastructure project will need Npgsql package.

2. **MediatR will be used for CQRS pattern**
   - Justification: The architecture.md explicitly states "Always use MediatR for handling commands and queries in Application layer."

3. **FluentValidation will be used for input validation**
   - Justification: The architecture.md mentions FluentValidation in the Application layer Validators folder, and security.md references FluentValidation.

4. **GUID will be used as primary key**
   - Justification: The work item specifies "datatype: guid" for the id field.

5. **Name uniqueness will be enforced at database level**
   - Justification: The work item specifies "name: unique", so we'll add a unique constraint in the EF Core configuration.

6. **No authentication/authorization will be implemented in this ticket**
   - Justification: The security.md mentions applying [Authorize] attributes, but the work item doesn't mention authentication requirements. This can be added later as a separate ticket.

7. **Configuration IDs mentioned in acceptance criteria will not be implemented in this ticket**
   - Justification: The acceptance criteria mentions "associated with related configuration IDs" but the Application model in the work item doesn't include this field. This appears to be future scope.

8. **Manual mapping will be used instead of AutoMapper**
   - Justification: The architecture.md states "favor manual mapping" and "create extensions for mapping instead of automapper."

9. **Comments field is optional**
   - Justification: The work item doesn't specify required for comments, so it can be null/empty.

10. **RFC 7807 Problem Details for error responses**
    - Justification: Security.md explicitly states "Ensure validation errors follow RFC 7807 Problem Details format."

## Questions

1. **Database Connection**: What is the PostgreSQL connection string that should be used during development? Should it be read from `appsettings.Development.json`?

2. **Configuration IDs**: The acceptance criteria mentions "associated with related configuration IDs" but the model doesn't include this. Should we add a collection of configuration IDs to the Application entity, or is this out of scope for this ticket?

3. **Pagination**: For the GET `/applications` endpoint, should we implement pagination, or return all applications for now?

4. **Soft Delete**: Should applications be soft-deleted (marked as inactive) or hard-deleted from the database? Note: Delete endpoint is not in the acceptance criteria, so this may not be applicable yet.

5. **CORS Policy**: Will the API need to support CORS for the admin UI? Should we add a basic CORS policy?

6. **Rate Limiting**: Should rate limiting be implemented as per security.md recommendations?
