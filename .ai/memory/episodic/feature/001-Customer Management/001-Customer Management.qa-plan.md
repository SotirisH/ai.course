# Test Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature
- **Implementation Plan**: `.ai/memory/episodic/feature/001-Customer Management/001-Customer Management.plan.md`

---

## Purpose

Validate that the Customer Management feature implements a complete, correct, and robust CRUD REST API for customer records. Tests must verify:

- All 5 endpoints (`POST`, `GET /{id}`, `GET`, `PUT /{id}`, `DELETE /{id}`) behave according to spec
- Input validation rejects invalid/malformed payloads with `400 Bad Request`
- Unique `tax_id` constraint enforcement returns `409 Conflict` on duplicate
- Missing entity scenarios return `404 Not Found`
- Mapping across all three boundary layers (API ↔ Application ↔ Infrastructure) is correct
- Database constraints (unique index, max lengths, required fields) are enforced

---

## Risks

| # | Risk | Severity | Mitigation |
|---|------|----------|------------|
| R1 | **TaxId uniqueness race condition** — Concurrent creates with same `TaxId` may cause `DbUpdateException` that returns a 500 instead of 409 | 🔴 High | Test duplicate key insertion under concurrency; verify exception middleware returns 409 |
| R2 | **Field mapping gaps** — A field added/renamed in one layer but not mapped across all three layers (Request → Command → DTO → Entity → DB) | 🔴 High | Dedicated mapping tests at each boundary, round-trip integration tests |
| R3 | **Nullable FirstName inconsistency** — The spec makes `first_name` optional but callers may expect it to be required. Downstream code may or may not handle null | 🟠 Medium | Test both null and non-null `FirstName` values through full pipeline |
| R4 | **Missing DELETE endpoint** — The implementation plan includes `DeleteCustomerCommand`, but it is absent from the current codebase | 🔴 High | Flag as gap; test plan includes DELETE scenarios but these cannot be executed until implemented |
| R5 | **GUID route binding** — Invalid or malformed GUIDs in route may not be handled gracefully | 🟠 Medium | Test edge cases: empty GUID, non-GUID string, valid GUID for non-existent record |
| R6 | **Max length truncation** — EF Core may silently truncate data exceeding `HasMaxLength` instead of throwing | 🟡 Low | Verify via integration tests with boundary values |
| R7 | **Hard delete implications** — No soft-delete means accidental data loss is permanent | 🟡 Low | Verify DELETE returns 204 and GET returns 404 afterward |

---

## Test Layers

| Layer | Project | Justification |
|-------|---------|---------------|
| **Application Unit** | `tests/Ai.Api.Application.Tests/` | Command/query handler logic with mocked `ICustomerRepository`. Fast execution, isolates business rules. |
| **Application Unit (Validation)** | `tests/Ai.Api.Application.Tests/` | FluentValidation validators for `CreateCustomerCommand` and `UpdateCustomerCommand`. No dependencies. |
| **Application Unit (Mapping)** | `tests/Ai.Api.Application.Tests/` | Extension methods for Command ↔ DTO mapping. Pure functions, no mocking needed. |
| **API Unit** | `tests/Ai.Api.Api.Tests/` | Controller actions with mocked `IMessageBus`. Verify HTTP status codes, routing, response types. |
| **API Unit (Mapping)** | `tests/Ai.Api.Api.Tests/` | Extension methods for Request ↔ Command and DTO ↔ Response mapping. Pure functions. |
| **Infrastructure Integration** | `tests/Ai.Api.Integration.Tests/` | Repository methods against real PostgreSQL via Testcontainers. Verify EF Core config, unique index, constraints. |
| **API Integration** | `tests/Ai.Api.Integration.Tests/` | Full HTTP round-trip via `WebApplicationFactory`. Verify end-to-end flow through all layers. |

---

## Test Scenarios (Gherkin)

### UC1: Create Customer (`POST /customers`)

```gherkin
@positive @create
Scenario: Create a customer with valid data
  Given a valid CreateCustomerRequest with first_name="John", last_name="Doe", tax_id="TX-12345", comments="VIP"
  When the client sends POST /customers
  Then the response status is 201 Created
  And the response body contains an Id (guid)
  And the response body contains first_name="John", last_name="Doe", tax_id="TX-12345", comments="VIP"

@positive @create @optional-firstname
Scenario: Create a customer without optional first_name
  Given a valid CreateCustomerRequest with first_name=null, last_name="Doe", tax_id="TX-12346"
  When the client sends POST /customers
  Then the response status is 201 Created

@positive @create @optional-comments
Scenario: Create a customer without optional comments
  Given a valid CreateCustomerRequest with first_name="John", last_name="Doe", tax_id="TX-12347", comments=null
  When the client sends POST /customers
  Then the response status is 201 Created
  And the response body contains comments=null

@negative @create @duplicate-taxid
Scenario: Create a customer with duplicate tax_id
  Given a customer exists with tax_id="TX-12345"
  And a CreateCustomerRequest with tax_id="TX-12345"
  When the client sends POST /customers
  Then the response status is 409 Conflict

@negative @create @validation
Scenario: Create a customer with missing required last_name
  Given a CreateCustomerRequest with first_name="John", last_name="", tax_id="TX-12348"
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "last_name"

@negative @create @validation
Scenario: Create a customer with missing required tax_id
  Given a CreateCustomerRequest with first_name="John", last_name="Doe", tax_id=""
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "tax_id"

@negative @create @validation
Scenario: Create a customer with last_name exceeding 256 characters
  Given a CreateCustomerRequest with last_name of 257 characters
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "last_name"

@negative @create @validation
Scenario: Create a customer with tax_id exceeding 16 characters
  Given a CreateCustomerRequest with tax_id of 17 characters
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "tax_id"

@negative @create @validation
Scenario: Create a customer with first_name exceeding 256 characters
  Given a CreateCustomerRequest with first_name of 257 characters
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "first_name"

@negative @create @validation
Scenario: Create a customer with comments exceeding 1024 characters
  Given a CreateCustomerRequest with comments of 1025 characters
  When the client sends POST /customers
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "comments"
```

### UC2: Get Customer by ID (`GET /customers/{id}`)

```gherkin
@positive @get-by-id
Scenario: Get an existing customer by ID
  Given a customer exists with id="{existingId}"
  When the client sends GET /customers/{existingId}
  Then the response status is 200 OK
  And the response body matches the customer data

@negative @get-by-id @not-found
Scenario: Get a non-existent customer by ID
  Given no customer exists with id="{nonExistentId}"
  When the client sends GET /customers/{nonExistentId}
  Then the response status is 404 Not Found

@negative @get-by-id @invalid-id
Scenario: Get customer with invalid GUID format in route
  When the client sends GET /customers/not-a-guid
  Then the response status is 400 Bad Request

@negative @get-by-id @empty-guid
Scenario: Get customer with empty GUID
  Given an empty GUID "00000000-0000-0000-0000-000000000000"
  When the client sends GET /customers/{emptyGuid}
  Then the response status is 404 Not Found
```

### UC3: Get All Customers (`GET /customers`)

```gherkin
@positive @get-all
Scenario: Get all customers when customers exist
  Given 3 customers exist in the system
  When the client sends GET /customers
  Then the response status is 200 OK
  And the response body contains a list of 3 customers

@positive @get-all @empty
Scenario: Get all customers when no customers exist
  Given no customers exist in the system
  When the client sends GET /customers
  Then the response status is 200 OK
  And the response body contains an empty list
```

### UC4: Update Customer (`PUT /customers/{id}`)

```gherkin
@positive @update
Scenario: Update an existing customer with valid data
  Given a customer exists with id="{existingId}" and last_name="Doe"
  And an UpdateCustomerRequest with last_name="Smith"
  When the client sends PUT /customers/{existingId}
  Then the response status is 200 OK
  And the response body contains last_name="Smith"

@negative @update @not-found
Scenario: Update a non-existent customer
  Given no customer exists with id="{nonExistentId}"
  And a valid UpdateCustomerRequest
  When the client sends PUT /customers/{nonExistentId}
  Then the response status is 404 Not Found

@negative @update @duplicate-taxid
Scenario: Update a customer to a tax_id that already exists
  Given customer A exists with tax_id="TX-11111"
  And customer B exists with tax_id="TX-22222"
  And an UpdateCustomerRequest for customer B with tax_id="TX-11111"
  When the client sends PUT /customers/{customerBId}
  Then the response status is 409 Conflict

@negative @update @validation
Scenario: Update a customer with missing required last_name
  Given a customer exists with id="{existingId}"
  And an UpdateCustomerRequest with last_name=""
  When the client sends PUT /customers/{existingId}
  Then the response status is 400 Bad Request
  And the response body contains a validation error for "last_name"

@negative @update @validation
Scenario: Update a customer with last_name exceeding 256 characters
  Given a customer exists with id="{existingId}"
  And an UpdateCustomerRequest with last_name of 257 characters
  When the client sends PUT /customers/{existingId}
  Then the response status is 400 Bad Request

@negative @update @validation
Scenario: Update a customer with empty Id in command
  Given an UpdateCustomerRequest with valid data
  When the client sends PUT /customers/{emptyGuid}
  Then the response status is 404 Not Found
```

### UC5: Delete Customer (`DELETE /customers/{id}`)

```gherkin
@positive @delete
Scenario: Delete an existing customer
  Given a customer exists with id="{existingId}"
  When the client sends DELETE /customers/{existingId}
  Then the response status is 204 No Content

@positive @delete @verify
Scenario: Delete an existing customer and verify it is removed
  Given a customer exists with id="{existingId}"
  When the client sends DELETE /customers/{existingId}
  Then the response status is 204 No Content
  And GET /customers/{existingId} returns 404 Not Found

@negative @delete @not-found
Scenario: Delete a non-existent customer
  Given no customer exists with id="{nonExistentId}"
  When the client sends DELETE /customers/{nonExistentId}
  Then the response status is 404 Not Found
```

### UC6: Mapping Tests

```gherkin
@mapping @api-to-application
Scenario: API CreateCustomerRequest maps correctly to CreateCustomerCommand
  Given a CreateCustomerRequest with all fields populated
  When ToCommand() is called
  Then the resulting CreateCustomerCommand has identical field values

@mapping @api-to-application
Scenario: API UpdateCustomerRequest maps correctly to UpdateCustomerCommand
  Given an UpdateCustomerRequest with all fields populated and a Guid id
  When ToCommand(id) is called
  Then the resulting UpdateCustomerCommand has identical field values and the given id

@mapping @api-to-application
Scenario: Application CustomerDto maps correctly to API CustomerResponse
  Given a CustomerDto with all fields populated
  When ToResponse() is called
  Then the resulting CustomerResponse has identical field values

@mapping @application-to-infrastructure
Scenario: CreateCustomerCommand maps correctly to CreateCustomerDto
  Given a CreateCustomerCommand with all fields populated
  When ToDto() is called
  Then the resulting CreateCustomerDto has identical field values

@mapping @infrastructure-to-application
Scenario: Customers entity maps correctly to CustomerDto (ToDto)
  Given a Customers entity with all fields populated
  When ToDto() is called
  Then the resulting CustomerDto has identical field values

@mapping @application-to-infrastructure
Scenario: CreateCustomerDto maps correctly to Customers entity (ToEntity)
  Given a CreateCustomerDto with all fields populated
  When ToEntity() is called
  Then the resulting Customers entity has identical field values

@mapping @update
Scenario: CustomerDto.ApplyTo updates Customers entity fields correctly
  Given an existing Customers entity with field values "A"
  And a CustomerDto with field values "B"
  When ApplyTo(entity) is called
  Then the entity field values are updated to "B"
```

### UC7: Database Constraint Tests

```gherkin
@db @unique-index
Scenario: Database unique index on TaxId rejects duplicate values
  Given a customer is inserted with tax_id="UNIQUE-001"
  When a second customer is inserted with tax_id="UNIQUE-001"
  Then a DbUpdateException with a "duplicate key" violation is thrown

@db @max-length
Scenario: Database enforces LastName max length of 256
  When a customer is inserted with last_name of 257 characters
  Then a DbUpdateException is thrown (or data is truncated based on provider behavior)

@db @required-fields
Scenario: Database enforces LastName as required (not null)
  When a customer is inserted with last_name=null
  Then a DbUpdateException is thrown

@db @required-fields
Scenario: Database enforces TaxId as required (not null)
  When a customer is inserted with tax_id=null
  Then a DbUpdateException is thrown

@db @nullable
Scenario: Database allows null FirstName
  Given a customer is created with first_name=null
  When the customer is retrieved
  Then first_name is null

@db @nullable
Scenario: Database allows null Comments
  Given a customer is created with comments=null
  When the customer is retrieved
  Then comments is null
```

---

## Test File Map

### Application Unit Tests (`tests/Ai.Api.Application.Tests/`)

| Scenario | Test Class | Test Method |
|----------|-----------|-------------|
| UC1 - Create with valid data | `CreateCustomerCommandHandlerTests` | `Should_CreateCustomer_When_ValidCommand()` |
| UC1 - Create with duplicate tax_id | `CreateCustomerCommandHandlerTests` | `Should_ThrowInvalidOperationException_When_DuplicateTaxId()` |
| UC1 - Create - missing last_name validation | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_LastNameIsEmpty()` |
| UC1 - Create - missing tax_id validation | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_TaxIdIsEmpty()` |
| UC1 - Create - last_name exceeding 256 | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_LastNameExceedsMaxLength()` |
| UC1 - Create - tax_id exceeding 16 | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_TaxIdExceedsMaxLength()` |
| UC1 - Create - first_name exceeding 256 | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_FirstNameExceedsMaxLength()` |
| UC1 - Create - comments exceeding 1024 | `CreateCustomerCommandValidatorTests` | `Should_HaveError_When_CommentsExceedsMaxLength()` |
| UC1 - Create - valid command passes validation | `CreateCustomerCommandValidatorTests` | `Should_NotHaveError_When_CommandIsValid()` |
| UC2 - Get existing customer by ID | `GetCustomerByIdQueryHandlerTests` | `Should_ReturnCustomer_When_IdExists()` |
| UC2 - Get non-existent customer | `GetCustomerByIdQueryHandlerTests` | `Should_ThrowInvalidOperationException_When_IdNotFound()` |
| UC3 - Get all with customers | `GetCustomersQueryHandlerTests` | `Should_ReturnAllCustomers_When_CustomersExist()` |
| UC3 - Get all when empty | `GetCustomersQueryHandlerTests` | `Should_ReturnEmptyList_When_NoCustomersExist()` |
| UC4 - Update existing customer | `UpdateCustomerCommandHandlerTests` | `Should_UpdateCustomer_When_ValidCommand()` |
| UC4 - Update non-existent customer | `UpdateCustomerCommandHandlerTests` | `Should_ThrowInvalidOperationException_When_IdNotFound()` |
| UC4 - Update with duplicate tax_id | `UpdateCustomerCommandHandlerTests` | `Should_ThrowInvalidOperationException_When_DuplicateTaxId()` |
| UC4 - Update - empty Id validation | `UpdateCustomerCommandValidatorTests` | `Should_HaveError_When_IdIsEmpty()` |
| UC4 - Update - missing last_name validation | `UpdateCustomerCommandValidatorTests` | `Should_HaveError_When_LastNameIsEmpty()` |
| UC4 - Update - last_name exceeding 256 | `UpdateCustomerCommandValidatorTests` | `Should_HaveError_When_LastNameExceedsMaxLength()` |
| UC4 - Update - valid command passes validation | `UpdateCustomerCommandValidatorTests` | `Should_NotHaveError_When_CommandIsValid()` |
| UC5 - Delete existing customer | *(DeleteCustomerCommandHandler — pending implementation)* | `Should_DeleteCustomer_When_IdExists()` |
| UC5 - Delete non-existent customer | *(DeleteCustomerCommandHandler — pending implementation)* | `Should_ThrowInvalidOperationException_When_IdNotFound()` |
| UC6 - Command → DTO mapping (Create) | `CustomerMappingExtensionsTests` | `Should_MapCreateCustomerCommandToCreateCustomerDto()` |
| UC6 - Command → DTO mapping (Update) | `CustomerMappingExtensionsTests` | `Should_MapUpdateCustomerCommandToCustomerDto()` |

### API Unit Tests (`tests/Ai.Api.Api.Tests/`)

| Scenario | Test Class | Test Method |
|----------|-----------|-------------|
| UC1 - POST 201 Created | `CustomersControllerTests` | `Should_Return201Created_When_ValidCreateRequest()` |
| UC1 - POST 409 Conflict (duplicate tax_id) | `CustomersControllerTests` | `Should_Return409Conflict_When_DuplicateTaxId()` |
| UC1 - POST 400 Bad Request (validation) | `CustomersControllerTests` | `Should_Return400BadRequest_When_InvalidCreateRequest()` |
| UC2 - GET by ID 200 OK | `CustomersControllerTests` | `Should_Return200Ok_When_CustomerExists()` |
| UC2 - GET by ID 404 Not Found | `CustomersControllerTests` | `Should_Return404NotFound_When_CustomerDoesNotExist()` |
| UC3 - GET all 200 OK | `CustomersControllerTests` | `Should_Return200Ok_WithCustomerList()` |
| UC3 - GET all empty list | `CustomersControllerTests` | `Should_Return200Ok_WithEmptyList_WhenNoCustomers()` |
| UC4 - PUT 200 OK | `CustomersControllerTests` | `Should_Return200Ok_When_ValidUpdateRequest()` |
| UC4 - PUT 404 Not Found | `CustomersControllerTests` | `Should_Return404NotFound_When_UpdateNonExistent()` |
| UC4 - PUT 409 Conflict | `CustomersControllerTests` | `Should_Return409Conflict_When_UpdateDuplicateTaxId()` |
| UC4 - PUT 400 Bad Request | `CustomersControllerTests` | `Should_Return400BadRequest_When_InvalidUpdateRequest()` |
| UC5 - DELETE 204 No Content | `CustomersControllerTests` | `Should_Return204NoContent_When_DeleteExisting()` |
| UC5 - DELETE 404 Not Found | `CustomersControllerTests` | `Should_Return404NotFound_When_DeleteNonExistent()` |
| UC6 - Request → Command mapping | `CustomerMappingExtensionsTests` | `Should_MapCreateRequestToCommand()` |
| UC6 - Request → Command mapping (Update) | `CustomerMappingExtensionsTests` | `Should_MapUpdateRequestToCommand()` |
| UC6 - DTO → Response mapping | `CustomerMappingExtensionsTests` | `Should_MapDtoToResponse()` |
| UC6 - DTO list → Response list mapping | `CustomerMappingExtensionsTests` | `Should_MapDtoListToResponseList()` |

### Infrastructure Integration Tests (`tests/Ai.Api.Integration.Tests/`)

| Scenario | Test Class | Test Method |
|----------|-----------|-------------|
| UC7 - Unique index on TaxId | `CustomerRepositoryIntegrationTests` | `Should_ThrowDbUpdateException_When_DuplicateTaxId()` |
| UC7 - Max length on LastName | `CustomerRepositoryIntegrationTests` | `Should_ThrowDbUpdateException_When_LastNameTooLong()` |
| UC7 - Required LastName | `CustomerRepositoryIntegrationTests` | `Should_ThrowDbUpdateException_When_LastNameIsNull()` |
| UC7 - Required TaxId | `CustomerRepositoryIntegrationTests` | `Should_ThrowDbUpdateException_When_TaxIdIsNull()` |
| UC7 - Nullable FirstName | `CustomerRepositoryIntegrationTests` | `Should_AllowNullFirstName()` |
| UC7 - Nullable Comments | `CustomerRepositoryIntegrationTests` | `Should_AllowNullComments()` |
| UC1 - Repository AddAsync | `CustomerRepositoryIntegrationTests` | `Should_AddCustomer_When_ValidDto()` |
| UC2 - Repository GetByIdAsync | `CustomerRepositoryIntegrationTests` | `Should_GetCustomerById_When_Exists()` |
| UC2 - Repository GetByIdAsync null | `CustomerRepositoryIntegrationTests` | `Should_ReturnNull_When_GetByIdNotExists()` |
| UC3 - Repository GetAllAsync | `CustomerRepositoryIntegrationTests` | `Should_GetAllCustomers()` |
| UC4 - Repository UpdateAsync | `CustomerRepositoryIntegrationTests` | `Should_UpdateCustomer_When_Exists()` |
| UC5 - Repository DeleteAsync | `CustomerRepositoryIntegrationTests` | `Should_DeleteCustomer_When_Exists()` |
| UC6 - Entity → DTO mapping | `CustomerPersistenceMappingExtensionsTests` | `Should_MapEntityToDto()` |
| UC6 - DTO → Entity mapping | `CustomerPersistenceMappingExtensionsTests` | `Should_MapCreateDtoToEntity()` |
| UC6 - ApplyTo mapping | `CustomerPersistenceMappingExtensionsTests` | `Should_ApplyDtoToEntity()` |

### API Integration Tests (`tests/Ai.Api.Integration.Tests/`)

| Scenario | Test Class | Test Method |
|----------|-----------|-------------|
| UC1 - Full create flow | `CustomerEndpointsTests` | `Should_CreateCustomer_When_ValidRequest()` |
| UC1 - Create duplicate tax_id | `CustomerEndpointsTests` | `Should_Return409_When_DuplicateTaxId()` |
| UC1 - Create validation error | `CustomerEndpointsTests` | `Should_Return400_When_InvalidCreateRequest()` |
| UC2 - Get by ID | `CustomerEndpointsTests` | `Should_GetCustomerById_When_Exists()` |
| UC2 - Get by ID not found | `CustomerEndpointsTests` | `Should_Return404_When_CustomerNotFound()` |
| UC3 - Get all | `CustomerEndpointsTests` | `Should_GetAllCustomers()` |
| UC4 - Update | `CustomerEndpointsTests` | `Should_UpdateCustomer_When_ValidRequest()` |
| UC4 - Update not found | `CustomerEndpointsTests` | `Should_Return404_When_UpdateNonExistent()` |
| UC4 - Update duplicate tax_id | `CustomerEndpointsTests` | `Should_Return409_When_UpdateDuplicateTaxId()` |
| UC5 - Delete | `CustomerEndpointsTests` | `Should_DeleteCustomer_When_Exists()` |
| UC5 - Delete not found | `CustomerEndpointsTests` | `Should_Return404_When_DeleteNonExistent()` |
| UC5 - Delete then verify gone | `CustomerEndpointsTests` | `Should_Return404_AfterDelete()` |

---

## Automation Approach

### Tools

| Tool | Purpose |
|------|---------|
| **xUnit** | Test runner for all test projects |
| **Shouldly** | Fluent assertions |
| **Moq** | Mocking dependencies in unit tests |
| **WebApplicationFactory** | In-memory test server for API integration tests |
| **Testcontainers (PostgreSQL)** | Real PostgreSQL database for infrastructure and API integration tests |
| **Microsoft.AspNetCore.Mvc.Testing** | Custom factory setup for API tests |

### Project Setup

| Test Project | Target Framework | Key Packages |
|-------------|-----------------|--------------|
| `tests/Ai.Api.Application.Tests/` | .NET 10 | xUnit, Shouldly, Moq |
| `tests/Ai.Api.Api.Tests/` | .NET 10 | xUnit, Shouldly, Moq, Microsoft.AspNetCore.Mvc.Testing |
| `tests/Ai.Api.Integration.Tests/` | .NET 10 | xUnit, Shouldly, Testcontainers.PostgreSql, Microsoft.AspNetCore.Mvc.Testing |

### Test Data Builders (Recommended)

Create a `CustomerBuilder` helper class for unit test data setup:

```csharp
public static class CustomerBuilder
{
    public static CreateCustomerCommand ValidCreateCommand() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        TaxId = "TX-" + Guid.NewGuid().ToString("N")[..8],
        Comments = "Test customer"
    };

    public static CustomerDto ExistingCustomerDto() => new()
    {
        Id = Guid.CreateVersion7(),
        FirstName = "Jane",
        LastName = "Smith",
        TaxId = "TX-EXISTING-001",
        Comments = "Existing customer"
    };
}
```

### Integration Test Fixture

```csharp
public class CustomerTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; private set; }
    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        Container = new PostgreSqlBuilder()
            .WithDatabase("customer_test")
            .Build();
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
```

---

## Missing Information / Open Questions

| # | Question | Impact | Status |
|---|----------|--------|--------|
| Q1 | **DELETE endpoint not implemented** — The implementation plan includes `DeleteCustomerCommand` + handler + `DELETE /customers/{id}` but the controller has no delete action and the command handler does not exist in the codebase. | Tests cannot be executed. DELETE endpoint must be implemented first. | **🔴 Blocking** |
| Q2 | Should `first_name` be mandatory despite the model not listing it as required? | Validator design, API contract. Current validator accepts empty first_name. | Clarification needed |
| Q3 | Should `tax_id` have a specific format validation (e.g., SSN, VAT, alphanumeric pattern)? | Validator design. Currently no format validation. | Low priority |
| Q4 | Should `GET /customers` support pagination/sorting/filtering in initial implementation? | Query handler design. Currently returns all records. | Low — out of MVP scope |
| Q5 | Is the `DELETE` endpoint intended to be a hard delete or soft delete? | Implementation approach. Plan assumes hard delete. | Assumed resolved |

### Key Gap: DELETE Endpoint Not Yet Implemented

The implementation plan (Section 5.2) lists `DeleteCustomerCommand.cs` as a CREATE action, but this file does not exist in the codebase. The `CustomersController` also lacks a `DELETE` action. This is a **blocking gap** for the test plan. All DELETE scenarios above are documented but cannot be executed until this is implemented. Tests for DELETE should be written simultaneously with the implementation.
