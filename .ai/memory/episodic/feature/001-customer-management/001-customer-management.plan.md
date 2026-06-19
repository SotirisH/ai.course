# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, the system must provide full CRUD (Create, Read, Update, Delete) and list capabilities for Customers. Each Customer has a unique identifier (GUID), first name (optional), last name (mandatory), tax ID (mandatory, unique), and optional comments. The feature exposes a RESTful API at `/customers`.

---

## Acceptance Criteria

**Given** an administrator
**When** they send a `POST /customers` with valid `last_name` and `tax_id`
**Then** a new Customer is created and returned with a `201 Created` status.

**Given** an administrator
**When** they send a `PUT /customers/{id}` with updated fields
**Then** the Customer is updated and returned with a `200 OK` status.

**Given** an administrator
**When** they send a `GET /customers/{id}` for an existing Customer
**Then** the Customer is returned with a `200 OK` status.

**Given** an administrator
**When** they send a `GET /customers`
**Then** all Customers are returned as a list with a `200 OK` status.

**Given** an administrator
**When** they send a `DELETE /customers/{id}` for an existing Customer
**Then** the Customer is deleted and `204 No Content` is returned.

**Given** a duplicate `tax_id` is submitted
**When** creating or updating a Customer
**Then** a `409 Conflict` response is returned.

**Given** a non-existent Customer ID is requested
**When** fetching, updating, or deleting
**Then** a `404 Not Found` response is returned.

---

## Spec Issues

| # | Issue | Severity |
|---|-------|----------|
| 1 | **`first_name` mandatory?** — `last_name` and `tax_id` are explicitly marked "Traits: mandatory", but `first_name` has no such trait. This could be intentional (first name is optional) or an omission. | Medium |
| 2 | **No pagination on `GET /customers`** — Returning all customers without pagination could become a performance issue at scale. | Low |
| 3 | **No search/filter on `GET /customers`** — The story provides no query parameters for filtering (by name, by tax_id). | Low |

---

## Pre-Scaffold Detection Results

| Layer | Scan Pattern | Result |
|-------|-------------|--------|
| Domain | `*customer*` | No matches |
| Application | `*customer*` | No matches |
| Infrastructure | `*customer*` | No matches |
| API | `*customer*` | No matches |

**Conclusion**: All files are new (`CREATE`). No existing customer-related code to review.

---

## File Change List

### Application Layer

| # | File | Action | Path |
|---|------|--------|------|
| A1 | `CustomerDto.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/` |
| A2 | `CreateCustomerDto.cs` | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/` |
| A3 | `CreateCustomerCommand.cs` (command + handler) | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A4 | `UpdateCustomerCommand.cs` (command + handler) | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A5 | `DeleteCustomerCommand.cs` (command + handler) | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/` |
| A6 | `GetCustomerByIdQuery.cs` (query + handler) | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/` |
| A7 | `GetCustomersQuery.cs` (query + handler) | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/` |
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

### Domain Layer

No changes required. The existing `InvalidOperationException` from the framework is used for not-found and duplicate-key scenarios (matching the ApplicationManagement pattern).

---

## Implementation Details

### Data Model (Database Table: `Customers`)

| Column | Type | Constraints |
|--------|------|-------------|
| `Id` | `Guid` | PK, generated via `Guid.CreateVersion7()` |
| `FirstName` | `string(256)` | Optional (TBD — see Question 1; implemented as optional unless clarified) |
| `LastName` | `string(256)` | Required (`IsRequired()`) |
| `TaxId` | `string(16)` | Required, **Unique Index** |
| `Comments` | `string(1024)` | Optional |

### API Contract

**`POST /customers`**
- Request: `CreateCustomerRequest` → `{ FirstName?, LastName, TaxId, Comments? }`
- Response: `201 Created` + `CustomerResponse` body
- Errors: `400` (validation), `409` (duplicate tax_id)

**`PUT /customers/{id:guid}`**
- Request: `UpdateCustomerRequest` → `{ FirstName?, LastName, TaxId, Comments? }`
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
No changes. The existing `InvalidOperationException` is used for not-found and duplicate-key scenarios (following the established `ApplicationManagement` pattern).

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

Following the established `ApplicationManagement` pattern: each command/query record is co-located with its handler in the same file.

- **CreateCustomerCommand** → handler maps command → `CreateCustomerDto`, calls `ICustomerRepository.AddAsync`
- **UpdateCustomerCommand** → handler fetches existing, applies changes via `with` expression, calls `ICustomerRepository.UpdateAsync`
- **DeleteCustomerCommand** → handler verifies existence, calls `ICustomerRepository.DeleteAsync`
- **GetCustomerByIdQuery** → handler calls `ICustomerRepository.GetByIdAsync`, throws `InvalidOperationException` if not found
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

- **CreateCustomerCommandValidator**: `FirstName` max 256, `LastName` required + max 256, `TaxId` required + max 16, `Comments` max 1024
- **UpdateCustomerCommandValidator**: `Id` not empty + same field rules as create

Follows the `CreateApplicationCommandValidator` / `UpdateApplicationCommandValidator` pattern exactly — same structure, same `WithMessage` style.

#### 6. Application Layer — Mappings

`CustomerMappingExtensions` (static class):
- `CreateCustomerCommand → CreateCustomerDto`
- `UpdateCustomerCommand.ApplyTo(CustomerDto)` → `CustomerDto` (using `with` expression)

#### 7. Infrastructure Layer — Entity

```csharp
// Customer.cs
public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(256)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(16)]
    public string TaxId { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Comments { get; set; }
}
```

Note: DataAnnotations (`[Key]`, `[MaxLength]`) are kept for consistency with the existing `Application` entity pattern. Fluent API in `CustomerEntityConfiguration` provides the definitive configuration.

#### 8. Infrastructure Layer — Entity Configuration

`CustomerEntityConfiguration` implements `IEntityTypeConfiguration<Customer>`:
- `ToTable("Customers")`
- `HasKey(x => x.Id)`
- `FirstName` → `.HasMaxLength(256)` (`.IsRequired()` TBD per Question 1 — NOT required by default based on story)
- `LastName` → `.IsRequired().HasMaxLength(256)`
- `TaxId` → `.IsRequired().HasMaxLength(16)` + `HasIndex(x => x.TaxId).IsUnique()`
- `Comments` → `.HasMaxLength(1024)`

#### 9. Infrastructure Layer — Repository

`CustomerRepository(AppDbContext dbContext)` implements `ICustomerRepository`, following the exact `ApplicationRepository` pattern:
- `GetByIdAsync` → `AsNoTracking().FirstOrDefaultAsync`, returns `entity?.ToDto()`
- `GetAllAsync` → `AsNoTracking().ToListAsync`, maps each to DTO
- `AddAsync` → maps DTO to entity, adds, saves with duplicate-key catch for `TaxId`
- `UpdateAsync` → fetches entity (tracked), applies changes via `dto.ApplyTo()`, saves with duplicate-key catch
- `DeleteAsync` → fetches entity, removes, saves
- Private `IsDuplicateKeyViolation(DbUpdateException)` helper method

#### 10. Infrastructure Layer — Persistence Mapping Extensions

Internal static class `CustomerPersistenceMappingExtensions`:
- `ToDto(Customer)` → `CustomerDto`
- `ToEntity(CreateCustomerDto)` → `Customer` (generates `Guid.CreateVersion7()` for Id)
- `ApplyTo(CustomerDto, Customer)` → mutates entity properties

#### 11. Infrastructure Layer — DbContext Update

In `AppDbContext.cs`: add `public DbSet<Customer> Customers => Set<Customer>();`

#### 12. Infrastructure Layer — DI Update

In `DependencyInjection.cs`: add `services.AddScoped<ICustomerRepository, CustomerRepository>();`

#### 13. API Layer — Request/Response Models

```csharp
// CreateCustomerRequest.cs
public sealed record CreateCustomerRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

// UpdateCustomerRequest.cs — same shape as Create
public sealed record UpdateCustomerRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

// CustomerResponse.cs
public sealed record CustomerResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

#### 14. API Layer — Mappings

`CustomerMappingExtensions` (static class) in `src/Ai.Api/Mappers/`:
- `CreateCustomerRequest → CreateCustomerCommand`
- `UpdateCustomerRequest + Guid → UpdateCustomerCommand`
- `Guid → DeleteCustomerCommand`
- `CustomerDto → CustomerResponse`
- `IEnumerable<CustomerDto> → List<CustomerResponse>`

#### 15. API Layer — Controller

`CustomersController(IMessageBus messageBus) : ControllerBase`:
- `[Route("customers")]`
- `[HttpPost]` → `CreateCustomerCommand`, `CreatedAtAction(nameof(GetById), ...)`
- `[HttpGet]` → `GetCustomersQuery`
- `[HttpGet("{id:guid}")]` → `GetCustomerByIdQuery`
- `[HttpPut("{id:guid}")]` → `UpdateCustomerCommand`
- `[HttpDelete("{id:guid}")]` → `DeleteCustomerCommand`, `NoContent()`
- `[ProducesResponseType(StatusCodes.Status409Conflict)]` on POST and PUT
- `[ApiConventionMethod]` for standard status codes
- Error handling via Wolverine's built-in exception handling (the existing pattern does not use try-catch in controllers)

---

## Implementation Order

| Step | Files | Layer | Rationale |
|------|-------|-------|-----------|
| 1 | I1, I2 | Infrastructure | Database schema foundation — entity and configuration |
| 2 | A1, A2 | Application | Data contracts — DTOs |
| 3 | A8 | Application | Repository interface — abstraction |
| 4 | I4 | Infrastructure | Entity ↔ DTO mapping extensions |
| 5 | I3 | Infrastructure | Repository implementation |
| 6 | I5 | Infrastructure | Register `DbSet<Customer>` in DbContext |
| 7 | I6 | Infrastructure | Register `ICustomerRepository` in DI |
| 8 | A3, A4, A5, A6, A7 | Application | Commands & Queries (Wolverine handlers) |
| 9 | A11 | Application | Command → DTO mapping extensions |
| 10 | A9, A10 | Application | FluentValidation validators |
| 11 | P2, P3, P4 | API | API request/response models |
| 12 | P5 | API | API ↔ Application mapping extensions |
| 13 | P1 | API | Controller |

**Rationale**: Bottom-up dependency order — Infrastructure entities first, then contracts (DTOs, interfaces), then implementations (repository, handlers), then validators, and finally the API surface. This ensures each layer compiles before the next depends on it.

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| AS1 | `first_name` is **optional** (not mandatory) | The story model explicitly marks `last_name` and `tax_id` with "Traits: mandatory" but omits it for `first_name`. This is treated as intentional — a customer may have only a last name. If incorrect, this is a one-line change. |
| AS2 | The existing Wolverine mediator pattern (`IMessageBus`) is used for command/query dispatch | Matches the established `ApplicationsController` pattern. |
| AS3 | No authentication/authorization middleware is needed at this stage | The story says "As an administrator" but no auth requirements are specified in acceptance criteria. The existing `ApplicationsController` also has no auth attributes. |
| AS4 | `Guid.CreateVersion7()` is used for ID generation | Follows architecture rules and the existing `Application` entity pattern. |
| AS5 | The existing `AppDbContext` connection string `"Default"` is reused | No separate connection string is specified for Customers. |
| AS6 | Pagination is not implemented for `GET /customers` | The story does not mention pagination. Added as a question. |
| AS7 | Hard delete is used (not soft delete) | The story says "delete" with no qualifiers; the existing `Application` pattern uses hard deletes. |
| AS8 | The `Customers` table name is pluralized | Follows the existing `Applications` table naming convention. |
| AS9 | `tax_id` is user-supplied (not auto-generated) | The model defining `tax_id` with "Traits: mandatory" implies it's an input field, not system-generated. |
| AS10 | Duplicate `tax_id` detection relies on PostgreSQL unique index + `DbUpdateException` catch pattern | Matches the `ApplicationRepository` pattern for unique constraint violations. |
| AS11 | The existing error handling pattern (throwing `InvalidOperationException` from handlers, letting Wolverine/ASP.NET middleware convert to HTTP responses) is sufficient | Matches the `ApplicationsController` pattern — no try-catch in controllers. |

---

## Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Is `first_name` mandatory or optional? Only `last_name` and `tax_id` are marked "Traits: mandatory". | Affects FluentValidation rules and EF Core `.IsRequired()` configuration on `FirstName`. Currently planned as optional. |
| Q2 | Should `GET /customers` include pagination? If so, what page size and page parameter names? | Affects `GetCustomersQuery`, repository method signature, and controller response format. |
| Q3 | Should `GET /customers` support search/filter parameters (e.g., by `tax_id`, by `last_name`)? | Would require adding query parameters and repository filtering logic. |
| Q4 | Should `tax_id` be validated against a specific format (e.g., regex for a tax ID format)? | Affects the `CreateCustomerCommandValidator` and `UpdateCustomerCommandValidator` rules. |
| Q5 | Is there any need for batch operations (bulk create/delete)? | Would require additional commands/queries beyond the current CRUD surface. |
