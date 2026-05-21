# Plan: Feature 001 - Application Management

## Story Summary
As an administrator, I want to be able to manage applications in the system so that I can create, update, retrieve, and list applications with unique identifiers and associated configuration IDs.

## Acceptance Criteria (Given-When-Then)

### Scenario 1: Create Application
**Given** an administrator provides valid application data (name, comments)
**When** they send a POST request to `/applications`
**Then** a new application is created with a unique GUID, and the system returns 201 Created with the application data

### Scenario 2: Update Application
**Given** an application exists with the specified ID
**And** an administrator provides valid update data (name, comments)
**When** they send a PUT request to `/applications/{id}`
**Then** the application is updated, and the system returns 200 OK with the updated application data

### Scenario 3: Get Application by ID
**Given** an application exists with the specified ID
**When** an administrator sends a GET request to `/applications/{id}`
**Then** the system returns 200 OK with the application data

### Scenario 4: List Applications
**Given** applications exist in the system
**When** an administrator sends a GET request to `/applications`
**Then** the system returns 200 OK with a list of all applications

### Scenario 5: Create Duplicate Application
**Given** an application with the same name already exists
**When** an administrator sends a POST request to `/applications` with a duplicate name
**Then** the system returns 400 Bad Request with a validation error

### Scenario 6: Update Non-existent Application
**Given** no application exists with the specified ID
**When** an administrator sends a PUT request to `/applications/{id}`
**Then** the system returns 404 Not Found

## Test Strategy

### Unit Tests
- **Domain Layer**: Test Application entity creation, validation rules, and business logic
- **Application Layer**: Test command/query handlers, validators, and mapping profiles
- **Infrastructure Layer**: Test repository implementations with in-memory database

### Integration Tests
- **API Controllers**: Test all endpoints with HTTP requests and verify responses
- **Database**: Test EF Core configurations and migrations with test database

### Test Coverage Areas
1. Create application with valid/invalid data
2. Update application with valid/invalid data
3. Retrieve existing and non-existing applications
4. List all applications
5. Handle duplicate name validation
6. Verify unique constraint on name field

## File Change List

### Domain Layer (Ai.Api.Domain)
- `Entities/Application.cs` - New file: Application entity with id, name, comments
- `Interfaces/IApplicationRepository.cs` - New file: Repository interface for CRUD operations

### Application Layer (Ai.Api.Application)
- `DTOs/ApplicationDto.cs` - New file: DTO for application data transfer
- `Commands/CreateApplicationCommand.cs` - New file: CQRS command for creation
- `Commands/UpdateApplicationCommand.cs` - New file: CQRS command for update
- `Queries/GetApplicationByIdQuery.cs` - New file: CQRS query for single retrieval
- `Queries/ListApplicationsQuery.cs` - New file: CQRS query for listing
- `Handlers/CreateApplicationHandler.cs` - New file: Command handler
- `Handlers/UpdateApplicationHandler.cs` - New file: Command handler
- `Handlers/GetApplicationByIdHandler.cs` - New file: Query handler
- `Handlers/ListApplicationsHandler.cs` - New file: Query handler
- `Validators/CreateApplicationValidator.cs` - New file: FluentValidation validator
- `Validators/UpdateApplicationValidator.cs` - New file: FluentValidation validator
- `Profiles/ApplicationProfile.cs` - New file: AutoMapper profile

### Infrastructure Layer (Ai.Api.Infrastructure)
- `Persistence/Context/ApplicationDbContext.cs` - New file: EF Core DbContext
- `Persistence/Repositories/ApplicationRepository.cs` - New file: Repository implementation
- `Persistence/Configurations/ApplicationConfiguration.cs` - New file: EF entity configuration

### API Layer (Ai.Api)
- `Controllers/ApplicationsController.cs` - New file: API controller with CRUD endpoints
- `Requests/CreateApplicationRequest.cs` - New file: Request model for POST
- `Requests/UpdateApplicationRequest.cs` - New file: Request model for PUT
- `Responses/ApplicationResponse.cs` - New file: Response model

### Configuration Updates
- `Ai.Api.Application.csproj` - Add MediatR, FluentValidation, AutoMapper packages
- `Ai.Api.Infrastructure.csproj` - Add EF Core, Npgsql packages
- `Ai.Api.csproj` - Add Swagger/OpenAPI enhancements if needed
- `Program.cs` - Register MediatR, AutoMapper, DbContext, repositories

## Implementation Details

### Application Entity (Domain)
```csharp
public class Application
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } // max 256 chars, unique
    public string Comments { get; private set; } // max 1024 chars
    
    private Application() { } // EF Core requires private parameterless constructor
    
    public Application(string name, string comments)
    {
        Id = Guid.NewGuid();
        Name = name;
        Comments = comments;
    }
    
    public void Update(string name, string comments)
    {
        Name = name;
        Comments = comments;
    }
}
```

### Repository Interface (Domain)
```csharp
public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<IEnumerable<Application>> GetAllAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(Application application);
    Task UpdateAsync(Application application);
}
```

### EF Core Configuration (Infrastructure)
- Configure `Application` entity
- Set primary key to `Id` (Guid)
- Set `Name` as required with max length 256 and unique index
- Set `Comments` with max length 1024

### API Endpoints
- `POST /applications` - Create new application
- `PUT /applications/{id}` - Update existing application
- `GET /applications/{id}` - Get application by ID
- `GET /applications` - List all applications

## Implementation Order

1. **Domain Layer**
   - Create `Application` entity
   - Create `IApplicationRepository` interface

2. **Infrastructure Layer**
   - Create `ApplicationDbContext` (or extend existing if available)
   - Create `ApplicationConfiguration` for EF Core
   - Create `ApplicationRepository` implementation

3. **Application Layer**
   - Create `ApplicationDto`
   - Create CQRS commands/queries (Create, Update, GetById, List)
   - Create command/query handlers
   - Create FluentValidation validators
   - Create AutoMapper profile

4. **API Layer**
   - Create request/response models
   - Create `ApplicationsController` with all endpoints

5. **Configuration**
   - Update all `.csproj` files with required NuGet packages
   - Update `Program.cs` to register all services

6. **Testing**
   - Write unit tests for each layer
   - Write integration tests for API endpoints

## Assumptions Made During Planning

1. **CQRS Pattern**: Using MediatR for CQRS implementation as suggested in `architecture.md`
2. **Database**: Using PostgreSQL with EF Core (based on persona.md mentioning PostgreSQL expertise)
3. **Validation**: Using FluentValidation for input validation (as mentioned in architecture.md)
4. **Mapping**: Using AutoMapper for entity-DTO mapping (as mentioned in architecture.md)
5. **No Authentication**: The current API doesn't have authentication implemented yet (no `[Authorize]` attributes)
6. **No Pagination**: List endpoint returns all applications without pagination (requirement doesn't specify)
7. **Configuration IDs**: The story mentions "associated with related configuration IDs" but the model doesn't include this field - assuming this is for a future iteration
8. **Error Handling**: Will use Problem Details (RFC 7807) as per security.md recommendations

## Questions to be Answered Before Implementation

1. **Database Choice**: Should we use PostgreSQL (as mentioned in persona) or SQL Server/SQLite for simplicity?
   - *PostgreSQL recommended based on persona.md*

2. **MediatR**: Should we use MediatR for CQRS, or use direct service/command handlers without the library?
   - *MediatR recommended based on architecture.md references*

3. **DbContext Location**: Should the `ApplicationDbContext` be in Infrastructure layer (recommended by architecture.md) or in the API layer?
   - *Infrastructure layer per architecture.md*

4. **Pagination**: Should the GET `/applications` endpoint support pagination?
   - *Not required by acceptance criteria, but could be added as a nice-to-have*

5. **Configuration IDs**: The story mentions "associated with related configuration IDs" but the model doesn't have this field. Should we add a `ConfigurationIds` field or is this for a future story?
   - *Assuming future iteration based on current model*

6. **Unique Name Constraint**: Should the unique constraint on `Name` be case-sensitive or case-insensitive?
   - *Case-sensitive per PostgreSQL default, but can be configured*

7. **Comments Field**: Is `comments` optional or required?
   - *Assuming optional (nullable) since not explicitly marked as required*

