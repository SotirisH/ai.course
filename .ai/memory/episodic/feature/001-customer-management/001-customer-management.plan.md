# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to manage customers in the system. The feature provides full CRUD for `Customers` via REST endpoints. Each customer has a GUID `id`, mandatory `last_name` and `tax_id` (tax_id must be unique), optional `first_name` and `comments`.

---

## Acceptance Criteria (Given-When-Then)

| # | Scenario | Given | When | Then |
|---|----------|-------|------|------|
| AC1 | Create a customer | Valid customer payload | POST `/customers` | 201 Created with customer resource, Location header set |
| AC2 | Create duplicate tax_id | A customer with tax_id "TAX123" exists | POST `/customers` with tax_id "TAX123" | 409 Conflict |
| AC3 | Create missing last_name | No last_name in payload | POST `/customers` | 400 Bad Request (validation) |
| AC4 | Create missing tax_id | No tax_id in payload | POST `/customers` | 400 Bad Request (validation) |
| AC5 | Retrieve single customer | Customer with id=X exists | GET `/customers/{id}` | 200 OK with customer resource |
| AC6 | Retrieve missing customer | No customer with id=Y | GET `/customers/{id}` | 404 Not Found |
| AC7 | List all customers | Multiple customers exist | GET `/customers` | 200 OK with list of customers |
| AC8 | Update a customer | Customer with id=X exists | PUT `/customers/{id}` with valid payload | 200 OK with updated customer |
| AC9 | Update with duplicate tax_id | Customer A and B exist; B has tax_id "TAX456" | PUT `/customers/{A.id}` with tax_id "TAX456" | 409 Conflict |
| AC10 | Update missing customer | No customer with id=Y | PUT `/customers/{id}` | 404 Not Found |
| AC11 | Delete a customer | Customer with id=X exists | DELETE `/customers/{id}` | 204 No Content |
| AC12 | Delete missing customer | No customer with id=Y | DELETE `/customers/{id}` | 404 Not Found |

---

## Spec Consistency Check

### Mismatches & Issues Flagged

| # | Issue | Detail |
|---|-------|--------|
| S1 | **File name / ticket mismatch** | Work item file is `docs/002_customers.story.md` but `ticket_num` is `001`. Branch already exists as `feature/001-customer-management`. Proceed using ticket `001`. |
| S2 | **`first_name` mandatory trait missing** | Model marks `last_name` as "mandatory" and `tax_id` as "mandatory, unique", but `first_name` has no trait. Is `first_name` intentionally optional, or was "mandatory" omitted? |
| S3 | **Model uses snake_case** | Story model uses `first_name`, `last_name`, `tax_id`. Code uses PascalCase. JSON serialization will use camelCase. No conflict — just noting the convention translation. |
| S4 | **No response model defined in story** | Story only defines the persistence model. API response shapes (which fields to expose) are left to the implementer. |

---

## File Change List

> **Pre-scaffold scan result**: No existing customer-related files found in any layer. All files are new.

### Domain Layer (`Ai.Api.Domain`)

| # | File | Action |
|---|------|--------|
| D1 | `Exceptions/CustomerNotFoundException.cs` | 🟢 CREATE |

### Application Layer (`Ai.Api.Application`)

| # | File | Action |
|---|------|--------|
| A1 | `Features/CustomerManagement/DTOs/CustomerDto.cs` | 🟢 CREATE |
| A2 | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | 🟢 CREATE |
| A3 | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | 🟢 CREATE |
| A4 | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | 🟢 CREATE |
| A5 | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | 🟢 CREATE |
| A6 | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | 🟢 CREATE |
| A7 | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | 🟢 CREATE |
| A8 | `Interfaces/Repositories/ICustomerRepository.cs` | 🟢 CREATE |
| A9 | `Mappings/CustomerMappingExtensions.cs` | 🟢 CREATE |
| A10 | `Validators/CreateCustomerCommandValidator.cs` | 🟢 CREATE |
| A11 | `Validators/UpdateCustomerCommandValidator.cs` | 🟢 CREATE |

### Infrastructure Layer (`Ai.Api.Infrastructure`)

| # | File | Action |
|---|------|--------|
| I1 | `Persistence/Entities/Customer.cs` | 🟢 CREATE |
| I2 | `Persistence/Configurations/CustomerEntityConfiguration.cs` | 🟢 CREATE |
| I3 | `Persistence/Repositories/CustomerRepository.cs` | 🟢 CREATE |
| I4 | `Persistence/CustomerPersistenceMappingExtensions.cs` | 🟢 CREATE |

### Infrastructure — Existing File Modification

| # | File | Action |
|---|------|--------|
| I5 | `Persistence/Context/AppDbContext.cs` | 🔵 MODIFY — add `DbSet<Customer> Customers` property |
| I6 | `DependencyInjection.cs` | 🔵 MODIFY — register `ICustomerRepository` |

### API/Presentation Layer (`Ai.Api`)

| # | File | Action |
|---|------|--------|
| P1 | `Controllers/CustomersController.cs` | 🟢 CREATE |
| P2 | `Models/Requests/CreateCustomerRequest.cs` | 🟢 CREATE |
| P3 | `Models/Requests/UpdateCustomerRequest.cs` | 🟢 CREATE |
| P4 | `Models/Responses/CustomerResponse.cs` | 🟢 CREATE |
| P5 | `Mappers/CustomerMappingExtensions.cs` | 🟢 CREATE |

---

## Implementation Details

### Data Model (Persistence Entity)

```
Customer (table: Customers)
├── Id          : Guid (PK, Guid.CreateVersion7)
├── FirstName   : string(256), optional (default "")
├── LastName    : string(256), mandatory
├── TaxId       : string(16), mandatory, unique index
└── Comments    : string(1024), optional (nullable)
```

### API Contract

| Method | Route | Request Body | Response | Status Codes |
|--------|-------|-------------|----------|-------------|
| POST | `/customers` | `CreateCustomerRequest` | `CustomerResponse` | 201, 400, 409 |
| GET | `/customers/{id:guid}` | — | `CustomerResponse` | 200, 404 |
| GET | `/customers` | — | `IReadOnlyList<CustomerResponse>` | 200 |
| PUT | `/customers/{id:guid}` | `UpdateCustomerRequest` | `CustomerResponse` | 200, 400, 404, 409 |
| DELETE | `/customers/{id:guid}` | — | — | 204, 404 |

### Request/Response Models (API Layer)

```csharp
// CreateCustomerRequest
FirstName : string (optional, default "")
LastName  : string (required via FluentValidation)
TaxId     : string (required via FluentValidation, max 16)
Comments  : string? (optional)

// UpdateCustomerRequest (same shape as Create)
FirstName : string (optional, default "")
LastName  : string (required via FluentValidation)
TaxId     : string (required via FluentValidation, max 16)
Comments  : string? (optional)

// CustomerResponse
Id        : Guid
FirstName : string
LastName  : string
TaxId     : string
Comments  : string?
```

### DTOs (Application Layer)

```csharp
// CustomerDto (read/return)
Id, FirstName, LastName, TaxId, Comments

// CreateCustomerDto (write input)
FirstName, LastName, TaxId, Comments
```

### Command/Query Flow (Wolverine Mediator via IMessageBus)

```
POST /customers
  → CreateCustomerRequest
    → CreateCustomerCommand
      → CreateCustomerDto → ICustomerRepository.AddAsync()
        → Customer (entity) → save → CustomerDto
          → CustomerResponse

GET /customers/{id}
  → GetCustomerByIdQuery
    → ICustomerRepository.GetByIdAsync()
      → CustomerDto → CustomerResponse

GET /customers
  → GetCustomersQuery
    → ICustomerRepository.GetAllAsync()
      → IReadOnlyList<CustomerDto> → List<CustomerResponse>

PUT /customers/{id}
  → UpdateCustomerRequest
    → UpdateCustomerCommand
      → ICustomerRepository.GetByIdAsync() → ICustomerRepository.UpdateAsync()
        → CustomerDto → CustomerResponse

DELETE /customers/{id}
  → DeleteCustomerCommand
    → ICustomerRepository.GetByIdAsync() → ICustomerRepository.DeleteAsync()
      → 204 No Content (or 404)
```

### Validation Rules

| Field | Command | Rule |
|-------|---------|------|
| `LastName` | Create, Update | Not empty; max 256 chars |
| `TaxId` | Create, Update | Not empty; max 16 chars |
| `FirstName` | Create, Update | Max 256 chars (if provided) |
| `Comments` | Create, Update | Max 1024 chars (if provided) |
| `Id` | Update | Not empty (GUID required) |

### Error Handling Strategy

Following the **exact same pattern** as the existing `ApplicationManagement` feature:

- **404 Not Found**: Handlers throw `InvalidOperationException` with message containing `"was not found"`. The existing `ExceptionHandlingMiddleware` already maps this pattern to HTTP 404.
- **409 Conflict (duplicate tax_id)**: Repository catches `DbUpdateException` in `IsDuplicateKeyViolation()` helper and re-throws as `InvalidOperationException` with message containing `"already exists"`. Middleware maps this to HTTP 409.
- **400 Bad Request**: FluentValidation failures handled by Wolverine's `.UseFluentValidation()` middleware, which throws `FluentValidation.ValidationException`. Middleware maps this to HTTP 400.
- **Domain Exception**: `CustomerNotFoundException` extends `DomainException`. Middleware maps `DomainException` to HTTP 400 by default. However, the primary 404 mechanism will use `InvalidOperationException` per the existing pattern — the custom exception is included as a domain-level artifact but is not wired into the middleware for routing. If a dedicated 404 mapping is desired, the middleware would need a new case.

### Duplicate Detection (TaxId Uniqueness)

- `CustomerEntityConfiguration` creates a unique index on `TaxId` (same pattern as `Application.Name` unique index).
- Repository's `IsDuplicateKeyViolation()` helper detects PostgreSQL "duplicate key" violations.
- Re-throws as `InvalidOperationException` with `"A customer with the tax ID '{TaxId}' already exists."`

### Mapping Extension Locations

| Layer | File | Purpose |
|-------|------|---------|
| API `Mappers/` | `CustomerMappingExtensions.cs` | Request → Command, DTO → Response |
| Application `Mappings/` | `CustomerMappingExtensions.cs` | Command → DTO, query result mapping |
| Infrastructure `Persistence/` | `CustomerPersistenceMappingExtensions.cs` | DTO ↔ Entity, Entity updates |

---

## Implementation Order

| Step | Layer | File(s) | Depends On |
|------|-------|---------|------------|
| 1 | Domain | `CustomerNotFoundException.cs` | — |
| 2 | Application | `CustomerDto.cs`, `CreateCustomerDto.cs` | Step 1 |
| 3 | Application | `ICustomerRepository.cs` | Step 2 |
| 4 | Application | Command/Query files (A3–A7) | Steps 2, 3 |
| 5 | Application | `CustomerMappingExtensions.cs` (Mappings/) | Steps 2, 4 |
| 6 | Application | Validators (A10, A11) | Step 4 |
| 7 | Infrastructure | `Customer.cs` (Entity) | Step 2 |
| 8 | Infrastructure | `CustomerEntityConfiguration.cs` | Step 7 |
| 9 | Infrastructure | `CustomerPersistenceMappingExtensions.cs` | Steps 2, 7 |
| 10 | Infrastructure | `CustomerRepository.cs` | Steps 3, 7, 8, 9 |
| 11 | Infrastructure | Modify `AppDbContext.cs` — add `DbSet<Customer>` | Step 7 |
| 12 | Infrastructure | Modify `DependencyInjection.cs` — register `ICustomerRepository` | Step 10 |
| 13 | API | `CreateCustomerRequest.cs`, `UpdateCustomerRequest.cs`, `CustomerResponse.cs` | — |
| 14 | API | `CustomerMappingExtensions.cs` (Mappers/) | Steps 2, 4, 13 |
| 15 | API | `CustomersController.cs` | All above |

Steps within the same dependency level can be batched. Infrastructure depends on Application interfaces/DTOs. API depends on all lower layers.

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| AS1 | `first_name` is optional | Story model marks only `last_name` and `tax_id` as mandatory. `first_name` lacks this trait. `CreateCustomerRequest.FirstName` defaults to empty string; validator does not require it. |
| AS2 | `tax_id` uniqueness enforced at database level via unique index | Matches the existing `Application.Name` unique index pattern in `ApplicationEntityConfiguration`. |
| AS3 | `tax_id` format is free-form text (max 16 chars, no regex) | Story specifies only `string(16)`. No format constraint given. |
| AS4 | `InvalidOperationException` used for both "not found" (→404) and "already exists" (→409) | Follows the existing `ApplicationRepository` and handler pattern. The `ExceptionHandlingMiddleware` already pattern-matches on these message substrings. |
| AS5 | `CustomerNotFoundException` (extending `DomainException`) is created as a domain artifact but not wired into middleware | Keeps the domain layer expressive while maintaining backward compatibility with existing error mapping. The middleware maps `DomainException` → 400 by default; if 404 is desired, middleware needs updating (see Q3). |
| AS6 | Route constraints use `{id:guid}` | Matches existing `ApplicationsController` pattern. |
| AS7 | `CustomersController` uses plural route prefix `/customers` and plural class name | Follows ASP.NET conventions and existing `ApplicationsController` pattern. |
| AS8 | Wolverine's `.UseFluentValidation()` middleware is already configured and handles validation automatically | Confirmed in `Application/DependencyInjection.cs`. |
| AS9 | `Customer` entity table name is `Customers` (plural) | Matches EF Core convention and the story's model naming. |
| AS10 | `AppDbContext` uses `ApplyConfigurationsFromAssembly` for auto-discovery | Confirmed — `CustomerEntityConfiguration` will be discovered automatically. |
| AS11 | `IMessageBus.InvokeAsync<T>` is used in the controller (not direct handler invocation) | Matches `ApplicationsController` pattern. |
| AS12 | Mapping extensions use `Mappings/` folder in Application layer, `Mappers/` folder in API layer | Follows exact folder naming from the existing codebase. |
| AS13 | Entity uses both DataAnnotations and Fluent API configuration | Matches existing `Application` entity pattern which has `[Key]` and `[MaxLength]` attributes plus `IEntityTypeConfiguration`. |

---

## Open Questions

| # | Question | Context |
|---|----------|---------|
| Q1 | Is `first_name` intentionally optional, or should it also be mandatory? | Story only marks `last_name` and `tax_id` as mandatory. The plan assumes optional. |
| Q2 | Should `tax_id` have a format validation (e.g., regex for tax ID patterns)? | Story only specifies `string(16)`. No format constraint given. |
| Q3 | Should `CustomerNotFoundException` be wired into the `ExceptionHandlingMiddleware` as a dedicated 404 case, or stick with `InvalidOperationException` like the Application feature? | The middleware currently maps `DomainException` → 400. Using a custom exception is semantically cleaner but requires a middleware update. The existing Application feature uses `InvalidOperationException`. |
| Q4 | Should `GET /customers` support pagination, filtering, or sorting? | Story lists only a bare `GET /customers`. No query parameters mentioned. |
| Q5 | The story file is named `002_customers.story.md` but ticket_num is `001` — which is correct? | The branch already uses `001`. Should the file be renamed to `001_customers.story.md` for consistency? |

---

## Completion Checklist

- [ ] 1 Domain file created (`CustomerNotFoundException`)
- [ ] 11 Application files created (2 DTOs, 5 Commands/Queries, 1 Repository interface, 1 Mappings, 2 Validators)
- [ ] 4 Infrastructure files created (Entity, Configuration, Repository, Persistence Mappings)
- [ ] 2 Infrastructure files modified (`AppDbContext.cs`, `DependencyInjection.cs`)
- [ ] 5 API files created (Controller, 2 Requests, 1 Response, 1 Mappers)
- [ ] All 5 CRUD endpoints functional per acceptance criteria (AC1–AC12)
- [ ] TaxId uniqueness enforced at database level via unique index
- [ ] Validation errors return 400 (FluentValidation via Wolverine middleware)
- [ ] Duplicate TaxId returns 409 (via `InvalidOperationException` pattern)
- [ ] Not-found cases return 404 (via `InvalidOperationException` pattern)
- [ ] EF Core migration generated for `Customers` table
