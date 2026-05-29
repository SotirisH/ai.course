# Feature Plan: Application Management (Ticket #001)

## Story Summary
As an administrator, I want to be able to manage applications in the system so that I can organize and track configuration data for various applications. The system should support creating, updating, retrieving, and listing applications with a unique name constraint.

## Work Item Details
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Work Item File**: docs/01_Application_feature.md

## Acceptance Criteria (Given-When-Then)

### POST `/applications` - Create Application
**Given** I am an administrator  
**When** I submit a valid application with name and comments  
**Then** the system should create a new application with a unique GUID  
**And** return the created application with HTTP 201 status  
**And** return 400 if the name is invalid (empty or >256 characters)  
**And** return 409 if the name already exists

### PUT `/applications/{id}` - Update Application
**Given** I am an administrator  
**When** I submit valid updates for an existing application  
**Then** the system should update the application  
**And** return the updated application with HTTP 200 status  
**And** return 404 if the application does not exist  
**And** return 409 if the updated name conflicts with another application

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

### Unit Tests
1. **Domain Layer**:
   - Application entity creation with valid/invalid parameters
   - Update method behavior
   - Business rule validation (name length, etc.)

2. **Application Layer**:
   - Command/Query handlers with mocked repositories
   - Mapping extensions
   - Validators (FluentValidation)

3. **Infrastructure Layer**:
   - Repository implementations with in-memory database

### Integration Tests
1. **API Endpoints**:
   - Controller actions with TestServer
   - HTTP status codes and response bodies
   - Database integration with PostgreSQL test container

### Manual Tests
1. Test all endpoints with Swagger UI
2. Verify unique constraint on name field
3. Verify GUID generation
4. Test error scenarios (404, 409, 400)

## File Change List

### Domain Layer (`Ai.Api.Domain`)

1. **Entities/Application.cs** (NEW)
   - Application entity with Id (Guid), Name (string, max 256), Comments (string, max 1024)
   - Protected constructor for EF Core
   - Private setters for encapsulation
   - Update method for business logic

2. **Interfaces/IApplicationRepository.cs** (NEW)
   - Repository interface with CRUD operations
   - Methods: GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, ExistsByNameAsync

### Application Layer (`Ai.Api.Application`)

1. **DTOs/ApplicationDto.cs** (NEW)
   - Id (Guid), Name (string), Comments (string?) properties
   - Read-only DTO for responses

2. **Commands/CreateApplication/CreateApplicationCommand.cs** (NEW)
   - MediatR command with Name and Comments
   - Implements IRequest<ApplicationDto>

3. **Commands/CreateApplication/CreateApplicationCommandHandler.cs** (NEW)
   - Handler that creates application via repository
   - Checks for duplicate name
   - Returns ApplicationDto

4. **Commands/UpdateApplication/UpdateApplicationCommand.cs** (NEW)
   - MediatR command with Id, Name, Comments
   - Implements IRequest<ApplicationDto>

5. **Commands/UpdateApplication/UpdateApplicationCommandHandler.cs** (NEW)
   - Handler that updates application via repository
   - Checks if application exists
   - Checks for duplicate name (excluding self)

6. **Queries/GetApplicationById/GetApplicationByIdQuery.cs** (NEW)
   - MediatR query with Id
   - Implements IRequest<ApplicationDto?>

7. **Queries/GetApplicationById/GetApplicationByIdQueryHandler.cs** (NEW)
   - Handler that retrieves application by ID
   - Returns null if not found

8. **Queries/GetApplications/GetApplicationsQuery.cs** (NEW)
   - MediatR query (no parameters for listing all)
   - Implements IRequest<IEnumerable<ApplicationDto>>

9. **Queries/GetApplications/GetApplicationsQueryHandler.cs** (NEW)
   - Handler that retrieves all applications

10. **Mappings/ApplicationMappings.cs** (NEW)
    - Extension methods for mapping between Entity and DTOs
    - ToDto() extension method

11. **Validators/CreateApplicationValidator.cs** (NEW)
    - FluentValidation for CreateApplicationCommand
    - Name: required, max 256 characters
    - Comments: max 1024 characters

12. **Validators/UpdateApplicationValidator.cs** (NEW)
    - FluentValidation for UpdateApplicationCommand
    - Id: required (not empty)
    - Name: required, max 256 characters
    - Comments: max 1024 characters

### Infrastructure Layer (`Ai.Api.Infrastructure`)

1. **Persistence/Context/ApplicationDbContext.cs** (NEW)
   - DbContext with Applications DbSet
   - Model configuration via IEntityTypeConfiguration
   - Use PostgreSQL provider

2. **Persistence/Configurations/ApplicationConfiguration.cs** (NEW)
   - EF Core entity configuration
   - Primary key: Id (Guid)
   - Unique constraint on Name
   - Column types and lengths

3. **Persistence/Repositories/ApplicationRepository.cs** (NEW)
   - Implementation of IApplicationRepository
   - Async methods using EF Core

4. **DependencyInjection.cs** (NEW)
   - Extension method to register infrastructure services
   - Register DbContext with PostgreSQL
   - Register Repositories

### API Layer (`Ai.Api`)

1. **Controllers/ApplicationsController.cs** (NEW)
   - [ApiController] attribute
   - Route: `api/applications`
   - POST, PUT, GET endpoints
   - Use MediatR to send commands/queries
   - Return appropriate HTTP status codes

2. **Program.cs** (MODIFY)
   - Add MediatR registration for Application layer
   - Add DbContext registration with PostgreSQL
   - Add Infrastructure services via DependencyInjection
   - Add FluentValidation registration

3. **Ai.Api.csproj** (MODIFY)
   - Add package references:
     - MediatR
     - EntityFrameworkCore
     - EntityFrameworkCore.Design
     - Npgsql.EntityFrameworkCore.PostgreSQL
     - FluentValidation.AspNetCore

4. **appsettings.json** (MODIFY)
   - Add ConnectionStrings section with PostgreSQL connection string placeholder

5. **appsettings.Development.json** (MODIFY)
   - Add ConnectionStrings section with local PostgreSQL connection string

## Implementation Details

### Domain Entity Design
```csharp
namespace Ai.Api.Domain.Entities;

public class Application
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Comments { get; private set; }

    private Application() { } // For EF Core

    public Application(string name, string? comments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty.", nameof(name));
        
        if (name.Length > 256)
            throw new ArgumentException("Application name cannot exceed 256 characters.", nameof(name));
        
        if (comments is not null && comments.Length > 1024)
            throw new ArgumentException("Comments cannot exceed 1024 characters.", nameof(comments));

        Id = Guid.NewGuid();
        Name = name;
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name cannot be empty.", nameof(name));
        
        if (name.Length > 256)
            throw new ArgumentException("Application name cannot exceed 256 characters.", nameof(name));
        
        if (comments is not null && comments.Length > 1024)
            throw new ArgumentException("Comments cannot exceed 1024 characters.", nameof(comments));

        Name = name;
        Comments = comments;
    }
}
```

### Repository Interface
```csharp
namespace Ai.Api.Domain.Interfaces;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id);
    Task<IEnumerable<Application>> GetAllAsync();
    Task AddAsync(Application application);
    Task UpdateAsync(Application application);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
}
```

### MediatR Commands and Queries

**CreateApplicationCommand**:
```csharp
using MediatR;

namespace Ai.Api.Application.Commands.CreateApplication;

public record CreateApplicationCommand(string Name, string? Comments) : IRequest<ApplicationDto>;
```

**UpdateApplicationCommand**:
```csharp
using MediatR;

namespace Ai.Api.Application.Commands.UpdateApplication;

public record UpdateApplicationCommand(Guid Id, string Name, string? Comments) : IRequest<ApplicationDto>;
```

**GetApplicationByIdQuery**:
```csharp
using MediatR;

namespace Ai.Api.Application.Queries.GetApplicationById;

public record GetApplicationByIdQuery(Guid Id) : IRequest<ApplicationDto?>;
```

**GetApplicationsQuery**:
```csharp
using MediatR;

namespace Ai.Api.Application.Queries.GetApplications;

public record GetApplicationsQuery() : IRequest<IEnumerable<ApplicationDto>>;
```

### Controller Endpoints
```csharp
[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateApplicationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateApplicationCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID in route does not match ID in body.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetApplicationByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetApplicationsQuery());
        return Ok(result);
    }
}
```

### DbContext Configuration
```csharp
using Microsoft.EntityFrameworkCore;

namespace Ai.Api.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public DbSet<Application> Applications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### EF Core Entity Configuration
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ai.Api.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(a => a.Name)
            .IsUnique();
        
        builder.Property(a => a.Comments)
            .HasMaxLength(1024);
    }
}
```

## Implementation Order

1. **Domain Layer** (no dependencies)
   - Create Application entity
   - Create IApplicationRepository interface

2. **Application Layer** (depends on Domain)
   - Create DTOs
   - Create Commands (CreateApplication, UpdateApplication)
   - Create Queries (GetApplicationById, GetApplications)
   - Create Handlers for all Commands/Queries
   - Create Mapping extensions
   - Create Validators

3. **Infrastructure Layer** (depends on Domain and Application)
   - Create ApplicationDbContext
   - Create ApplicationConfiguration
   - Create ApplicationRepository
   - Create DependencyInjection registration

4. **API Layer** (depends on all layers)
   - Update Ai.Api.csproj with packages
   - Update appsettings.json with connection string
   - Create ApplicationsController
   - Update Program.cs with all registrations

5. **Testing** (throughout)
   - Unit tests for each layer
   - Integration tests for API

## Assumptions

1. **Database**: Using PostgreSQL as the database provider
   - **Justification**: The persona.md specifies PostgreSQL expertise, and it's a robust choice for this type of configuration management system.

2. **ORM**: Using Entity Framework Core with Npgsql provider
   - **Justification**: Standard for .NET projects, supports Clean Architecture patterns, and works well with PostgreSQL.

3. **CQRS Pattern**: Using MediatR for command/query separation
   - **Justification**: Explicitly required in architecture.md ("Always use MediatR for handling commands and queries in Application layer").

4. **No authentication in this ticket**: Endpoints are public for now
   - **Justification**: The story doesn't specify authentication requirements. Security.md mentions adding `[Authorize]` but that can be added in a future ticket when authentication is implemented.

5. **No pagination for list endpoint**: Get all applications without pagination
   - **Justification**: Acceptance criteria doesn't mention pagination. The system is expected to have a manageable number of applications initially. Can be added later as an enhancement.

6. **FluentValidation for validation**: Using FluentValidation library
   - **Justification**: Mentioned in architecture.md Validators folder description and security.md for input validation. Provides clean, testable validation rules.

7. **Manual mapping**: Using extension methods, not AutoMapper
   - **Justification**: Architecture.md states "favor manual mapping" and "default to use extension classes for mapping instead of automapper".

8. **Unique name constraint**: Enforced at database level with unique index
   - **Justification**: Acceptance criteria states name should be unique. EF Core configuration will add unique index, and repository will check for duplicates before insert/update.

9. **Feature-based folder structure in Application layer**: Commands and Queries are grouped by feature
   - **Justification**: Architecture.md mentions "Group by feature" as an alternative structure for larger applications. This improves organization as the application grows.

10. **No Problem Details implementation yet**: Using basic error responses
    - **Justification**: Security.md recommends RFC 7807 Problem Details, but implementing it properly requires more setup. Can be added as a separate improvement ticket.

## Questions to Resolve Before Implementation

1. **Database connection**: What is the PostgreSQL connection string for development?
   - *Recommendation*: Use appsettings.Development.json for dev and environment variables for production (per security.md). Provide a placeholder in appsettings.json.

2. **Database creation/migration**: Should we create an initial migration?
   - *Recommendation*: Yes, create an initial migration after implementing the DbContext and configuration.

3. **Configuration IDs relationship**: The acceptance criteria mentions "associated with related configuration IDs" but the model doesn't include this. Should we add a relationship?
   - *Recommendation*: Skip for now as the model doesn't define it. Can be added in a future ticket when the configuration management feature is implemented.

4. **API versioning**: Should we add API versioning from the start?
   - *Recommendation*: Not necessary for the initial implementation. Can be added when the API evolves and multiple versions are needed.

5. **Swagger/OpenAPI documentation**: Should we add XML comments and response types?
   - *Recommendation*: Yes, add basic OpenAPI documentation with [ProducesResponseType] attributes as shown in the implementation details.

6. **Error handling middleware**: Should we implement custom error handling?
   - *Recommendation**: Use the built-in exception handling for now. Problem Details implementation can be a separate ticket.

## Completion Criteria

- [x] Test strategy and file changes identified
- [x] Feature branch created (feature/001-01_Application_feature.md)
- [ ] Plan committed to feature branch (this document)
- [ ] All questions resolved or documented as future enhancements

---

**Note to User**: This plan is ready for review. Once approved, we can proceed to the **BUILD & ASSESS** stage where we will implement the solution according to this plan.

