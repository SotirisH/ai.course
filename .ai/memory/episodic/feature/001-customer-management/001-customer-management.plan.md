# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, provide full CRUD (Create, Read, Update, Delete) and list capabilities for Customers. Each Customer has a unique identifier (GUID), first name, last name (mandatory), tax ID (mandatory, unique), and optional comments. The feature exposes a RESTful API at `/customers`.

---

## Acceptance Criteria

**Given** an authenticated administrator  
**When** they send a `POST /customers` with valid first_name, last_name, and tax_id  
**Then** a new Customer is created and returned with a `201 Created` status.

**Given** an authenticated administrator  
**When** they send a `PUT /customers/{id}` with updated fields  
**Then** the Customer is updated and returned with a `200 OK` status.

**Given** an authenticated administrator  
**When** they send a `GET /customers/{id}` for an existing Customer  
**Then** the Customer is returned with a `200 OK` status.

**Given** an authenticated administrator  
**When** they send a `GET /customers`  
**Then** all Customers are returned as a list with a `200 OK` status.

**Given** an authenticated administrator  
**When** they send a `DELETE /customers/{id}` for an existing Customer  
**Then** the Customer is deleted and `204 No Content` is returned.

**Given** a duplicate tax_id is submitted  
**When** creating or updating a Customer  
**Then** a `409 Conflict` response is returned.

**Given** a non-existent Customer ID is requested  
**When** fetching, updating, or deleting  
**Then** a `404 Not Found` response is returned.

---

## Spec Issues

| # | Issue | Severity |
|---|-------|----------|
| 1 | **`first_name` mandatory?** — `last_name` is explicitly marked "Traits: mandatory", but `first_name` has no such trait. Is `first_name` required or optional? | Medium |
| 2 | **No pagination on GET `/customers`** — Returning all customers without pagination could become a performance issue at scale. | Low |

---

## Pre-Scaffold Detection Results

No existing customer-related files were found in any layer. All files are new (`CREATE`).

---

## File Change List

### Domain Layer — No Changes

The Domain layer does not require new exceptions, enums, or events for this feature. Existing `InvalidOperationException` from the framework is used for not-found and duplicate-key scenarios (matching the Applications pattern).

### Application Layer

| # | File | Action | Path |
|---|------|--------|------|
| A1 | `CustomerDto.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/` |
| A2 | `CreateCustomerDto.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/` |
| A3 | `CreateCustomerCommand.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A4 | `UpdateCustomerCommand.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A5 | `DeleteCustomerCommand.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A6 | `GetCustomerByIdQuery.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/` |
| A7 | `GetCustomersQuery.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/` |
| A8 | `ICustomerRepository.cs` | CREATE | `src/Ai.Api.Application/Interfaces/Repositories/` |
| A9 | `CreateCustomerCommandValidator.cs` | CREATE | `src/Ai.Api.Application/Validators/` |
| A10 | `UpdateCustomerCommandValidator.cs` | CREATE | `src/Ai.Api.Application/Validators/` |
| A11 | `CustomerMappingExtensions.cs` | CREATE | `src/Ai.Api.Application/Mappings/` |

### Infrastructure Layer

| # | File | Action | Path |
|---|------|--------|------|
| I1 | `Customer.cs` (entity) | CREATE | `src/Ai.Api.Infrastructure/Persistence/Entities/` |
| I2 | `CustomerEntityConfiguration.cs` | CREATE | `src/Ai.Api.Infrastructure/Persistence/Configurations/` |
| I3 | `CustomerRepository.cs` | CREATE | `src/Ai.Api.Infrastructure/Persistence/Repositories/` |
| I4 | `CustomerPersistenceMappingExtensions.cs` | CREATE | `src/Ai.Api.Infrastructure/Persistence/` |
| I5 | `AppDbContext.cs` | UPDATE | `src/Ai.Api.Infrastructure/Persistence/Context/` |
| I6 | `DependencyInjection.cs` | UPDATE | `src/Ai.Api.Infrastructure/` |

### API Layer

| # | File | Action | Path |
|---|------|--------|------|
| P1 | `CustomersController.cs` | CREATE | `src/Ai.Api/Controllers/` |
| P2 | `CreateCustomerRequest.cs` | CREATE | `src/Ai.Api/Models/Requests/` |
| P3 | `UpdateCustomerRequest.cs` | CREATE | `src/Ai.Api/Models/Requests/` |
| P4 | `CustomerResponse.cs` | CREATE | `src/Ai.Api/Models/Responses/` |
| P5 | `CustomerMappingExtensions.cs` | CREATE | `src/Ai.Api/Mappers/` |

---

## Implementation Details

### Data Model (Database Table: `Customers`)

| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | `Guid` | PK, generated via `Guid.CreateVersion7()` |
| `FirstName` | `string(256)` | TBD — see Question 1 |
| `LastName` | `string(256)` | Required (`IsRequired()`) |
| `TaxId` | `string(16)` | Required, **Unique Index** |
| `Comments` | `string(1024)` | Optional |

### API Contract

**`POST /customers`**
- Request: `CreateCustomerRequest` → `{ FirstName, LastName, TaxId, Comments }`
- Response: `201 Created` + `CustomerResponse` body
- Errors: `400` (validation), `409` (duplicate tax_id)

**`PUT /customers/{id:guid}`**
- Request: `UpdateCustomerRequest` → `{ FirstName, LastName, TaxId, Comments }`
- Response: `200 OK` + `CustomerResponse` body
- Errors: `400` (validation), `404` (not found), `409` (duplicate tax_id)

**`GET /customers/{id:guid}`**
- Response: `200 OK` + `CustomerResponse` body
- Errors: `404` (not found)

**`GET /customers`**
- Response: `200 OK` + `List<CustomerResponse>` body

**`DELETE /customers/{id:guid}`**
- Response: `204 No Content`
- Errors: `404` (not found)

### Layer-by-Layer Design

#### 1. Domain Layer
No changes. Uses framework `InvalidOperationException` for not-found / duplicate scenarios (matching existing Application Management pattern).

#### 2. Application Layer — DTOs

```csharp
// CustomerDto.cs
public sealed record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

// CreateCustomerDto.cs
public sealed record CreateCustomerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

#### 3. Application Layer — Commands & Queries (Wolverine CQRS)

Following the established pattern: each command/query record is co-located with its handler class in the same file.

- **CreateCustomerCommand** → handler maps command → `CreateCustomerDto`, calls `ICustomerRepository.AddAsync`
- **UpdateCustomerCommand** → handler fetches existing, applies changes, calls `ICustomerRepository.UpdateAsync`
- **DeleteCustomerCommand** → handler verifies existence, calls `ICustomerRepository.DeleteAsync`
- **GetCustomerByIdQuery** → handler calls `ICustomerRepository.GetByIdAsync`
- **GetCustomersQuery** → handler calls `ICustomerRepository.GetAllAsync`

#### 4. Application Layer — Repository Interface

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

#### 5. Application Layer — Validators

- **CreateCustomerCommandValidator**: `FirstName` (conditional per Question 1), `LastName` required + max 256, `TaxId` required + max 16, `Comments` max 1024
- **UpdateCustomerCommandValidator**: `Id` not empty + same field rules as create

#### 6. Infrastructure Layer — Entity & Configuration

```csharp
// Customer.cs (entity)
public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? Comments { get; set; }
}
```

Fluent API configuration (`CustomerEntityConfiguration`):
- `ToTable("Customers")`
- `HasKey(x => x.Id)`
- `LastName` → `.IsRequired().HasMaxLength(256)`
- `FirstName` → `.HasMaxLength(256)` (`.IsRequired()` TBD per Question 1)
- `TaxId` → `.IsRequired().HasMaxLength(16)` + `HasIndex(x => x.TaxId).IsUnique()`
- `Comments` → `.HasMaxLength(1024)`

#### 7. Infrastructure Layer — Repository

Follows the existing `ApplicationRepository` pattern exactly:
- `GetByIdAsync` → `AsNoTracking().FirstOrDefaultAsync`, returns `entity?.ToDto()`
- `GetAllAsync` → `AsNoTracking().ToListAsync`, maps each to DTO
- `AddAsync` → maps DTO to entity, adds, saves with duplicate-key catch for `TaxId`
- `UpdateAsync` → fetches entity (tracked), applies changes, saves with duplicate-key catch
- `DeleteAsync` → fetches entity, removes, saves

#### 8. Infrastructure Layer — Persistence Mapping Extensions

Internal static class `CustomerPersistenceMappingExtensions`:
- `ToDto(Customer entity)` → `CustomerDto`
- `ToEntity(CreateCustomerDto dto)` → `Customer` (generates `Guid.CreateVersion7()` for Id)
- `ApplyTo(CustomerDto dto, Customer entity)` → mutates entity properties

#### 9. Infrastructure Layer — DI Registration

In `DependencyInjection.cs`: add `services.AddScoped<ICustomerRepository, CustomerRepository>();`

#### 10. Infrastructure Layer — DbContext

In `AppDbContext.cs`: add `public DbSet<Customer> Customers => Set<Customer>();`

#### 11. API Layer — Controller

`CustomersController` follows the `ApplicationsController` pattern:
- Primary constructor: `(IMessageBus messageBus)`
- `[Route("customers")]`
- Route constraints: `{id:guid}` on GET/PUT/DELETE by-id
- `[ApiConventionMethod]` for standard status codes
- Explicit `[ProducesResponseType(StatusCodes.Status409Conflict)]` on POST and PUT
- `CreatedAtAction` for POST response
- `NoContent` for DELETE response

#### 12. API Layer — Mapping Extensions

`CustomerMappingExtensions` in API Mappers folder:
- `CreateCustomerRequest → CreateCustomerCommand`
- `UpdateCustomerRequest + Guid → UpdateCustomerCommand`
- `Guid → DeleteCustomerCommand`
- `CustomerDto → CustomerResponse`
- `IEnumerable<CustomerDto> → List<CustomerResponse>`

---

## Implementation Order

1. **Infrastructure — Entity + Configuration** (I1, I2) — Database schema foundation
2. **Application — DTOs** (A1, A2) — Data contracts
3. **Application — Repository Interface** (A8) — Abstraction
4. **Infrastructure — Persistence Mappings** (I4) — Entity ↔ DTO mapping
5. **Infrastructure — Repository** (I3) — Data access implementation
6. **Infrastructure — DbContext update** (I5) — Register `DbSet<Customer>`
7. **Infrastructure — DI update** (I6) — Register `ICustomerRepository`
8. **Application — Commands & Queries** (A3–A7) — Business logic
9. **Application — Mappings** (A11) — Command → DTO mapping
10. **Application — Validators** (A9, A10) — Input validation
11. **API — Request/Response models** (P2–P4) — API contract
12. **API — Mapping extensions** (P5) — API ↔ Application mapping
13. **API — Controller** (P1) — Endpoint exposure

**Rationale**: Bottom-up dependency order. Infrastructure entities first, then contracts (DTOs, interfaces), then implementations (repository, handlers), then validators, and finally the API surface. This ensures each layer compiles before the next depends on it.

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| AS1 | `first_name` is **mandatory** (same as `last_name`) | The omission of "Traits: mandatory" for `first_name` appears to be a documentation oversight. A customer without a first name would be unusual in a management system. Will implement as required unless user answers otherwise. |
| AS2 | The existing Wolverine mediator pattern (`IMessageBus`) is used for command/query dispatch | This matches the established `ApplicationsController` pattern in the codebase. |
| AS3 | No authentication/authorization middleware is needed at this stage | The story says "As an administrator" but no auth requirements are specified in acceptance criteria. The existing `ApplicationsController` also has no auth attributes. |
| AS4 | `Guid.CreateVersion7()` is used for ID generation | This follows the architecture rules and the existing Application entity pattern. |
| AS5 | The existing `AppDbContext` connection string `"Default"` is reused | No separate connection string is specified for Customers. |
| AS6 | Pagination is not implemented for `GET /customers` | The story does not mention pagination. Added as a question. |
| AS7 | Soft delete is not required | The story says "delete" with no qualifiers; the existing Application pattern uses hard deletes. |
| AS8 | The `Customers` table name is pluralized | Follows the existing `Applications` table naming convention. |

---

## Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Is `first_name` mandatory or optional? (Only `last_name` is marked "Traits: mandatory") | Affects FluentValidation rules and EF Core `.IsRequired()` configuration on `FirstName`. |
| Q2 | Should `GET /customers` include pagination? If so, what page size? | Affects `GetCustomersQuery`, repository method signature, and controller response format. |
| Q3 | Is `tax_id` user-supplied or system-generated? The model has no auto-generation trait. | Current plan treats it as user-supplied (required input). If auto-generated, change the approach. |
| Q4 | Are there any additional search/filter parameters expected for `GET /customers` (e.g., by name, by tax_id)? | Would require adding query parameters and repository filtering logic. |
| Q5 | Should the `tax_id` be validated against any format (e.g., regex for a specific tax ID format)? | Affects the `CreateCustomerCommandValidator` rules. |
