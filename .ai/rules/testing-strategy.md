# Testing Structure Documentation for C# API Projects

## Overview

This document describes a comprehensive testing structure and strategy for C# API projects. Tests are organized in a dedicated `tests/` folder at the solution level, with clear separation by test type following Clean Architecture principles.

## Test Folder Structure

```
tests/
├── UnitTests/              # Unit tests for handlers, validators, mappers, domain logic
├── IntegrationTests/       # Integration tests for repositories, DbContext (Testcontainers)
└── E2ETests/              # API integration tests using WebApplicationFactory
```

## Test Layer Definitions

### Unit Tests (`tests/UnitTests/`)

**Purpose**: Test individual components in isolation without external dependencies.

**Target Components**:
- Command and query handlers (MediatR/CQRS)
- Validators (FluentValidation)
- Mappers (AutoMapper, Mapster, or extension methods)
- Domain entities and value objects
- Business rules and domain logic
- Service classes
- Utility functions

**Tools**:
- **xUnit** (test framework)
- **Shouldly** or **FluentAssertions** (assertions)
- **Moq** or **NSubstitute** (mocking dependencies)

**Characteristics**:
- Fast execution (milliseconds)
- No database or external services
- Mock all dependencies (repositories, external services, etc.)
- Focus on single responsibility
- Deterministic results
- No I/O operations

**Example Structure**:
```
tests/UnitTests/
├── Features/
│   ├── UserManagement/
│   │   ├── CreateUserCommandHandlerTests.cs
│   │   ├── GetUserQueryHandlerTests.cs
│   │   └── UpdateUserCommandHandlerTests.cs
│   └── OrderProcessing/
│       ├── CreateOrderCommandHandlerTests.cs
│       └── OrderValidatorTests.cs
├── Mappers/
│   └── UserMappingExtensionsTests.cs
├── Validators/
│   ├── CreateUserCommandValidatorTests.cs
│   └── UpdateUserCommandValidatorTests.cs
└── Domain/
    ├── Entities/
    │   └── UserTests.cs
    └── ValueObjects/
        └── EmailTests.cs
```

### Integration Tests (`tests/IntegrationTests/`)

**Purpose**: Test infrastructure layer components with real dependencies.

**Target Components**:
- Repository implementations
- DbContext operations (Entity Framework Core)
- Database migrations
- Entity configurations (IEntityTypeConfiguration)
- Data access patterns
- Database constraints and indexes
- Transaction handling
- External API integrations (with real HTTP calls or test servers)
- Message broker integrations (RabbitMQ, Azure Service Bus, etc.)
- Cache implementations (Redis, Memory Cache)

**Tools**:
- **xUnit** (test framework)
- **Shouldly** or **FluentAssertions** (assertions)
- **Testcontainers** (real database instances via Docker)
- **WireMock.Net** (mock external HTTP APIs)

**Characteristics**:
- Slower than unit tests (seconds)
- Uses real database (PostgreSQL, SQL Server, etc. via Docker)
- Tests actual database behavior
- Verifies constraints, indexes, and relationships
- Tests transaction handling and concurrency
- Requires setup/teardown
- May require Docker to be running

**Example Structure**:
```
tests/IntegrationTests/
├── Repositories/
│   ├── UserRepositoryIntegrationTests.cs
│   └── OrderRepositoryIntegrationTests.cs
├── Persistence/
│   ├── DatabaseMigrationsTests.cs
│   └── EntityConfigurationTests.cs
├── ExternalServices/
│   └── PaymentGatewayServiceIntegrationTests.cs
└── Fixtures/
    └── DatabaseFixture.cs
```

### E2E Tests (`tests/E2ETests/`)

**Purpose**: Test the full API request/response cycle through the HTTP layer.

**Target Components**:
- API endpoints (controllers or minimal APIs)
- Full request pipeline
- Middleware (exception handling, authentication, logging, etc.)
- Authentication/Authorization
- Model binding and validation
- Content negotiation
- Complete feature workflows
- HTTP status codes and response formats

**Tools**:
- **xUnit** (test framework)
- **Shouldly** or **FluentAssertions** (assertions)
- **Microsoft.AspNetCore.Mvc.Testing** (WebApplicationFactory)
- **HttpClient** (for making requests)

**Characteristics**:
- Tests complete request/response cycle
- In-memory test server (no real HTTP calls)
- Happy path and error scenarios
- Verifies HTTP status codes (200, 201, 400, 404, 500, etc.)
- Verifies response structure and content
- Fast execution (no network overhead)
- Can use in-memory database or test database
- Tests authentication/authorization flows

**Example Structure**:
```
tests/E2ETests/
├── Features/
│   ├── UserManagementE2ETests.cs
│   └── OrderProcessingE2ETests.cs
├── Middleware/
│   └── ExceptionHandlingMiddlewareE2ETests.cs
├── Authentication/
│   └── JwtAuthenticationE2ETests.cs
└── Fixtures/
    └── CustomWebApplicationFactory.cs
```

## Testing Tools Stack

### Core Testing Frameworks
- **xUnit 2.9.3+**: Primary testing framework for all test types
  - Modern, extensible, and widely adopted in .NET
  - Supports parallel test execution
  - Rich ecosystem of extensions

### Assertion Libraries
- **Shouldly**: Fluent assertion library with readable error messages
  - Example: `result.ShouldNotBeNull();`
  - Example: `user.Name.ShouldBe("John Doe");`
- **FluentAssertions**: Alternative with extensive assertion methods
  - Example: `result.Should().NotBeNull();`
  - Example: `user.Name.Should().Be("John Doe");`

### Mocking Frameworks (Unit Tests)
- **Moq**: Most popular mocking framework for .NET
  ```csharp
  var mockRepository = new Mock<IUserRepository>();
  mockRepository.Setup(x => x.GetByIdAsync(userId))
      .ReturnsAsync(user);
  ```
- **NSubstitute**: Alternative with cleaner syntax
  ```csharp
  var repository = Substitute.For<IUserRepository>();
  repository.GetByIdAsync(userId).Returns(user);
  ```

### E2E Testing
- **Microsoft.AspNetCore.Mvc.Testing**: For API integration tests
  - Provides `WebApplicationFactory<TEntryPoint>`
  - In-memory test server
  - Configuration overrides
  - Fast execution without network overhead

### Integration Testing
- **Testcontainers**: For integration tests requiring real databases
  - Disposable, lightweight database containers
  - Supports PostgreSQL, SQL Server, MySQL, MongoDB, etc.
  - Automatically manages container lifecycle
  - Ensures tests run against real database behavior
  ```csharp
  var container = new PostgreSqlBuilder()
      .WithDatabase("testdb")
      .WithUsername("test")
      .WithPassword("test")
      .Build();
  ```

### HTTP Mocking (Integration Tests)
- **WireMock.Net**: For mocking external HTTP APIs
  - Simulates HTTP responses without actual network calls
  - Useful for testing external service integrations
  ```csharp
  var server = WireMockServer.Start();
  server.Given(Request.Create().WithPath("/api/users"))
      .RespondWith(Response.Create().WithStatusCode(200));
  ```

## Test Naming Conventions

### Test Class Names
- `{ComponentName}Tests` for unit tests
  - Example: `CreateUserCommandHandlerTests`
  - Example: `UserValidatorTests`
- `{ComponentName}IntegrationTests` for integration tests
  - Example: `UserRepositoryIntegrationTests`
  - Example: `DatabaseMigrationsIntegrationTests`
- `{FeatureName}E2ETests` for E2E tests
  - Example: `UserManagementE2ETests`
  - Example: `OrderProcessingE2ETests`

### Test Method Names
Use descriptive names that explain the scenario. Choose one convention and stick with it:

**Option 1: Should_When Pattern**
- `Should_{ExpectedBehavior}_When_{Condition}`
- Example: `Should_ReturnUser_When_IdExists`
- Example: `Should_ThrowValidationException_When_EmailIsInvalid`
- Example: `Should_Return404_When_UserNotFound`

**Option 2: Given_When_Then Pattern**
- `Given_{Precondition}_When_{Action}_Then_{ExpectedResult}`
- Example: `Given_ValidUserId_When_GetUser_Then_ReturnsUser`
- Example: `Given_InvalidEmail_When_CreateUser_Then_ThrowsValidationException`

**Recommendation**: Use the `Should_When` pattern for simplicity and readability.

## Running Tests

### Run All Tests
```powershell
dotnet test
```

### Run Specific Test Project
```powershell
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests
dotnet test tests/E2ETests
```

### Run Tests by Category/Trait
```powershell
# Run only fast tests
dotnet test --filter Category=Unit

# Run only integration tests
dotnet test --filter Category=Integration

# Run specific test class
dotnet test --filter FullyQualifiedName~UserRepositoryIntegrationTests
```

### Run Tests with Coverage
```powershell
# Using built-in coverage
dotnet test --collect:"XPlat Code Coverage"

# Using coverlet
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report with ReportGenerator
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### Run Tests in Watch Mode
```powershell
dotnet watch test --project tests/UnitTests
```

### Run Tests with Detailed Output
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Test Organization Best Practices

### 1. Mirror Source Structure
Organize test files to mirror the source code structure:
- Source: `src/YourApi.Application/Features/Users/CreateUserCommandHandler.cs`
- Test: `tests/UnitTests/Features/Users/CreateUserCommandHandlerTests.cs`

### 2. One Test Class Per Component
Each component should have its own test class with focused tests.

### 3. Arrange-Act-Assert Pattern
Structure tests clearly with three distinct sections:
```csharp
[Fact]
public async Task Should_CreateUser_When_ValidRequest()
{
    // Arrange
    var command = new CreateUserCommand 
    { 
        Name = "John Doe", 
        Email = "john@example.com" 
    };
    var handler = new CreateUserCommandHandler(_mockRepository.Object);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.ShouldNotBeNull();
    result.Name.ShouldBe("John Doe");
    result.Email.ShouldBe("john@example.com");
}
```

### 4. Test Data Builders
Use builder patterns for complex test data setup:
```csharp
public class UserBuilder
{
    private string _name = "Default Name";
    private string _email = "default@example.com";
    
    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public User Build() => new User { Name = _name, Email = _email };
}

// Usage
var user = new UserBuilder()
    .WithName("John Doe")
    .WithEmail("john@example.com")
    .Build();
```

### 5. Shared Fixtures
Use xUnit fixtures for expensive setup (e.g., Testcontainers):
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; private set; }
    public string ConnectionString => Container.GetConnectionString();
    
    public async Task InitializeAsync()
    {
        Container = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .Build();
        await Container.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}

public class UserRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public UserRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 6. Test Isolation
Each test should be independent and not rely on other tests. Use separate database transactions or clean up data after each test.

### 7. Descriptive Test Names
Test names should clearly describe what is being tested without needing to read the code.

### 8. Single Assertion Focus
Each test should verify one specific behavior (when practical). Multiple assertions are acceptable if they verify the same behavior.

### 9. Avoid Test Logic
Tests should be simple and straightforward. Avoid complex logic, loops, or conditionals in tests.

### 10. Use Test Categories/Traits
Group tests by feature, speed, or type for selective execution:
```csharp
[Trait("Category", "Unit")]
[Trait("Feature", "UserManagement")]
public class CreateUserCommandHandlerTests { }
```

## Coverage Goals

- **Unit Tests**: Aim for 70-90% code coverage
  - Focus on business logic, handlers, validators
  - Cover edge cases and error scenarios
- **Integration Tests**: Cover all repository methods and database operations
  - Test CRUD operations
  - Test complex queries
  - Test transaction handling
- **E2E Tests**: Cover all critical API endpoints and workflows
  - Happy path scenarios
  - Common error scenarios (400, 404, 500)
  - Authentication/authorization flows

**Important**: Coverage is a metric, not a goal. Focus on meaningful tests that provide confidence, not arbitrary percentages.

## Test Pyramid Strategy

Follow the test pyramid principle for optimal test distribution:

```
        /\
       /E2E\      <- Few (10-20%)
      /------\    Slow, brittle, high maintenance
     /  Integ \   <- Some (20-30%)
    /----------\  Moderate speed, some setup
   /    Unit    \ <- Many (50-70%)
  /--------------\ Fast, isolated, easy to maintain
```

**Distribution Guidelines**:
- **50-70% Unit Tests**: Fast, isolated, cover business logic
- **20-30% Integration Tests**: Verify infrastructure and data access
- **10-20% E2E Tests**: Validate critical API workflows

## CI/CD Integration

### Test Execution Order
Tests should be run in the following order in CI/CD pipelines:
1. **Unit Tests** (fastest, fail fast) - Run on every commit
2. **Integration Tests** (slower, require Docker) - Run on pull requests
3. **E2E Tests** (moderate speed) - Run before deployment

### Pipeline Optimization
```yaml
# Example GitHub Actions workflow
- name: Run Unit Tests
  run: dotnet test tests/UnitTests --no-build --verbosity normal

- name: Run Integration Tests
  run: dotnet test tests/IntegrationTests --no-build --verbosity normal
  # Requires Docker to be available

- name: Run E2E Tests
  run: dotnet test tests/E2ETests --no-build --verbosity normal

- name: Generate Coverage Report
  run: |
    dotnet test --collect:"XPlat Code Coverage"
    reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
```

### Prerequisites
- **Unit Tests**: No prerequisites
- **Integration Tests**: Docker must be running (for Testcontainers)
- **E2E Tests**: No prerequisites (uses in-memory server)

## Test Data Management

### Unit Tests
- Use hardcoded test data or builders
- Keep data minimal and focused on the test scenario
- Use constants for reusable values
```csharp
private const string ValidEmail = "test@example.com";
private const string ValidName = "John Doe";
```

### Integration Tests
- Use database seeding in fixture setup
- Clean up data after each test (transactions or explicit cleanup)
- Use unique identifiers to avoid conflicts
```csharp
public async Task InitializeAsync()
{
    await _dbContext.Database.MigrateAsync();
    await SeedTestDataAsync();
}

private async Task SeedTestDataAsync()
{
    _dbContext.Users.Add(new User { Id = Guid.NewGuid(), Name = "Test User" });
    await _dbContext.SaveChangesAsync();
}
```

### E2E Tests
- Use API calls to set up test data (when needed)
- Clean up data in test teardown
- Use unique identifiers (GUIDs) to avoid conflicts
- Consider using in-memory database for faster execution
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real database with in-memory database
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

## Common Anti-Patterns to Avoid

### 1. Testing Implementation Details
❌ **Bad**: Testing private methods or internal state
```csharp
// Don't test private methods directly
[Fact]
public void Should_ValidateEmail() 
{
    var result = InvokePrivateMethod("ValidateEmail", "test@example.com");
    result.ShouldBeTrue();
}
```

✅ **Good**: Test public behavior
```csharp
[Fact]
public async Task Should_CreateUser_When_EmailIsValid()
{
    var command = new CreateUserCommand { Email = "test@example.com" };
    var result = await _handler.Handle(command, CancellationToken.None);
    result.ShouldNotBeNull();
}
```

### 2. Fragile Tests
❌ **Bad**: Tests that break with minor refactoring
```csharp
// Brittle assertion on exact error message
exception.Message.ShouldBe("User with email test@example.com already exists");
```

✅ **Good**: Test behavior, not exact messages
```csharp
exception.ShouldBeOfType<DomainException>();
exception.Message.ShouldContain("already exists");
```

### 3. Slow Tests
❌ **Bad**: Using real database in unit tests
```csharp
// Unit test should not use real database
var dbContext = new AppDbContext(realConnectionString);
```

✅ **Good**: Mock dependencies in unit tests
```csharp
var mockRepository = new Mock<IUserRepository>();
```

### 4. Flaky Tests
❌ **Bad**: Tests with non-deterministic results
```csharp
// Time-dependent test
var result = await _handler.Handle(command);
result.CreatedAt.ShouldBe(DateTime.UtcNow); // Flaky!
```

✅ **Good**: Use deterministic assertions
```csharp
result.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
```

### 5. Test Interdependence
❌ **Bad**: Tests that depend on execution order
```csharp
private static User _sharedUser; // Shared state between tests

[Fact]
public void Test1_CreateUser() 
{
    _sharedUser = new User { Name = "Test" };
}

[Fact]
public void Test2_UpdateUser() 
{
    _sharedUser.Name = "Updated"; // Depends on Test1
}
```

✅ **Good**: Independent tests
```csharp
[Fact]
public void Should_CreateUser()
{
    var user = new User { Name = "Test" };
    // Test logic
}

[Fact]
public void Should_UpdateUser()
{
    var user = new User { Name = "Test" }; // Independent setup
    user.Name = "Updated";
    // Test logic
}
```

### 6. Over-Mocking
❌ **Bad**: Mocking everything, including simple objects
```csharp
var mockString = new Mock<string>(); // Don't mock primitives
var mockList = new Mock<List<User>>(); // Don't mock simple collections
```

✅ **Good**: Mock only external dependencies
```csharp
var mockRepository = new Mock<IUserRepository>(); // Mock external dependency
var users = new List<User>(); // Use real list
```

### 7. Under-Testing
❌ **Bad**: Only testing happy path
```csharp
[Fact]
public async Task Should_CreateUser()
{
    var command = new CreateUserCommand { Name = "Test", Email = "test@example.com" };
    var result = await _handler.Handle(command);
    result.ShouldNotBeNull();
}
// Missing: validation tests, error scenarios, edge cases
```

✅ **Good**: Test edge cases and error scenarios
```csharp
[Fact]
public async Task Should_CreateUser_When_ValidRequest() { }

[Fact]
public async Task Should_ThrowValidationException_When_EmailIsInvalid() { }

[Fact]
public async Task Should_ThrowValidationException_When_NameIsEmpty() { }

[Fact]
public async Task Should_ThrowDomainException_When_EmailAlreadyExists() { }
```

### 8. Duplicate Tests
❌ **Bad**: Multiple tests verifying the same behavior
```csharp
[Fact]
public async Task Should_CreateUser_Test1() { /* Same test */ }

[Fact]
public async Task Should_CreateUser_Test2() { /* Same test */ }
```

✅ **Good**: One test per behavior, use theory for variations
```csharp
[Theory]
[InlineData("test1@example.com")]
[InlineData("test2@example.com")]
public async Task Should_CreateUser_When_ValidEmail(string email)
{
    var command = new CreateUserCommand { Email = email };
    var result = await _handler.Handle(command);
    result.ShouldNotBeNull();
}
```

## Continuous Improvement

### Review Test Failures
- Investigate and fix flaky tests immediately
- Don't ignore failing tests
- Update tests when requirements change

### Refactor Tests
- Keep tests maintainable and readable
- Remove duplicate code using helper methods
- Use test data builders for complex setup

### Update Tests
- Keep tests in sync with code changes
- Update tests before or alongside production code
- Don't leave broken tests in the codebase

### Monitor Coverage
- Track coverage trends over time
- Focus on covering critical paths
- Don't chase 100% coverage

### Test Performance
- Monitor and optimize slow tests
- Keep unit tests under 100ms
- Keep integration tests under 5 seconds
- Keep E2E tests under 10 seconds

### Team Reviews
- Include tests in code review process
- Review test quality, not just production code
- Share testing best practices with the team

## Project Structure Example

Complete example of a well-organized C# API test project:

```
YourApi.sln
├── src/
│   ├── YourApi/                    # API layer
│   ├── YourApi.Application/        # Application layer (handlers, validators)
│   ├── YourApi.Domain/             # Domain layer (entities, value objects)
│   └── YourApi.Infrastructure/     # Infrastructure layer (repositories, DbContext)
└── tests/
    ├── UnitTests/
    │   ├── YourApi.UnitTests.csproj
    │   ├── Features/
    │   │   └── Users/
    │   │       ├── CreateUserCommandHandlerTests.cs
    │   │       ├── GetUserQueryHandlerTests.cs
    │   │       └── UpdateUserCommandHandlerTests.cs
    │   ├── Validators/
    │   │   └── CreateUserCommandValidatorTests.cs
    │   ├── Mappers/
    │   │   └── UserMappingExtensionsTests.cs
    │   └── Builders/
    │       └── UserBuilder.cs
    ├── IntegrationTests/
    │   ├── YourApi.IntegrationTests.csproj
    │   ├── Repositories/
    │   │   └── UserRepositoryIntegrationTests.cs
    │   ├── Persistence/
    │   │   └── DatabaseMigrationsTests.cs
    │   └── Fixtures/
    │       └── DatabaseFixture.cs
    └── E2ETests/
        ├── YourApi.E2ETests.csproj
        ├── Features/
        │   └── UserManagementE2ETests.cs
        └── Fixtures/
            └── CustomWebApplicationFactory.cs
```

## Required NuGet Packages

### All Test Projects
```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="Shouldly" Version="4.2.1" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
```

### Unit Tests
```xml
<PackageReference Include="Moq" Version="4.20.70" />
<!-- OR -->
<PackageReference Include="NSubstitute" Version="5.1.0" />
```

### Integration Tests
```xml
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
<!-- OR for SQL Server -->
<PackageReference Include="Testcontainers.MsSql" Version="3.10.0" />
<PackageReference Include="WireMock.Net" Version="1.6.6" />
```

### E2E Tests
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

## Related Resources

- [xUnit Documentation](https://xunit.net/)
- [Shouldly Documentation](https://docs.shouldly.org/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [Microsoft.AspNetCore.Mvc.Testing Documentation](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## Customization for Your Project

This document provides a comprehensive framework for C# API testing. Customize it based on:
- Your specific architecture (Clean Architecture, Onion, N-Tier, etc.)
- Database technology (PostgreSQL, SQL Server, MongoDB, etc.)
- Authentication mechanism (JWT, OAuth, Identity Server, etc.)
- External integrations (payment gateways, third-party APIs, etc.)
- Team preferences (Moq vs NSubstitute, Shouldly vs FluentAssertions, etc.)

**Remember**: The goal is to build confidence in your API, catch bugs early, and enable safe refactoring. Focus on writing meaningful tests that provide value, not just achieving coverage metrics.
