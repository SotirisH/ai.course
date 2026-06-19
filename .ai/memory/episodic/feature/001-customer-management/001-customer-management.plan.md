# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. The feature provides full CRUD (Create, Read, Update, Delete) and list operations for Customer entities via a RESTful API.

---

## Acceptance Criteria

**Given** an administrator is authenticated in the system
**When** they interact with the `/customers` endpoints
**Then** they should be able to:

| Operation | Method | Endpoint | Description |
|-----------|--------|----------|-------------|
| Create | `POST` | `/customers` | Create a new customer |
| Update | `PUT` | `/customers/{id}` | Update an existing customer |
| Get by ID | `GET` | `/customers/{id}` | Retrieve a single customer |
| List all | `GET` | `/customers` | Retrieve all customers |
| Delete | `DELETE` | `/customers/{id}` | Delete a customer |

### Customer Model

| Field | Type | Constraints |
|-------|------|-------------|
| `id` | `Guid` | Primary key |
| `first_name` | `string(256)` | Optional |
| `last_name` | `string(256)` | **Mandatory** |
| `tax_id` | `string(16)` | **Mandatory**, **Unique** |
| `comments` | `string(1024)` | Optional |

---

## Spec Consistency Check

| # | Finding | Severity |
|---|---------|----------|
| 1 | `first_name` is not marked mandatory while `last_name` is — this asymmetry is intentional per the model spec | ✅ Info |
| 2 | `tax_id` has a `unique` constraint — requires a unique database index | ✅ Info |
| 3 | Model uses snake_case naming (database convention); C# code will use PascalCase | ✅ Info |
| 4 | No contradictions between story, acceptance criteria, and model definition | ✅ Clean |

---

## File Change List

All files are **CREATE** (no existing customer-related files found in pre-scaffold scan).

### Domain Layer (`src/Ai.Api.Domain/`)
*No new files required.* The existing `DomainException` class is sufficient for this feature.

### Application Layer (`src/Ai.Api.Application/`)

| # | File | Action | Purpose |
|---|------|--------|---------|
| 1 | `Features/CustomerManagement/DTOs/CustomerDto.cs` | CREATE | Read/output DTO for Customer |
| 2 | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | CREATE | Input DTO for creating a Customer |
| 3 | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | CREATE | Command + Handler for creating a Customer |
| 4 | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | CREATE | Command + Handler for updating a Customer |
| 5 | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | CREATE | Command + Handler for deleting a Customer |
| 6 | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | CREATE | Query + Handler for getting a Customer by ID |
| 7 | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | CREATE | Query + Handler for listing all Customers |
| 8 | `Interfaces/Repositories/ICustomerRepository.cs` | CREATE | Repository interface for Customer data access |
| 9 | `Mappings/CustomerMappingExtensions.cs` | CREATE | Extension methods for mapping commands → DTOs |
| 10 | `Validators/CreateCustomerCommandValidator.cs` | CREATE | FluentValidation validator for CreateCustomerCommand |
| 11 | `Validators/UpdateCustomerCommandValidator.cs` | CREATE | FluentValidation validator for UpdateCustomerCommand |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)

| # | File | Action | Purpose |
|---|------|--------|---------|
| 12 | `Persistence/Entities/Customer.cs` | CREATE | EF Core entity for the `Customers` table |
| 13 | `Persistence/Configurations/CustomerEntityConfiguration.cs` | CREATE | Fluent API configuration for Customer entity |
| 14 | `Persistence/Repositories/CustomerRepository.cs` | CREATE | Repository implementation for Customer |
| 15 | `Persistence/CustomerPersistenceMappingExtensions.cs` | CREATE | Extension methods for entity ↔ DTO mapping |
| 16 | `Persistence/Context/AppDbContext.cs` | MODIFY | Add `DbSet<Customer>` property |

### API Layer (`src/Ai.Api/`)

| # | File | Action | Purpose |
|---|------|--------|---------|
| 17 | `Controllers/CustomersController.cs` | CREATE | API controller for `/customers` endpoints |
| 18 | `Models/Requests/CreateCustomerRequest.cs` | CREATE | Request model for POST `/customers` |
| 19 | `Models/Requests/UpdateCustomerRequest.cs` | CREATE | Request model for PUT `/customers/{id}` |
| 20 | `Models/Responses/CustomerResponse.cs` | CREATE | Response model for Customer endpoints |
| 21 | `Mappers/CustomerMappingExtensions.cs` | CREATE | Extension methods for request → command and DTO → response |

### Dependency Injection

| # | File | Action | Purpose |
|---|------|--------|---------|
| 22 | `src/Ai.Api.Infrastructure/DependencyInjection.cs` | MODIFY | Register `ICustomerRepository` / `CustomerRepository` |

---

## Implementation Details

### 1. DTOs (Application Layer)

**CustomerDto** — the read/output contract:
```csharp
public sealed record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

**CreateCustomerDto** — the input contract for creation:
```csharp
public sealed record CreateCustomerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

### 2. Commands & Queries (Application Layer)

Following the existing `ApplicationManagement` pattern:
- **CreateCustomerCommand** → `CreateCustomerCommandHandler` returns `CustomerDto`
- **UpdateCustomerCommand** → `UpdateCustomerCommandHandler` returns `CustomerDto` (throws `InvalidOperationException` if not found)
- **DeleteCustomerCommand** → `DeleteCustomerCommandHandler` returns void (throws `InvalidOperationException` if not found)
- **GetCustomerByIdQuery** → `GetCustomerByIdQueryHandler` returns `CustomerDto` (throws `InvalidOperationException` if not found)
- **GetCustomersQuery** → `GetCustomersQueryHandler` returns `IReadOnlyList<CustomerDto>`

All handlers use Wolverine mediator pattern (command/query in same file as handler).

### 3. Repository Interface (Application Layer)

```csharp
public interface ICustomerRepository
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> AddAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(CustomerDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

### 4. Entity & Configuration (Infrastructure Layer)

**Customer entity** — maps to `Customers` table:
- `Id` (Guid, PK)
- `FirstName` (string, max 256)
- `LastName` (string, max 256, required)
- `TaxId` (string, max 16, required, unique index)
- `Comments` (string, max 1024, nullable)

**Fluent API configuration** (`CustomerEntityConfiguration`):
- Table: `"Customers"`
- PK on `Id`
- `LastName` → `.IsRequired().HasMaxLength(256)`
- `TaxId` → `.IsRequired().HasMaxLength(16)`
- Unique index on `TaxId`
- `FirstName` → `.HasMaxLength(256)`
- `Comments` → `.HasMaxLength(1024)`

### 5. Repository Implementation (Infrastructure Layer)

Follows the exact pattern of `ApplicationRepository`:
- `GetByIdAsync`: `AsNoTracking().FirstOrDefaultAsync()`, maps entity → DTO
- `GetAllAsync`: `AsNoTracking().ToListAsync()`, maps each entity → DTO
- `AddAsync`: maps DTO → entity, `AddAsync()`, `SaveChangesAsync()`, catches `DbUpdateException` for duplicate `tax_id`
- `UpdateAsync`: loads tracked entity, applies DTO values, `SaveChangesAsync()`, catches `DbUpdateException` for duplicate `tax_id`
- `DeleteAsync`: loads entity, `Remove()`, `SaveChangesAsync()`
- `IsDuplicateKeyViolation` helper for detecting unique constraint violations

### 6. Validators (Application Layer)

**CreateCustomerCommandValidator**:
- `LastName` → `.NotEmpty()` (mandatory)
- `TaxId` → `.NotEmpty()` (mandatory), `.MaximumLength(16)`
- `FirstName` → `.MaximumLength(256)`
- `Comments` → `.MaximumLength(1024)`

**UpdateCustomerCommandValidator**:
- `Id` → `.NotEmpty()`
- Same field validations as Create

### 7. API Controller (API Layer)

**CustomersController** — follows `ApplicationsController` pattern:
- Uses `IMessageBus` (Wolverine mediator)
- `POST /customers` → `CreateCustomerRequest` → `CreateCustomerCommand` → returns `201 Created` with `CustomerResponse`
- `PUT /customers/{id:guid}` → `UpdateCustomerRequest` + route `id` → `UpdateCustomerCommand` → returns `200 OK` with `CustomerResponse`
- `GET /customers/{id:guid}` → `GetCustomerByIdQuery` → returns `200 OK` with `CustomerResponse`
- `GET /customers` → `GetCustomersQuery` → returns `200 OK` with `IReadOnlyList<CustomerResponse>`
- `DELETE /customers/{id:guid}` → `DeleteCustomerCommand` → returns `204 NoContent`
- Uses `[ApiConventionMethod]` where applicable
- `[ProducesResponseType(StatusCodes.Status409Conflict)]` on Create and Update (for duplicate `tax_id`)

### 8. Request/Response Models (API Layer)

**CreateCustomerRequest**: `FirstName`, `LastName`, `TaxId`, `Comments`
**UpdateCustomerRequest**: `FirstName`, `LastName`, `TaxId`, `Comments`
**CustomerResponse**: `Id`, `FirstName`, `LastName`, `TaxId`, `Comments`

### 9. Mapping Extensions

**API Layer** (`CustomerMappingExtensions`):
- `CreateCustomerRequest.ToCommand()` → `CreateCustomerCommand`
- `UpdateCustomerRequest.ToCommand(Guid id)` → `UpdateCustomerCommand`
- `Guid.ToCommand()` → `DeleteCustomerCommand` (extension on Guid)
- `CustomerDto.ToResponse()` → `CustomerResponse`
- `IEnumerable<CustomerDto>.ToResponseList()` → `List<CustomerResponse>`

**Application Layer** (`CustomerMappingExtensions`):
- `CreateCustomerCommand.ToDto()` → `CreateCustomerDto`
- `UpdateCustomerCommand.ApplyTo(CustomerDto existing)` → `CustomerDto` (with-expression)

**Infrastructure Layer** (`CustomerPersistenceMappingExtensions`):
- `CustomerEntity.ToDto()` → `CustomerDto`
- `CreateCustomerDto.ToEntity()` → `CustomerEntity` (generates `Guid.CreateVersion7()`)
- `CustomerDto.ApplyTo(CustomerEntity entity)` → void (mutates entity)

### 10. Error Handling

The existing `ExceptionHandlingMiddleware` already handles:
- `ValidationException` → 400 Bad Request
- `InvalidOperationException` with "was not found" → 404 Not Found
- `InvalidOperationException` with "already exists" → 409 Conflict

No middleware changes needed. Handlers throw `InvalidOperationException` for not-found and duplicate scenarios, which the middleware maps correctly.

---

## Implementation Order

1. **Domain Layer** — No changes needed
2. **Application Layer — DTOs** (`CustomerDto`, `CreateCustomerDto`)
3. **Application Layer — Repository Interface** (`ICustomerRepository`)
4. **Application Layer — Commands & Queries** (all 5 handler files)
5. **Application Layer — Validators** (Create + Update)
6. **Application Layer — Mappings** (`CustomerMappingExtensions`)
7. **Infrastructure Layer — Entity** (`Customer`)
8. **Infrastructure Layer — Entity Configuration** (`CustomerEntityConfiguration`)
9. **Infrastructure Layer — Persistence Mappings** (`CustomerPersistenceMappingExtensions`)
10. **Infrastructure Layer — Repository** (`CustomerRepository`)
11. **Infrastructure Layer — DbContext** (add `DbSet<Customer>`)
12. **Infrastructure Layer — DI Registration** (register `ICustomerRepository`)
13. **API Layer — Request/Response Models**
14. **API Layer — Mappings** (`CustomerMappingExtensions`)
15. **API Layer — Controller** (`CustomersController`)
16. **Database Migration** — create and apply EF Core migration
17. **Testing** — unit tests, integration tests, API tests

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | `first_name` is optional (not mandatory) | Only `last_name` is marked "mandatory" in the model spec. `first_name` has no such trait. |
| A2 | `tax_id` uniqueness is enforced at the database level via a unique index | The model spec marks `tax_id` as "unique". Following the existing pattern (unique index on `Name` in `ApplicationEntityConfiguration`), this is enforced via Fluent API configuration. |
| A3 | No authentication/authorization is implemented at this stage | The story says "As an administrator" but the existing `ApplicationsController` has no auth. Auth is a cross-cutting concern to be added later. |
| A4 | The `Customers` table name is plural | Following the existing `Applications` table naming convention. |
| A5 | `Guid.CreateVersion7()` is used for ID generation | Per architecture rules, and following the existing `ApplicationPersistenceMappingExtensions` pattern. |
| A6 | Wolverine mediator (`IMessageBus`) is used for command/query dispatch | Following the existing `ApplicationsController` pattern. |
| A7 | No custom domain exceptions are needed | The existing `InvalidOperationException` pattern (caught by middleware) is sufficient for not-found and duplicate scenarios. |
| A8 | The `tax_id` field maps to `TaxId` in C# (PascalCase) | Standard .NET naming convention. Snake_case in the model spec is the database convention. |

---

## Open Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should `first_name` also be mandatory? The model spec only marks `last_name` as mandatory, but this seems asymmetric. | Changes validator and entity configuration |
| Q2 | Should `tax_id` have any format validation (e.g., regex pattern for tax ID format)? | Adds validation rule in FluentValidation |
| Q3 | Should the GET `/customers` endpoint support pagination, filtering, or sorting? The acceptance criteria only specifies a simple list. | Could significantly change the query handler and repository |
| Q4 | Should there be a unique constraint on the combination of `first_name` + `last_name` + `tax_id`, or just `tax_id` alone? | Affects database index design |
