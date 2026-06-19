# Customer Management — Implementation Plan

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. The feature provides full CRUD operations (Create, Read, Update, Delete, List) for Customer records via a REST API.

---

## Acceptance Criteria

### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/customers` | Create a new customer |
| PUT | `/customers/{id}` | Update an existing customer |
| GET | `/customers/{id}` | Retrieve a single customer by ID |
| GET | `/customers` | List all customers |
| DELETE | `/customers/{id}` | Delete a customer |

### Customer Model

| Field | Type | Required | Unique | Max Length |
|-------|------|----------|--------|------------|
| id | GUID (PK) | auto | — | — |
| first_name | string | no | no | 256 |
| last_name | string | **yes** | no | 256 |
| tax_id | string | **yes** | **yes** | 16 |
| comments | string | no | no | 1024 |

### Given-When-Then Scenarios

**GIVEN** an administrator is authenticated
**WHEN** they send a POST to `/customers` with valid `first_name`, `last_name`, `tax_id`, and optional `comments`
**THEN** a new customer is created, persisted, and a `201 Created` response is returned with the customer resource.

**GIVEN** an existing customer with a known ID
**WHEN** the administrator sends a GET to `/customers/{id}`
**THEN** the full customer record is returned with `200 OK`.

**GIVEN** customers exist in the system
**WHEN** the administrator sends a GET to `/customers`
**THEN** a list of all customers is returned with `200 OK`.

**GIVEN** an existing customer with a known ID
**WHEN** the administrator sends a PUT to `/customers/{id}` with updated fields
**THEN** the customer record is updated and a `200 OK` is returned with the updated resource.

**GIVEN** an existing customer with a known ID
**WHEN** the administrator sends a DELETE to `/customers/{id}`
**THEN** the customer is removed and `204 No Content` is returned.

**GIVEN** a POST/PUT with a `tax_id` that already exists for another customer
**WHEN** the request is processed
**THEN** a `409 Conflict` is returned.

**GIVEN** a GET/PUT/DELETE with a non-existent ID
**WHEN** the request is processed
**THEN** a `404 Not Found` is returned.

---

## Spec Issues

| # | Issue | Severity |
|---|-------|----------|
| 1 | **Authorization gap**: Story says "administrator" but no auth mechanism or role-check is defined in acceptance criteria or model. The existing `ApplicationsController` also has no auth — suggest following the same pattern and deferring auth to a separate ticket. | 🟡 Medium |
| 2 | **`first_name` optionality**: The model does NOT mark `first_name` as mandatory (unlike `last_name`). This is treated as intentional — `first_name` will be nullable. | 🟢 Low |

---

## File Change List

### Domain Layer (`src/Ai.Api.Domain/`)

| # | File | Action | Notes |
|---|------|--------|-------|
| — | *(none)* | — | Existing `DomainException` and middleware already handle the error patterns needed. No new domain types required. |

### Application Layer (`src/Ai.Api.Application/`)

| # | File | Action | Notes |
|---|------|--------|-------|
| 1 | `Features/CustomerManagement/DTOs/CustomerDto.cs` | 🆕 CREATE | `sealed record` — Id, FirstName, LastName, TaxId, Comments |
| 2 | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | 🆕 CREATE | `sealed record` — FirstName, LastName, TaxId, Comments |
| 3 | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | 🆕 CREATE | Command + Handler co-located |
| 4 | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | 🆕 CREATE | Command + Handler co-located |
| 5 | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | 🆕 CREATE | Command + Handler co-located |
| 6 | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | 🆕 CREATE | Query + Handler co-located |
| 7 | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | 🆕 CREATE | Query + Handler co-located |
| 8 | `Interfaces/Repositories/ICustomerRepository.cs` | 🆕 CREATE | Mirror of `IApplicationRepository` pattern |
| 9 | `Validators/CreateCustomerCommandValidator.cs` | 🆕 CREATE | FluentValidation — last_name required, tax_id required+max16, first_name optional max256, comments optional max1024 |
| 10 | `Validators/UpdateCustomerCommandValidator.cs` | 🆕 CREATE | FluentValidation — id required, same field rules as create |
| 11 | `Mappings/CustomerMappingExtensions.cs` | 🆕 CREATE | Command→Dto mappings (CreateCustomerCommand→CreateCustomerDto, UpdateCustomerCommand.ApplyTo) |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)

| # | File | Action | Notes |
|---|------|--------|-------|
| 12 | `Persistence/Entities/Customer.cs` | 🆕 CREATE | EF Core entity matching model + DataAnnotations (`[Key]`, `[MaxLength]`). Note: entity filename `Customer.cs` (not `CustomerEntity.cs`) to match existing `Application.cs` pattern. |
| 13 | `Persistence/Configurations/CustomerEntityConfiguration.cs` | 🆕 CREATE | Fluent API config — table `Customers`, PK, `LastName` required, `TaxId` required + unique index, max lengths |
| 14 | `Persistence/Repositories/CustomerRepository.cs` | 🆕 CREATE | Full CRUD mirroring `ApplicationRepository`; duplicate-key detection on `TaxId` |
| 15 | `Persistence/CustomerPersistenceMappingExtensions.cs` | 🆕 CREATE | `ToDto()`, `ToEntity()`, `ApplyTo()` extension methods |
| 16 | `Persistence/Context/AppDbContext.cs` | ✏️ MODIFY | Add `DbSet<Customer> Customers` property |
| 17 | `DependencyInjection.cs` | ✏️ MODIFY | Register `ICustomerRepository` → `CustomerRepository` (Scoped) |

### API Layer (`src/Ai.Api/`)

| # | File | Action | Notes |
|---|------|--------|-------|
| 18 | `Models/Requests/CreateCustomerRequest.cs` | 🆕 CREATE | `sealed record` — FirstName, LastName, TaxId, Comments |
| 19 | `Models/Requests/UpdateCustomerRequest.cs` | 🆕 CREATE | `sealed record` — FirstName, LastName, TaxId, Comments |
| 20 | `Models/Responses/CustomerResponse.cs` | 🆕 CREATE | `sealed record` — Id, FirstName, LastName, TaxId, Comments |
| 21 | `Mappers/CustomerMappingExtensions.cs` | 🆕 CREATE | Request→Command, Dto→Response, Guid→DeleteCommand mappings |
| 22 | `Controllers/CustomersController.cs` | 🆕 CREATE | Full CRUD controller following `ApplicationsController` pattern |

---

## Implementation Details

### Naming Conventions

All names follow the existing codebase conventions observed in the `ApplicationManagement` feature.

| Concept | .NET Name | Notes |
|---------|-----------|-------|
| Table | `Customers` | Plural |
| Entity | `Customer` | Matches existing `Application` naming (no `Entity` suffix) |
| PK Column | `Id` | `Guid` via `Guid.CreateVersion7()` |
| `first_name` | `FirstName` | PascalCase property |
| `last_name` | `LastName` | PascalCase property |
| `tax_id` | `TaxId` | PascalCase property |
| `comments` | `Comments` | PascalCase property |

### Request Flow (POST example)

```
POST /customers { CreateCustomerRequest }
  → CustomersController.Create()
    → request.ToCommand()                        // API Mapper: CreateCustomerRequest → CreateCustomerCommand
    → messageBus.InvokeAsync<CustomerDto>(cmd)
      → CreateCustomerCommandHandler.Handle()
        → command.ToDto()                        // App Mapper: CreateCustomerCommand → CreateCustomerDto
        → repository.AddAsync(dto)
          → dto.ToEntity()                       // Infra Mapper: CreateCustomerDto → Customer entity
          → dbContext.Customers.AddAsync(entity)
          → dbContext.SaveChangesAsync()
          → entity.ToDto()                       // Infra Mapper: Customer entity → CustomerDto
        → return customerDto
    → dto.ToResponse()                           // API Mapper: CustomerDto → CustomerResponse
  → CreatedAtAction(…, response)
```

### Error Handling

Follow the existing middleware pattern — throw `InvalidOperationException` with messages matching the middleware's pattern detection:

| Scenario | Exception | HTTP Status |
|----------|-----------|-------------|
| Entity not found | `InvalidOperationException("Customer with ID '{id}' was not found.")` | 404 |
| Duplicate `tax_id` | Caught as `DbUpdateException` → re-thrown as `InvalidOperationException("A customer with the tax ID '{taxId}' already exists.")` | 409 |
| Validation failure | FluentValidation `ValidationException` (thrown by Wolverine pipeline) | 400 |

### Duplicate Detection

The repository's `IsDuplicateKeyViolation` helper (copied from `ApplicationRepository`) checks the inner exception for `"duplicate key"`. The unique index on `TaxId` in PostgreSQL ensures this fires correctly.

### `first_name` Optionality

Since `first_name` is not marked mandatory in the model:
- Entity property: `public string? FirstName { get; set; }` (nullable)
- DTO property: `public string? FirstName { get; init; }` (nullable)
- The `CreateCustomerCommandValidator` will NOT require `FirstName`
- The `UpdateCustomerCommandValidator` will NOT require `FirstName`
- Max length constraint still applies

---

## Implementation Order

The order respects dependency direction (inner → outer):

1. **Application DTOs** — `CustomerDto`, `CreateCustomerDto` (no dependencies)
2. **Repository Interface** — `ICustomerRepository` (depends on DTOs)
3. **Application Mappings** — `CustomerMappingExtensions` (depends on DTOs + Commands)
4. **Commands & Queries** — All 5 CQRS files (depend on DTOs + Repository interface + Mappings)
5. **Validators** — `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator` (depend on Commands)
6. **Infrastructure Entity** — `Customer.cs` (no internal deps)
7. **Infrastructure Configuration** — `CustomerEntityConfiguration` (depends on Entity)
8. **Infrastructure Persistence Mappings** — `CustomerPersistenceMappingExtensions` (depends on Entity + DTOs)
9. **Infrastructure Repository** — `CustomerRepository` (depends on Entity + Config + DTOs + Interface)
10. **Update DbContext** — Add `DbSet<Customer>` (depends on Entity)
11. **Update Infrastructure DI** — Register `ICustomerRepository` (depends on Repository)
12. **API Request/Response Models** — 3 model files (no internal deps)
13. **API Mappers** — `CustomerMappingExtensions` (depends on Models + Commands + DTOs)
14. **API Controller** — `CustomersController` (depends on everything above)

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | No authentication/authorization is implemented in this ticket. | The existing `ApplicationsController` has no auth attributes. The story mentions "administrator" but that is treated as out-of-scope for this feature — consistent with the current codebase. |
| A2 | `first_name` is intentionally optional. | The model explicitly marks `last_name` and `tax_id` as mandatory but does not mark `first_name`. This is treated as a deliberate design choice. |
| A3 | Entity class is named `Customer` (not `CustomerEntity`). | The existing entity is `Application`, not `ApplicationEntity`. Following established convention. |
| A4 | `tax_id` uniqueness is enforced at the database level via a unique index. | Following the pattern used for `Application.Name` which has `HasIndex(x => x.Name).IsUnique()`. |
| A5 | The table name is `Customers` (plural). | The existing table is `Applications`. Following EF Core pluralization convention. |
| A6 | No soft-delete is implemented. | The existing `Application` feature uses hard deletes. This follows the same pattern. |
| A7 | Wolverine handler discovery is automatic. | The existing `DependencyInjection.AddApplication()` uses `opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly)`, which auto-discovers all handlers. |
| A8 | FluentValidation middleware is already registered via `UseFluentValidation()`. | Confirmed in the existing `Application\DependencyInjection.cs`. |
| A9 | `Guid.CreateVersion7()` is used for new IDs (not DB-generated). | Following the pattern in `ApplicationPersistenceMappingExtensions.ToEntity()`. |
| A10 | No migration is generated as part of this plan. | Migrations should be generated after entity and configuration are in place, as a separate step during implementation. |

---

## Open Questions

| # | Question | Context |
|---|----------|---------|
| Q1 | Should `first_name` really be optional? The model says `string(256)` without "mandatory" trait, but it seems unusual for a person entity to lack a first name. Please confirm. | Spec Issue #2 |
| Q2 | Is there any authorization requirement (e.g., `[Authorize(Roles = "Administrator")]`) that should be included? The story mentions "administrator" but no auth is defined. | Spec Issue #1 |
| Q3 | Should there be a combined unique constraint on `first_name` + `last_name` (to prevent exact duplicate names) in addition to the unique `tax_id`? | Design decision |
| Q4 | Are there any additional fields needed for `Customer` beyond the 5 defined fields? | Scope clarification |

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| `tax_id` format not validated | Invalid data stored | Could add regex validation in the validator (e.g., alphanumeric only). Currently only length is constrained. |
| Concurrent update conflicts | Last-write-wins | Acceptable for v1. Can add row versioning later if needed. |
| No pagination on GET `/customers` | Performance with large datasets | Follow existing `Applications` pattern (no pagination). Add in separate ticket if needed. |
