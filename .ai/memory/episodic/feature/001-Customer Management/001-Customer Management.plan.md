# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. The system should allow administrators to create, update, retrieve, and list Customers. Each Customer has a unique identifier.

---

## Acceptance Criteria (Given-When-Then)

| # | Given | When | Then |
|---|-------|------|------|
| AC1 | A valid customer payload is provided | POST `/customers` is called | A new customer is created and `201 Created` is returned with the customer details |
| AC2 | A valid customer payload is provided | PUT `/customers/{id}` is called | The customer is updated and `200 OK` is returned with updated details |
| AC3 | An existing customer ID is provided | GET `/customers/{id}` is called | `200 OK` is returned with the customer details |
| AC4 | No customer exists for the given ID | GET `/customers/{id}` is called | `404 Not Found` is returned |
| AC5 | Customers exist | GET `/customers` is called | `200 OK` is returned with the list of all customers |
| AC6 | A create/update payload with missing mandatory fields (last_name, tax_id) | POST or PUT is called | `400 Bad Request` is returned with validation errors |
| AC7 | A create/update payload with duplicate tax_id | POST or PUT is called | `409 Conflict` is returned |

---

## Customer Model

| Field | Type | Constraints |
|-------|------|-------------|
| id | guid | Primary key |
| first_name | string(256) | Optional |
| last_name | string(256) | **Mandatory** |
| tax_id | string(16) | **Mandatory, Unique** |
| comments | string(1024) | Optional |

---

## API Endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/customers` | Create customer |
| PUT | `/customers/{id}` | Update customer |
| GET | `/customers/{id}` | Get customer by ID |
| GET | `/customers` | List all customers |

---

## Spec Issues

1. **No DELETE endpoint**: The Applications feature includes a DELETE endpoint, but the Customer Management story only defines Create, Update, GetById, and GetAll. No DELETE is listed. This may be intentional or an omission.
2. **No duplicate-check acceptance criterion for tax_id**: The model defines tax_id as "unique," but there is no explicit acceptance criterion covering a duplicate tax_id conflict scenario. The plan includes AC7 as an inferred criterion based on the model constraint.

---

## File Change List

### Domain Layer — No changes required

The existing `DomainException` and `ExceptionHandlingMiddleware` already handle `InvalidOperationException` for "was not found" (→ 404) and "already exists" (→ 409) scenarios. No new domain exceptions are needed.

---

### Application Layer

| # | File | Action | Purpose |
|---|------|--------|---------|
| A1 | `Features/CustomerManagement/DTOs/CustomerDto.cs` | **Create** | Read DTO returned from Infrastructure |
| A2 | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | **Create** | DTO for passing create data to Infrastructure |
| A3 | `Features/CustomerManagement/Commands/CreateCustomerCommandHandler.cs` | **Create** | Create command + handler |
| A4 | `Features/CustomerManagement/Commands/UpdateCustomerCommandHandler.cs` | **Create** | Update command + handler |
| A5 | `Features/CustomerManagement/Queries/GetCustomerByIdQueryHandler.cs` | **Create** | Get-by-ID query + handler |
| A6 | `Features/CustomerManagement/Queries/GetCustomersQueryHandler.cs` | **Create** | Get-all query + handler |
| A7 | `Interfaces/Repositories/ICustomerRepository.cs` | **Create** | Repository interface |
| A8 | `Validators/CreateCustomerCommandValidator.cs` | **Create** | FluentValidation validator for create |
| A9 | `Validators/UpdateCustomerCommandValidator.cs` | **Create** | FluentValidation validator for update |
| A10 | `Mappings/CustomerMappingExtensions.cs` | **Create** | Command → DTO mapping extensions |
| A11 | `GlobalUsings.cs` | **Update** | Add global using for CustomerManagement DTOs |

---

### Infrastructure Layer

| # | File | Action | Purpose |
|---|------|--------|---------|
| I1 | `Persistence/Entities/Customers.cs` | **Create** | EF entity (database table representation) |
| I2 | `Persistence/Configurations/CustomerEntityConfiguration.cs` | **Create** | EF configuration (keys, indexes, constraints) |
| I3 | `Persistence/Repositories/CustomerRepository.cs` | **Create** | Repository implementation |
| I4 | `Persistence/CustomerPersistenceMappingExtensions.cs` | **Create** | Entity ↔ DTO mapping extensions |
| I5 | `Persistence/Context/AppDbContext.cs` | **Update** | Add `DbSet<Customers>` property |
| I6 | `DependencyInjection.cs` | **Update** | Register `ICustomerRepository` and its implementation |

---

### API Layer

| # | File | Action | Purpose |
|---|------|--------|---------|
| P1 | `Models/Requests/CreateCustomerRequest.cs` | **Create** | API request model for create |
| P2 | `Models/Requests/UpdateCustomerRequest.cs` | **Create** | API request model for update |
| P3 | `Models/Responses/CustomerResponse.cs` | **Create** | API response model |
| P4 | `Controllers/CustomersController.cs` | **Create** | API controller with CRUD endpoints |
| P5 | `Mappers/CustomerMappingExtensions.cs` | **Create** | Request → Command, DTO → Response mapping |
| P6 | `GlobalUsings.cs` | **Update** | Add global using for `CustomerResponse` (already has `Ai.Api.Models.Responses`) |

---

## Implementation Details

### DTOs (Application Layer)

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

### Commands & Queries (Application Layer)

**CreateCustomerCommand** — contains `FirstName`, `LastName`, `TaxId`, `Comments`. Handler maps to `CreateCustomerDto`, calls `ICustomerRepository.AddAsync`, returns `CustomerDto`.

**UpdateCustomerCommand** — contains `Id`, `FirstName`, `LastName`, `TaxId`, `Comments`. Handler fetches existing via `GetByIdAsync`, throws `InvalidOperationException` if not found, applies update via an extension method, calls `ICustomerRepository.UpdateAsync`, returns `CustomerDto`.

**GetCustomerByIdQuery** — contains `Id`. Handler calls `ICustomerRepository.GetByIdAsync`, throws `InvalidOperationException` if not found, returns `CustomerDto`.

**GetCustomersQuery** — empty record. Handler calls `ICustomerRepository.GetAllAsync`, returns `IReadOnlyList<CustomerDto>`.

### Validators (Application Layer)

**CreateCustomerCommandValidator:**
- `LastName`: `NotEmpty()` + `MaximumLength(256)`
- `TaxId`: `NotEmpty()` + `MaximumLength(16)`
- `FirstName`: `MaximumLength(256)`
- `Comments`: `MaximumLength(1024)`

**UpdateCustomerCommandValidator:**
- `Id`: `NotEmpty()`
- `LastName`: `NotEmpty()` + `MaximumLength(256)`
- `TaxId`: `NotEmpty()` + `MaximumLength(16)`
- `FirstName`: `MaximumLength(256)`
- `Comments`: `MaximumLength(1024)`

### Repository Interface (Application Layer)

```csharp
public interface ICustomerRepository
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> AddAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(CustomerDto dto, CancellationToken cancellationToken = default);
}
```

### EF Entity (Infrastructure Layer)

```csharp
public class Customers
{
    [Key] public Guid Id { get; set; }
    [MaxLength(256)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(256)] public string LastName { get; set; } = string.Empty;
    [MaxLength(16)] public string TaxId { get; set; } = string.Empty;
    [MaxLength(1024)] public string? Comments { get; set; }
}
```

### Entity Configuration (Infrastructure Layer)

- Table name: `Customers`
- Primary key on `Id`
- `LastName`: `IsRequired()`, `HasMaxLength(256)`
- `TaxId`: `IsRequired()`, `HasMaxLength(16)`, unique index
- `FirstName`: `HasMaxLength(256)`
- `Comments`: `HasMaxLength(1024)`

### Repository (Infrastructure Layer)

`CustomerRepository` follows the same pattern as `ApplicationRepository`:
- `AddAsync`: Maps `CreateCustomerDto` → entity, adds, catches `DbUpdateException` for duplicate tax_id with message "A customer with the tax ID '{TaxId}' already exists."
- `UpdateAsync`: Loads entity tracking, applies DTO changes, catches duplicate key for tax_id.
- `GetByIdAsync`: `AsNoTracking()`, `FirstOrDefaultAsync`, maps to DTO.
- `GetAllAsync`: `AsNoTracking()`, `ToListAsync`, maps to DTO list.
- `IsDuplicateKeyViolation`: Same helper pattern.

### API Controller (API Layer)

**`CustomersController`** — follows `ApplicationsController` pattern:
- `POST /customers` → `Create` → returns `CreatedAtAction(nameof(GetById), ..., response)`
- `PUT /customers/{id:guid}` → `Update` → returns `Ok(response)`
- `GET /customers/{id:guid}` → `GetById` → returns `Ok(response)`
- `GET /customers` → `GetAll` → returns `Ok(responseList)`

Uses `IMessageBus` (Wolverine mediator), `[ApiConventionMethod]`, explicit `[FromBody]`/`[FromRoute]`, `CancellationToken`.

### Mapping Extensions

**Application Layer Mappings** (`Application/Mappings/CustomerMappingExtensions.cs`):
- `CreateCustomerCommand → CreateCustomerDto`
- `UpdateCustomerCommand.ApplyTo(CustomerDto)` → returns updated `CustomerDto` using `with` expression

**Infrastructure Layer Mappings** (`Persistence/CustomerPersistenceMappingExtensions.cs`):
- `CreateCustomerDto → Customers` (entity, using `Guid.CreateVersion7()`)
- `Customers → CustomerDto`
- `CustomerDto.ApplyTo(Customers)` (mutates entity in place)

**API Layer Mappings** (`Mappers/CustomerMappingExtensions.cs`):
- `CreateCustomerRequest → CreateCustomerCommand`
- `UpdateCustomerRequest → UpdateCustomerCommand` (accepts `Guid id`)
- `CustomerDto → CustomerResponse`
- `IEnumerable<CustomerDto> → List<CustomerResponse>`

### DI Registration

- **Infrastructure `DependencyInjection.cs`**: Add `services.AddScoped<ICustomerRepository, CustomerRepository>();`
- **Application `DependencyInjection.cs`**: No changes needed — Wolverine handler discovery is assembly-based and will auto-discover Customer handlers.
- **`AppDbContext.cs`**: Add `public DbSet<Entities.Customers> Customers => Set<Entities.Customers>();`

### GlobalUsings Update

- **Application `GlobalUsings.cs`**: Add `global using Ai.Api.Application.Features.CustomerManagement.DTOs;`
  - Note: `Ai.Api.Application.Features.CustomerManagement.Commands` and `Ai.Api.Application.Interfaces.Repositories` are already covered since they share namespace patterns with the existing globals — but since Customer commands are in a different feature namespace, the global needs to be added. Actually, looking more closely at the existing GlobalUsings:
    - `global using Ai.Api.Application.Features.ApplicationManagement.Commands;` — only ApplicationManagement commands
    - Commands for CustomerManagement will be in `Ai.Api.Application.Features.CustomerManagement.Commands` — this needs a new global using

---

## Implementation Order

1. **DTOs** (A1, A2) — Foundation types, no dependencies
2. **Repository Interface** (A7) — Defines contract
3. **Application Mappings** (A10) — Maps commands to DTOs
4. **Commands & Queries** (A3–A6) — Depend on DTOs, interface, and mappings
5. **Validators** (A8, A9) — Depend on commands
6. **GlobalUsings update** (A11) — Add CustomerManagement.DTOs
7. **EF Entity** (I1) — Database representation
8. **Entity Configuration** (I2) — Depend on entity
9. **DbContext update** (I5) — Depend on entity
10. **Infrastructure Mappings** (I4) — Depend on entity and DTOs
11. **Repository Implementation** (I3) — Depend on entity, DbContext, mappings, interface
12. **DI Registration update** (I6) — Register repository
13. **API Request/Response models** (P1–P3) — API contract types
14. **API Mappings** (P5) — Depend on commands, DTOs, API models
15. **Controller** (P4) — Depend on everything above
16. **GlobalUsings update** (P6) — Already covered by existing `global using Ai.Api.Models.Responses;`

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| AS1 | No DELETE endpoint is required | The story explicitly lists POST, PUT, GET/{id}, GET but not DELETE. The Applications feature has DELETE — this may be an intentional omission for Customers or an oversight. |
| AS2 | `tax_id` uniqueness is enforced at the database level via a unique index | The model defines tax_id as "unique." Following the Applications pattern (unique index on Name in EF configuration), the same approach is used. |
| AS3 | `tax_id` duplicate violation returns 409 Conflict | Following the existing middleware pattern, `InvalidOperationException` with "already exists" maps to 409. The repository catches `DbUpdateException` for duplicate key violations and wraps it in `InvalidOperationException`. |
| AS4 | `last_name` is mandatory but `first_name` is optional | The model traits say "mandatory" only for last_name and tax_id. |
| AS5 | Entity class name is `Customers` (plural, matching table name) | Per architecture rules: "Entity classes: It should match the name of the database table." The table is named `Customers`. |
| AS6 | `Guid.CreateVersion7()` is used for ID generation | Per architecture rules (DTO Design section) and the existing Applications pattern. |
| AS7 | No domain events are needed for Customer CRUD | The story doesn't mention any side effects (notifications, logging, etc.). Can be added later if needed. |
| AS8 | The controller is named `CustomersController` (plural) | Follows ASP.NET convention and the existing `ApplicationsController` pattern. |
| AS9 | The feature folder is named `CustomerManagement` (singular "Customer") | Follows the existing pattern: `ApplicationManagement` (singular "Application"). |

---

## Open Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should a DELETE `/customers/{id}` endpoint be included? | If yes, adds 1 command + handler, 1 controller action, and 1 repository method (already exists in `ICustomerRepository` pattern from `IApplicationRepository`). Currently excluded per ACs. |
| Q2 | Should `tax_id` be validated with a specific format (e.g., regex for tax ID patterns)? | The story doesn't specify a format. Currently only length (max 16) and non-empty are enforced. |
| Q3 | Should `GET /customers` support pagination or filtering? | The story doesn't mention pagination. For now, it returns all customers. This could be a problem at scale. |
| Q4 | Are there any authorization requirements (e.g., admin role) for these endpoints? | The story says "as an administrator" but doesn't specify authorization. Not addressed in this plan. |
| Q5 | Should there be a unique constraint on `last_name` + `first_name` combination? | Not specified in the model. Only `tax_id` is marked unique. Not included. |
