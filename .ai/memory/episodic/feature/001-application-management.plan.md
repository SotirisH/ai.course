# Implementation Plan: Application Management

## Story Summary
As an administrator, I want to be able to manage applications in the system. The feature requires implementing full CRUD operations (Create, Read, Update, List) for applications via a RESTful API.

## Acceptance Criteria (Given-When-Then)

### Scenario 1: Create Application
- **Given** an administrator provides valid application data (name, comments)
- **When** they submit a POST request to `/applications`
- **Then** a new application is created with a unique GUID identifier
- **And** the application is persisted in the database
- **And** a 201 Created response is returned with the application data

### Scenario 2: Update Application
- **Given** an application exists with a specific ID
- **When** an administrator submits a PUT request to `/applications/{id}` with valid data
- **Then** the application is updated with the new values
- **And** a 200 OK response is returned with the updated application data

### Scenario 3: Get Application by ID
- **Given** an application exists with a specific ID
- **When** an administrator submits a GET request to `/applications/{id}`
- **Then** the application details are returned with a 200 OK response

### Scenario 4: List Applications
- **Given** applications exist in the system
- **When** an administrator submits a GET request to `/applications`
- **Then** a list of all applications is returned with a 200 OK response

### Scenario 5: Validation - Duplicate Name
- **Given** an application with a specific name already exists
- **When** an administrator tries to create another application with the same name
- **Then** a 400 Bad Request response is returned with a validation error

### Scenario 6: Validation - Not Found
- **Given** no application exists with a specific ID
- **When** an administrator submits a GET or PUT request to `/applications/{id}`
- **Then** a 404 Not Found response is returned

## Test Strategy

### Unit Tests
- **Domain Layer**: Test Application entity creation, validation rules
- **Application Layer**: Test command/query handlers, validators, mapping logic
- **Infrastructure Layer**: Test repository implementations (with mocked DbContext)

### Integration Tests
- **API Controllers**: Test HTTP endpoints with in-memory database
- **Persistence**: Test DbContext configurations and migrations

### File Changes Identified

#### Domain Layer (Ai.Api.Domain)
1. **Entities/Application.cs** (NEW)
   - Application entity with Id (Guid), Name (string, 256), Comments (string, 1024)
   - Business logic methods for updates

2. **Interfaces/IApplicationRepository.cs** (NEW)
   - Repository interface defining CRUD operations for Application

#### Application Layer (Ai.Api.Application)
3. **DTOs/ApplicationDto.cs** (NEW)
   - DTO for returning application data

4. **DTOs/CreateApplicationRequest.cs** (NEW)
   - DTO for creating a new application

5. **DTOs/UpdateApplicationRequest.cs** (NEW)
   - DTO for updating an existing application

6. **Commands/CreateApplication/CreateApplicationCommand.cs** (NEW)
   - MediatR command for creating application

7. **Commands/CreateApplication/CreateApplicationHandler.cs** (NEW)
   - Handler for create application command

8. **Commands/UpdateApplication/UpdateApplicationCommand.cs** (NEW)
   - MediatR command for updating application

9. **Commands/UpdateApplication/UpdateApplicationHandler.cs** (NEW)
   - Handler for update application command

10. **Queries/GetApplicationById/GetApplicationByIdQuery.cs** (NEW)
    - MediatR query for getting application by ID

11. **Queries/GetApplicationById/GetApplicationByIdHandler.cs** (NEW)
    - Handler for get application by ID query

12. **Queries/ListApplications/ListApplicationsQuery.cs** (NEW)
    - MediatR query for listing all applications

13. **Queries/ListApplications/ListApplicationsHandler.cs** (NEW)
    - Handler for list applications query

14. **Validators/CreateApplicationValidator.cs** (NEW)
    - FluentValidation validator for create request

15. **Validators/UpdateApplicationValidator.cs** (NEW)
    - FluentValidation validator for update request

16. **Mappings/ApplicationMappings.cs** (NEW)
    - Extension methods for mapping between entities and DTOs

#### Infrastructure Layer (Ai.Api.Infrastructure)
17. **Persistence/Context/ApplicationDbContext.cs** (NEW)
    - EF Core DbContext with Applications DbSet

18. **Persistence/Configurations/ApplicationConfiguration.cs** (NEW)
    - EF Core entity configuration for Application

19. **Persistence/Repositories/ApplicationRepository.cs** (NEW)
    - Repository implementation for Application

20. **DependencyInjection.cs** (NEW)
    - Service registration for Infrastructure layer

#### API Layer (Ai.Api)
21. **Controllers/ApplicationsController.cs** (NEW)
    - API controller with POST, PUT, GET endpoints

22. **Program.cs** (MODIFY)
    - Add MediatR, DbContext, and repository registrations

23. **Ai.Api.csproj** (MODIFY)
    - Add package references for MediatR, EF Core, Npgsql

24. **Ai.Api.Infrastructure.csproj** (MODIFY)
    - Add package references for EF Core, Npgsql

25. **Ai.Api.Application.csproj** (MODIFY)
    - Add package references for MediatR, FluentValidation

## Implementation Details

### Architecture Pattern
- **Clean Architecture** with separation: Domain → Application → Infrastructure → API
- **CQRS** using MediatR for commands and queries
- **Repository Pattern** for data access abstraction
- **FluentValidation** for input validation

### Database
- **PostgreSQL** with EF Core
- **Application Table**:
  - `id` (uuid, primary key, default gen_random_uuid())
  - `name` (varchar(256), unique, not null)
  - `comments` (varchar(1024), nullable)

### API Endpoints
- `POST /api/applications` - Create application
- `PUT /api/applications/{id}` - Update application
- `GET /api/applications/{id}` - Get application by ID
- `GET /api/applications` - List all applications

### Key Design Decisions
1. **GUID for ID**: Using Guid for primary key as specified in requirements
2. **No automapper**: Following architecture.md guidance to use manual mapping extensions
3. **MediatR for CQRS**: Following architecture.md requirement to always use MediatR
4. **FluentValidation**: For input validation in Application layer
5. **Repository Pattern**: Abstract data access behind interface in Domain, implement in Infrastructure

## Implementation Order

1. **Domain Layer**
   - Create Application entity
   - Create IApplicationRepository interface

2. **Infrastructure Layer**
   - Create ApplicationDbContext
   - Create ApplicationConfiguration
   - Create ApplicationRepository
   - Create DependencyInjection for Infrastructure
   - Update Ai.Api.Infrastructure.csproj with packages

3. **Application Layer**
   - Create DTOs (ApplicationDto, CreateApplicationRequest, UpdateApplicationRequest)
   - Create Commands (CreateApplicationCommand + Handler)
   - Create Commands (UpdateApplicationCommand + Handler)
   - Create Queries (GetApplicationByIdQuery + Handler)
   - Create Queries (ListApplicationsQuery + Handler)
   - Create Validators (CreateApplicationValidator, UpdateApplicationValidator)
   - Create Mappings (ApplicationMappings)
   - Update Ai.Api.Application.csproj with packages

4. **API Layer**
   - Create ApplicationsController
   - Update Program.cs with service registrations
   - Update Ai.Api.csproj with packages

5. **Database Migration**
   - Create initial migration
   - Apply migration to database

## Assumptions

1. **PostgreSQL Database**: Assumed PostgreSQL is the target database as mentioned in persona.md.
   - *Justification*: The persona specifies "C#, PostgreSQL and API design" as core expertise areas.

2. **MediatR for CQRS**: Assumed MediatR should be used for command/query handling.
   - *Justification*: Architecture.md explicitly states "Always use MediatR for handling commands and queries in Application layer."

3. **No Authentication in Initial Implementation**: Assumed authentication/authorization will be added later.
   - *Justification*: Security.md mentions applying [Authorize] attributes, but the work item doesn't specify auth requirements. This can be added as a separate task.

4. **Manual Mapping Over AutoMapper**: Assumed manual mapping extensions should be used.
   - *Justification*: Architecture.md states "favor manual mapping" and "default to use extension classes for mapping instead of automapper."

5. **FluentValidation for Validation**: Assumed FluentValidation should be used for input validation.
   - *Justification*: Architecture.md lists "Validators/" folder with "(FluentValidation)" and security.md mentions FluentValidation.

6. **Unique Name Constraint**: Assumed application name must be unique across the system.
   - *Justification*: The model shows "name: unique" and acceptance criteria mentions "Each application should have a unique identifier" - interpreted as unique name.

7. **Comments Field is Optional**: Assumed comments can be null/empty.
   - *Justification*: The work item doesn't specify required, and typically comments are optional.

8. **EF Core with PostgreSQL Provider**: Assumed Npgsql.EntityFrameworkCore.PostgreSQL package.
   - *Justification*: Standard PostgreSQL provider for EF Core in .NET ecosystem.

## Questions to Resolve Before Implementation

1. **Database Connection**: What is the PostgreSQL connection string to use during development?
   - *Suggestion*: Use appsettings.Development.json with a configurable connection string.

2. **Error Response Format**: Should we use RFC 7807 Problem Details (as recommended in security.md) or a custom format?
   - *Suggestion*: Implement RFC 7807 Problem Details for standards compliance.

3. **Logging Requirements**: Are there specific logging requirements for audit trails as mentioned in security.md?
   - *Suggestion*: Implement basic ILogger structured logging initially.

4. **CORS Policy**: Will the API need to support CORS for the admin UI?
   - *Suggestion*: Add a basic CORS policy that can be configured later.

5. **Rate Limiting**: Should rate limiting be implemented as per security.md recommendations?
   - *Suggestion*: Add basic rate limiting since security.md recommends it.

6. **Environment for Feature**: Should this feature work only in Development or all environments?
   - *Suggestion*: Implement for all environments, Swagger UI restricted to Development only.
