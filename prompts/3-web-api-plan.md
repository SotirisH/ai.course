# Config Service Implementation Plan

## Overview
This plan outlines the implementation of a REST Web API Configuration Service using .NET 10, PostgreSQL v16, and Entity Framework Core. The service will manage applications and their associated configurations with a clean architecture approach.

## 1. Project Structure

Following Clean Architecture principles from https://cleanarchitecture.jasontaylor.dev/, the project will be organized as follows:

```
config-service/
├── src/
│   ├── ConfigService.Api/
│   │   ├── Controllers/
│   │   │   ├── ApplicationsController.cs
│   │   │   ├── ApplicationsController.Tests.cs
│   │   │   ├── ConfigurationsController.cs
│   │   │   └── ConfigurationsController.Tests.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── ExceptionHandlingMiddleware.Tests.cs
│   │   ├── Program.cs
│   │   ├── Program.Tests.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── ConfigService.Api.csproj
│   │
│   ├── ConfigService.Application/
│   │   ├── DTOs/
│   │   │   ├── ApplicationDto.cs
│   │   │   ├── ApplicationDto.Tests.cs
│   │   │   ├── CreateApplicationRequest.cs
│   │   │   ├── CreateApplicationRequest.Tests.cs
│   │   │   ├── UpdateApplicationRequest.cs
│   │   │   ├── UpdateApplicationRequest.Tests.cs
│   │   │   ├── ConfigurationDto.cs
│   │   │   ├── ConfigurationDto.Tests.cs
│   │   │   ├── CreateConfigurationRequest.cs
│   │   │   ├── CreateConfigurationRequest.Tests.cs
│   │   │   ├── UpdateConfigurationRequest.cs
│   │   │   └── UpdateConfigurationRequest.Tests.cs
│   │   ├── Interfaces/
│   │   │   ├── IApplicationService.cs
│   │   │   └── IConfigurationService.cs
│   │   ├── Services/
│   │   │   ├── ApplicationService.cs
│   │   │   ├── ApplicationService.Tests.cs
│   │   │   ├── ConfigurationService.cs
│   │   │   └── ConfigurationService.Tests.cs
│   │   ├── Validators/
│   │   │   ├── CreateApplicationRequestValidator.cs
│   │   │   ├── CreateApplicationRequestValidator.Tests.cs
│   │   │   ├── UpdateApplicationRequestValidator.cs
│   │   │   ├── UpdateApplicationRequestValidator.Tests.cs
│   │   │   ├── CreateConfigurationRequestValidator.cs
│   │   │   ├── CreateConfigurationRequestValidator.Tests.cs
│   │   │   ├── UpdateConfigurationRequestValidator.cs
│   │   │   └── UpdateConfigurationRequestValidator.Tests.cs
│   │   └── ConfigService.Application.csproj
│   │
│   ├── ConfigService.Domain/
│   │   ├── Entities/
│   │   │   ├── Application.cs
│   │   │   ├── Application.Tests.cs
│   │   │   ├── Configuration.cs
│   │   │   └── Configuration.Tests.cs
│   │   ├── Exceptions/
│   │   │   ├── NotFoundException.cs
│   │   │   ├── NotFoundException.Tests.cs
│   │   │   ├── ValidationException.cs
│   │   │   └── ValidationException.Tests.cs
│   │   └── ConfigService.Domain.csproj
│   │
│   └── ConfigService.Infrastructure/
│       ├── Data/
│       │   ├── ConfigDbContext.cs
│       │   ├── ConfigDbContext.Tests.cs
│       │   ├── Configurations/
│       │   │   ├── ApplicationConfiguration.cs
│       │   │   ├── ApplicationConfiguration.Tests.cs
│       │   │   ├── ConfigurationConfiguration.cs
│       │   │   └── ConfigurationConfiguration.Tests.cs
│       │   └── Migrations/
│       ├── Repositories/
│       │   ├── IApplicationRepository.cs
│       │   ├── ApplicationRepository.cs
│       │   ├── ApplicationRepository.Tests.cs
│       │   ├── IConfigurationRepository.cs
│       │   ├── ConfigurationRepository.cs
│       │   └── ConfigurationRepository.Tests.cs
│       └── ConfigService.Infrastructure.csproj
│
├── .env
├── .env.example
├── .gitignore
└── README.md
```

## 2. Dependencies

### ConfigService.Api.csproj
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="dotenv.net" Version="3.2.1" />
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

### ConfigService.Application.csproj
```xml
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

### ConfigService.Domain.csproj
```xml
<PackageReference Include="Ulid" Version="1.3.4" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
```

### ConfigService.Infrastructure.csproj
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

## 3. Entity Models

### Application Entity (ConfigService.Domain/Entities/Application.cs)
```csharp
using System.ComponentModel.DataAnnotations;

namespace ConfigService.Domain.Entities;

public class Application
{
    [Key]
    [MaxLength(26)] // ULID is 26 characters
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Comments { get; set; }
    
    public ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();
}
```

### Configuration Entity (ConfigService.Domain/Entities/Configuration.cs)
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfigService.Domain.Entities;

public class Configuration
{
    [Key]
    [MaxLength(26)] // ULID is 26 characters
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    
    [Required]
    [MaxLength(26)]
    public string ApplicationId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1024)]
    public string? Comments { get; set; }
    
    [Column(TypeName = "jsonb")]
    public Dictionary<string, string> Config { get; set; } = new();
    
    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;
}
```

## 4. Database Context

### ConfigDbContext (ConfigService.Infrastructure/Data/ConfigDbContext.cs)
```csharp
using Microsoft.EntityFrameworkCore;
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Data;

public class ConfigDbContext : DbContext
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options) : base(options)
    {
    }
    
    public DbSet<Application> Applications { get; set; }
    public DbSet<Configuration> Configurations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigDbContext).Assembly);
    }
}
```

### Entity Configurations

**ApplicationConfiguration.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("applications");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(a => a.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(a => a.Name)
            .IsUnique();
        
        builder.Property(a => a.Comments)
            .HasMaxLength(1024);
        
        builder.HasMany(a => a.Configurations)
            .WithOne(c => c.Application)
            .HasForeignKey(c => c.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**ConfigurationConfiguration.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Data.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Configuration>
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.ToTable("configurations");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(c => c.ApplicationId)
            .HasMaxLength(26)
            .IsRequired();
        
        builder.Property(c => c.Name)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.HasIndex(c => new { c.ApplicationId, c.Name })
            .IsUnique();
        
        builder.Property(c => c.Comments)
            .HasMaxLength(1024);
        
        builder.Property(c => c.Config)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
```

## 5. API Controllers

Using MVC Controllers approach for better structure and testability.

### ApplicationsController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using ConfigService.Application.Interfaces;
using ConfigService.Application.DTOs;

namespace ConfigService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    
    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create([FromBody] CreateApplicationRequest request)
    {
        var result = await _applicationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicationDto>> Update(string id, [FromBody] UpdateApplicationRequest request)
    {
        var result = await _applicationService.UpdateAsync(id, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDto>> GetById(string id)
    {
        var result = await _applicationService.GetByIdAsync(id);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetAll()
    {
        var result = await _applicationService.GetAllAsync();
        return Ok(result);
    }
}
```

### ConfigurationsController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using ConfigService.Application.Interfaces;
using ConfigService.Application.DTOs;

namespace ConfigService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    
    public ConfigurationsController(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> Create([FromBody] CreateConfigurationRequest request)
    {
        var result = await _configurationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ConfigurationDto>> Update(string id, [FromBody] UpdateConfigurationRequest request)
    {
        var result = await _configurationService.UpdateAsync(id, request);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDto>> GetById(string id)
    {
        var result = await _configurationService.GetByIdAsync(id);
        return Ok(result);
    }
}
```

## 6. DTOs (Data Transfer Objects)

### Application DTOs

**ApplicationDto.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class ApplicationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public List<string> ConfigurationIds { get; set; } = new();
}
```

**CreateApplicationRequest.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class CreateApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
```

**UpdateApplicationRequest.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class UpdateApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
```

### Configuration DTOs

**ConfigurationDto.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class ConfigurationDto
{
    public string Id { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}
```

**CreateConfigurationRequest.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class CreateConfigurationRequest
{
    public string ApplicationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}
```

**UpdateConfigurationRequest.cs**
```csharp
namespace ConfigService.Application.DTOs;

public class UpdateConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}
```

## 7. Validation

Using FluentValidation for input validation.

### CreateApplicationRequestValidator.cs
```csharp
using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}
```

### UpdateApplicationRequestValidator.cs
```csharp
using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class UpdateApplicationRequestValidator : AbstractValidator<UpdateApplicationRequest>
{
    public UpdateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}
```

### CreateConfigurationRequestValidator.cs
```csharp
using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class CreateConfigurationRequestValidator : AbstractValidator<CreateConfigurationRequest>
{
    public CreateConfigurationRequestValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("ApplicationId is required")
            .Length(26).WithMessage("ApplicationId must be a valid ULID (26 characters)");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
        
        RuleFor(x => x.Config)
            .NotNull().WithMessage("Config is required");
    }
}
```

### UpdateConfigurationRequestValidator.cs
```csharp
using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class UpdateConfigurationRequestValidator : AbstractValidator<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
        
        RuleFor(x => x.Config)
            .NotNull().WithMessage("Config is required");
    }
}
```

## 8. Error Handling

### Custom Exceptions

**NotFoundException.cs**
```csharp
namespace ConfigService.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
    
    public NotFoundException(string name, object key) 
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
```

**ValidationException.cs**
```csharp
namespace ConfigService.Domain.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }
}
```

### Exception Handling Middleware

**ExceptionHandlingMiddleware.cs**
```csharp
using System.Net;
using System.Text.Json;
using ConfigService.Domain.Exceptions;

namespace ConfigService.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                message = notFoundEx.Message
            },
            ValidationException validationEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = validationEx.Message,
                errors = validationEx.Errors
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "An error occurred while processing your request."
            }
        };
        
        context.Response.StatusCode = response.statusCode;
        
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

## 9. Database Migrations

### Migration Strategy

1. **Initial Setup**: Create initial migration after defining all entities
2. **Migration Commands**:
   ```bash
   # Create migration
   dotnet ef migrations add InitialCreate --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api
   
   # Update database
   dotnet ef database update --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api
   ```

3. **Automatic Migration on Startup** (Development only):
   ```csharp
   // In Program.cs
   if (app.Environment.IsDevelopment())
   {
       using var scope = app.Services.CreateScope();
       var dbContext = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
       await dbContext.Database.MigrateAsync();
   }
   ```

## 10. Testing Strategy

### Unit Testing Framework
- **Framework**: xUnit
- **Mocking**: Moq
- **In-Memory Database**: EF Core InMemory provider for repository tests

### Testing Approach

1. **Entity Tests**: Validate entity creation, property constraints
2. **Validator Tests**: Test all validation rules with valid and invalid inputs
3. **Service Tests**: Mock repositories, test business logic
4. **Controller Tests**: Mock services, test HTTP responses
5. **Repository Tests**: Use InMemory database, test CRUD operations
6. **Middleware Tests**: Test exception handling scenarios

### Test File Naming Convention
- Co-locate test files with source files
- Use `.Tests.cs` suffix (e.g., `ApplicationService.cs` → `ApplicationService.Tests.cs`)

### Example Test Structure

```csharp
using Xunit;
using Moq;
using ConfigService.Application.Services;
using ConfigService.Infrastructure.Repositories;

namespace ConfigService.Application.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IApplicationRepository> _mockRepository;
    private readonly ApplicationService _service;
    
    public ApplicationServiceTests()
    {
        _mockRepository = new Mock<IApplicationRepository>();
        _service = new ApplicationService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsApplicationDto()
    {
        // Arrange
        var request = new CreateApplicationRequest { Name = "Test App" };
        
        // Act
        var result = await _service.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test App", result.Name);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Application?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetByIdAsync("non-existent-id"));
    }
}
```

## 11. Configuration Loading

### .env File Structure

**.env**
```env
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=configservice;Username=postgres;Password=yourpassword
ASPNETCORE_ENVIRONMENT=Development
LOGGING_LEVEL=Information
```

**.env.example**
```env
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=configservice;Username=postgres;Password=yourpassword
ASPNETCORE_ENVIRONMENT=Development
LOGGING_LEVEL=Information
```

### Loading .env in Program.cs

```csharp
using dotenv.net;

// Load .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Override configuration with environment variables
builder.Configuration.AddEnvironmentVariables();

// Configure database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseNpgsql(connectionString));
```

## 12. Setup Instructions

### Prerequisites
- .NET 10 SDK (version 10.0.202)
- PostgreSQL v16
- Docker (optional, for running PostgreSQL in a container)

### Development Environment Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd config-service
   ```

2. **Set up PostgreSQL**
   
   Option A: Using Docker
   ```bash
   docker run --name postgres-config -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres:16
   ```
   
   Option B: Install PostgreSQL locally from https://www.postgresql.org/download/

3. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your database credentials
   ```

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

5. **Run database migrations**
   ```bash
   cd src/ConfigService.Api
   dotnet ef database update --project ../ConfigService.Infrastructure
   ```

6. **Run the application**
   ```bash
   dotnet run --project src/ConfigService.Api
   ```
   
   The API will be available at: `https://localhost:5001` or `http://localhost:5000`
   
   Swagger UI: `https://localhost:5001/swagger`

7. **Run tests**
   ```bash
   # Run all tests
   dotnet test
   
   # Run tests with coverage
   dotnet test --collect:"XPlat Code Coverage"
   ```

### Project Build Commands

```bash
# Build the solution
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Clean build artifacts
dotnet clean

# Run the API
dotnet run --project src/ConfigService.Api

# Watch mode (auto-reload on changes)
dotnet watch --project src/ConfigService.Api
```

### Database Commands

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api

# Update database to latest migration
dotnet ef database update --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api

# Rollback to specific migration
dotnet ef database update <MigrationName> --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api

# Remove last migration
dotnet ef migrations remove --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api

# Generate SQL script
dotnet ef migrations script --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api
```

## 13. Program.cs Configuration

```csharp
using ConfigService.Infrastructure.Data;
using ConfigService.Infrastructure.Repositories;
using ConfigService.Application.Interfaces;
using ConfigService.Application.Services;
using ConfigService.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using dotenv.net;

// Load .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Override configuration with environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Configure FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateApplicationRequestValidator>();

// Configure database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

// Register services
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Auto-migrate database in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 14. Repository Pattern

### IApplicationRepository.cs
```csharp
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Repositories;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(string id);
    Task<Application?> GetByNameAsync(string name);
    Task<IEnumerable<Application>> GetAllAsync();
    Task<Application> CreateAsync(Application application);
    Task<Application> UpdateAsync(Application application);
    Task DeleteAsync(string id);
}
```

### ApplicationRepository.cs
```csharp
using ConfigService.Domain.Entities;
using ConfigService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ConfigDbContext _context;
    
    public ApplicationRepository(ConfigDbContext context)
    {
        _context = context;
    }
    
    public async Task<Application?> GetByIdAsync(string id)
    {
        return await _context.Applications
            .Include(a => a.Configurations)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Application?> GetByNameAsync(string name)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Name == name);
    }
    
    public async Task<IEnumerable<Application>> GetAllAsync()
    {
        return await _context.Applications
            .Include(a => a.Configurations)
            .ToListAsync();
    }
    
    public async Task<Application> CreateAsync(Application application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }
    
    public async Task<Application> UpdateAsync(Application application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }
    
    public async Task DeleteAsync(string id)
    {
        var application = await GetByIdAsync(id);
        if (application != null)
        {
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }
    }
}
```

### IConfigurationRepository.cs
```csharp
using ConfigService.Domain.Entities;

namespace ConfigService.Infrastructure.Repositories;

public interface IConfigurationRepository
{
    Task<Configuration?> GetByIdAsync(string id);
    Task<Configuration?> GetByApplicationAndNameAsync(string applicationId, string name);
    Task<IEnumerable<Configuration>> GetByApplicationIdAsync(string applicationId);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task<Configuration> UpdateAsync(Configuration configuration);
    Task DeleteAsync(string id);
}
```

### ConfigurationRepository.cs
```csharp
using ConfigService.Domain.Entities;
using ConfigService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigDbContext _context;
    
    public ConfigurationRepository(ConfigDbContext context)
    {
        _context = context;
    }
    
    public async Task<Configuration?> GetByIdAsync(string id)
    {
        return await _context.Configurations
            .Include(c => c.Application)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Configuration?> GetByApplicationAndNameAsync(string applicationId, string name)
    {
        return await _context.Configurations
            .FirstOrDefaultAsync(c => c.ApplicationId == applicationId && c.Name == name);
    }
    
    public async Task<IEnumerable<Configuration>> GetByApplicationIdAsync(string applicationId)
    {
        return await _context.Configurations
            .Where(c => c.ApplicationId == applicationId)
            .ToListAsync();
    }
    
    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
    
    public async Task<Configuration> UpdateAsync(Configuration configuration)
    {
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
    
    public async Task DeleteAsync(string id)
    {
        var configuration = await GetByIdAsync(id);
        if (configuration != null)
        {
            _context.Configurations.Remove(configuration);
            await _context.SaveChangesAsync();
        }
    }
}
```

## 15. Service Layer

### IApplicationService.cs
```csharp
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Interfaces;

public interface IApplicationService
{
    Task<ApplicationDto> CreateAsync(CreateApplicationRequest request);
    Task<ApplicationDto> UpdateAsync(string id, UpdateApplicationRequest request);
    Task<ApplicationDto> GetByIdAsync(string id);
    Task<IEnumerable<ApplicationDto>> GetAllAsync();
}
```

### ApplicationService.cs
```csharp
using ConfigService.Application.DTOs;
using ConfigService.Application.Interfaces;
using ConfigService.Domain.Entities;
using ConfigService.Domain.Exceptions;
using ConfigService.Infrastructure.Repositories;

namespace ConfigService.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repository;
    
    public ApplicationService(IApplicationRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ApplicationDto> CreateAsync(CreateApplicationRequest request)
    {
        // Check if application with same name exists
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "An application with this name already exists." } }
            });
        }
        
        var application = new Application
        {
            Name = request.Name,
            Comments = request.Comments
        };
        
        var created = await _repository.CreateAsync(application);
        
        return MapToDto(created);
    }
    
    public async Task<ApplicationDto> UpdateAsync(string id, UpdateApplicationRequest request)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), id);
        }
        
        // Check if another application with same name exists
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing != null && existing.Id != id)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "An application with this name already exists." } }
            });
        }
        
        application.Name = request.Name;
        application.Comments = request.Comments;
        
        var updated = await _repository.UpdateAsync(application);
        
        return MapToDto(updated);
    }
    
    public async Task<ApplicationDto> GetByIdAsync(string id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), id);
        }
        
        return MapToDto(application);
    }
    
    public async Task<IEnumerable<ApplicationDto>> GetAllAsync()
    {
        var applications = await _repository.GetAllAsync();
        return applications.Select(MapToDto);
    }
    
    private static ApplicationDto MapToDto(Application application)
    {
        return new ApplicationDto
        {
            Id = application.Id,
            Name = application.Name,
            Comments = application.Comments,
            ConfigurationIds = application.Configurations.Select(c => c.Id).ToList()
        };
    }
}
```

### IConfigurationService.cs
```csharp
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Interfaces;

public interface IConfigurationService
{
    Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request);
    Task<ConfigurationDto> UpdateAsync(string id, UpdateConfigurationRequest request);
    Task<ConfigurationDto> GetByIdAsync(string id);
}
```

### ConfigurationService.cs
```csharp
using ConfigService.Application.DTOs;
using ConfigService.Application.Interfaces;
using ConfigService.Domain.Entities;
using ConfigService.Domain.Exceptions;
using ConfigService.Infrastructure.Repositories;

namespace ConfigService.Application.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configRepository;
    private readonly IApplicationRepository _appRepository;
    
    public ConfigurationService(
        IConfigurationRepository configRepository,
        IApplicationRepository appRepository)
    {
        _configRepository = configRepository;
        _appRepository = appRepository;
    }
    
    public async Task<ConfigurationDto> CreateAsync(CreateConfigurationRequest request)
    {
        // Verify application exists
        var application = await _appRepository.GetByIdAsync(request.ApplicationId);
        if (application == null)
        {
            throw new NotFoundException(nameof(Application), request.ApplicationId);
        }
        
        // Check if configuration with same name exists for this application
        var existing = await _configRepository.GetByApplicationAndNameAsync(
            request.ApplicationId, request.Name);
        if (existing != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "A configuration with this name already exists for this application." } }
            });
        }
        
        var configuration = new Configuration
        {
            ApplicationId = request.ApplicationId,
            Name = request.Name,
            Comments = request.Comments,
            Config = request.Config
        };
        
        var created = await _configRepository.CreateAsync(configuration);
        
        return MapToDto(created);
    }
    
    public async Task<ConfigurationDto> UpdateAsync(string id, UpdateConfigurationRequest request)
    {
        var configuration = await _configRepository.GetByIdAsync(id);
        if (configuration == null)
        {
            throw new NotFoundException(nameof(Configuration), id);
        }
        
        // Check if another configuration with same name exists for this application
        var existing = await _configRepository.GetByApplicationAndNameAsync(
            configuration.ApplicationId, request.Name);
        if (existing != null && existing.Id != id)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Name", new[] { "A configuration with this name already exists for this application." } }
            });
        }
        
        configuration.Name = request.Name;
        configuration.Comments = request.Comments;
        configuration.Config = request.Config;
        
        var updated = await _configRepository.UpdateAsync(configuration);
        
        return MapToDto(updated);
    }
    
    public async Task<ConfigurationDto> GetByIdAsync(string id)
    {
        var configuration = await _configRepository.GetByIdAsync(id);
        if (configuration == null)
        {
            throw new NotFoundException(nameof(Configuration), id);
        }
        
        return MapToDto(configuration);
    }
    
    private static ConfigurationDto MapToDto(Configuration configuration)
    {
        return new ConfigurationDto
        {
            Id = configuration.Id,
            ApplicationId = configuration.ApplicationId,
            Name = configuration.Name,
            Comments = configuration.Comments,
            Config = configuration.Config
        };
    }
}
```

## 16. .gitignore

```gitignore
# Build results
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio
.vs/
.vscode/
*.suo
*.user
*.userosscache
*.sln.docstates

# Environment files
.env
.env.local
.env.*.local

# Database
*.db
*.db-shm
*.db-wal

# Test results
TestResults/
*.trx
*.coverage
*.coveragexml

# NuGet
*.nupkg
*.snupkg
.nuget/
packages/

# JetBrains Rider
.idea/
*.sln.iml
```

## 17. README.md Structure

The README.md should include:

1. **Project Overview**: Brief description of the Config Service
2. **Features**: List of key features
3. **Tech Stack**: Technologies used with versions
4. **Prerequisites**: Required software and versions
5. **Getting Started**: Step-by-step setup instructions
6. **API Documentation**: Endpoint descriptions and examples
7. **Database Schema**: Entity relationship diagram or description
8. **Testing**: How to run tests
9. **Development**: Development workflow and commands
10. **Deployment**: Deployment instructions (future)
11. **Contributing**: Contribution guidelines (if applicable)
12. **License**: License information

## Summary

This implementation plan provides a comprehensive blueprint for building a REST Web API Configuration Service using .NET 10, PostgreSQL v16, and Entity Framework Core. The architecture follows Clean Architecture principles with clear separation of concerns across Domain, Application, Infrastructure, and API layers.

Key highlights:
- **ULID** for primary keys using the `Ulid` NuGet package
- **JSONB** support for configuration dictionaries in PostgreSQL
- **FluentValidation** for input validation
- **Repository pattern** for data access abstraction
- **Service layer** for business logic
- **Exception handling middleware** for consistent error responses
- **xUnit** for comprehensive unit testing with co-located test files
- **.env** file for configuration management
- **EF Core migrations** for database schema management

All components are designed to be testable, maintainable, and extensible for future enhancements like feature flags (Module 3).

