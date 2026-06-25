# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. This feature implements full CRUD operations (Create, Read, Update, Delete) plus a list-all operation for customers. Each customer has a unique identifier, first name, last name (mandatory), a tax ID (mandatory, unique), and optional comments. The API exposes five endpoints under the `/customers` route.

---

## Acceptance Criteria (Given-When-Then)

### AC1 — Create Customer
- **Given** an administrator wants to add a new customer
- **When** they submit a POST request to `/customers` with valid first_name, last_name, tax_id, and optional comments
- **Then** the customer is created, persisted, and a 201 response with the created resource is returned
- **And** the response includes a unique GUID identifier

### AC2 — Update Customer
- **Given** an existing customer record
- **When** an administrator submits a PUT request to `/customers/{id}` with updated fields
- **Then** the customer record is updated and a 200 response with the updated resource is returned
- **And** if the customer does not exist, a 404 response is returned

### AC3 — Retrieve Customer by ID
- **Given** an existing customer record
- **When** an administrator submits a GET request to `/customers/{id}`
- **Then** the customer details are returned with a 200 response
- **And** if the customer does not exist, a 404 response is returned

### AC4 — List All Customers
- **Given** one or more customer records exist in the system
- **When** an administrator submits a GET request to `/customers`
- **Then** a list of all customers is returned with a 200 response

### AC5 — Delete Customer
- **Given** an existing customer record
- **When** an administrator submits a DELETE request to `/customers/{id}`
- **Then** the customer is removed from the system and a 204 No Content response is returned
- **And** if the customer does not exist, a 404 response is returned

---

## Spec Consistency Check

| Check | Status | Detail |
|-------|--------|--------|
| Model fields vs AC endpoints alignment | ✅ Pass | All 5 endpoints listed in AC cover the full CRUD+L for the `customers` model |
| Model field completeness | ✅ Pass | All fields (id, first_name, last_name, tax_id, comments) defined with types and constraints |
| `tax_id` uniqueness constraint present | ✅ Pass | `unique` trait specified in model definition |
| `last_name` mandatory constraint present | ✅ Pass | `mandatory` trait specified in model definition |
| `tax_id` mandatory constraint present | ✅ Pass | `mandatory` trait specified in model definition |
| Endpoint paths consistent | ✅ Pass | All endpoints use `/customers` base path consistently |
| Data types consistent with .NET | ✅ Pass | guid → `Guid`, string(256) → `MaxLength(256)`, string(16) → `MaxLength(16)`, string(1024) → `MaxLength(1024)` |

**Summary: The work item is internally consistent. No contradictions found between the story, acceptance criteria, and model definition.**

---

## File Change List

### Domain Layer
| # | Action | File | Notes |
|---|--------|------|-------|
| D1 | No changes needed | `src/Ai.Api.Domain/` | No domain-specific exceptions or enums required beyond existing `DomainException`. Existing `ExceptionHandlingMiddleware` handles `InvalidOperationException` → 404/409. |

### Application Layer
| # | Action | File | Notes |
|---|--------|------|-------|
| A1 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/CustomerDto.cs` | Output DTO: Id, FirstName, LastName, TaxId, Comments |
| A2 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | Input DTO for repository: FirstName, LastName, TaxId, Comments |
| A3 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | Command + Handler in same file. Handler calls `ICustomerRepository.AddAsync()` |
| A4 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | Command + Handler in same file. Handler checks existence, applies update, calls `ICustomerRepository.UpdateAsync()` |
| A5 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | Command + Handler in same file. Handler checks existence, calls `ICustomerRepository.DeleteAsync()` |
| A6 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | Query + Handler in same file. Returns `CustomerDto` or throws |
| A7 | CREATE | `src/Ai.Api.Application/Features/CustomerManagement/Queries/GetCustomersQuery.cs` | Query + Handler in same file. Returns `IReadOnlyList<CustomerDto>` |
| A8 | CREATE | `src/Ai.Api.Application/Interfaces/Repositories/ICustomerRepository.cs` | Interface: GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync |
| A9 | CREATE | `src/Ai.Api.Application/Mappings/CustomerMappingExtensions.cs` | `CreateCustomerCommand → CreateCustomerDto`, `UpdateCustomerCommand → CustomerDto` (apply) |
| A10 | CREATE | `src/Ai.Api.Application/Validators/CreateCustomerCommandValidator.cs` | Validate FirstName, LastName, TaxId required + max lengths |
| A11 | CREATE | `src/Ai.Api.Application/Validators/UpdateCustomerCommandValidator.cs` | Validate Id + same field rules as create |

### Infrastructure Layer
| # | Action | File | Notes |
|---|--------|------|-------|
| I1 | CREATE | `src/Ai.Api.Infrastructure/Persistence/Entities/Customer.cs` | DB entity: Id, FirstName, LastName, TaxId, Comments. Table name: `Customers` |
| I2 | CREATE | `src/Ai.Api.Infrastructure/Persistence/Configurations/CustomerEntityConfiguration.cs` | Fluent API: PK on Id, unique index on TaxId, IsRequired on LastName and TaxId, MaxLength constraints |
| I3 | CREATE | `src/Ai.Api.Infrastructure/Persistence/CustomerPersistenceMappingExtensions.cs` | `CustomerDto ← Customer`, `CreateCustomerDto → Customer`, `CustomerDto → Customer` (apply) |
| I4 | CREATE | `src/Ai.Api.Infrastructure/Persistence/Repositories/CustomerRepository.cs` | Full CRUD implementation following `ApplicationRepository` pattern with duplicate key detection |
| I5 | EDIT | `src/Ai.Api.Infrastructure/Persistence/Context/AppDbContext.cs` | Add `DbSet<Customer> Customers` |
| I6 | EDIT | `src/Ai.Api.Infrastructure/DependencyInjection.cs` | Register `ICustomerRepository → CustomerRepository` |

### API / Presentation Layer
| # | Action | File | Notes |
|---|--------|------|-------|
| P1 | CREATE | `src/Ai.Api/Models/Requests/CreateCustomerRequest.cs` | FirstName, LastName, TaxId, Comments |
| P2 | CREATE | `src/Ai.Api/Models/Requests/UpdateCustomerRequest.cs` | FirstName, LastName, TaxId, Comments |
| P3 | CREATE | `src/Ai.Api/Models/Responses/CustomerResponse.cs` | Id, FirstName, LastName, TaxId, Comments |
| P4 | CREATE | `src/Ai.Api/Mappers/CustomerMappingExtensions.cs` | Request→Command, Dto→Response mappings |
| P5 | CREATE | `src/Ai.Api/Controllers/CustomersController.cs` | Full CRUD controller using `IMessageBus` mediator pattern |

---

## Implementation Details

### 1. Model Mapping (Fields → C# Properties → DB Columns)

| Story Field | C# Property | DB Column | Type | Constraints |
|-------------|-------------|-----------|------|-------------|
| id | Id | id | `Guid` (PK) | `Guid.CreateVersion7()` |
| first_name | FirstName | first_name | `string(256)` | Nullable |
| last_name | LastName | last_name | `string(256)` | **Mandatory** (`IsRequired`) |
| tax_id | TaxId | tax_id | `string(16)` | **Mandatory**, **Unique Index** |
| comments | Comments | comments | `string(1024)` | Nullable |

**Naming convention**: C# uses PascalCase, DB uses snake_case (via EF Core Fluent API configuration). The EF Core entity configuration will map property names to snake_case column names.

### 2. API Endpoints

| Method | Route | Command/Query | Response | Status Codes |
|--------|-------|---------------|----------|--------------|
| POST | `/customers` | `CreateCustomerCommand` | `CustomerResponse` | 201 (Created), 409 (Conflict - duplicate tax_id) |
| PUT | `/customers/{id:guid}` | `UpdateCustomerCommand` | `CustomerResponse` | 200 (OK), 404 (Not Found), 409 (Conflict) |
| GET | `/customers/{id:guid}` | `GetCustomerByIdQuery` | `CustomerResponse` | 200 (OK), 404 (Not Found) |
| GET | `/customers` | `GetCustomersQuery` | `List<CustomerResponse>` | 200 (OK) |
| DELETE | `/customers/{id:guid}` | `DeleteCustomerCommand` | — | 204 (NoContent), 404 (Not Found) |

### 3. Validation Rules

| Field | Rule | Validator |
|-------|------|-----------|
| FirstName | MaxLength(256) | `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator` |
| LastName | NotEmpty, MaxLength(256) | `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator` |
| TaxId | NotEmpty, MaxLength(16) | `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator` |
| Comments | MaxLength(1024) | `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator` |
| Id | NotEmpty | `UpdateCustomerCommandValidator` only |

### 4. Error Handling Strategy

Following the established pattern from `ApplicationManagement`:
- **404 Not Found**: Throw `InvalidOperationException` with message `"Customer with ID '{id}' was not found."` — caught by `ExceptionHandlingMiddleware` and mapped to 404.
- **409 Conflict**: Throw `InvalidOperationException` with message `"A customer with the tax ID '{taxId}' already exists."` — caught by `ExceptionHandlingMiddleware` and mapped to 409.
- The repository catches `DbUpdateException` and checks for `"duplicate key"` in the inner exception message to detect unique constraint violations on `tax_id`.

### 5. Repository Pattern

`ICustomerRepository` (Application layer) follows the exact same signature pattern as `IApplicationRepository`:
- `GetByIdAsync(Guid id, CancellationToken ct)` → `CustomerDto?`
- `GetAllAsync(CancellationToken ct)` → `IReadOnlyList<CustomerDto>`
- `AddAsync(CreateCustomerDto dto, CancellationToken ct)` → `CustomerDto`
- `UpdateAsync(CustomerDto dto, CancellationToken ct)` → `CustomerDto`
- `DeleteAsync(Guid id, CancellationToken ct)` → `void`

`CustomerRepository` (Infrastructure layer) implements the interface using `AppDbContext`, maps between persistence entities and DTOs internally, and never exposes entity types.

### 6. Database Schema (Expected Migration SQL)

```sql
CREATE TABLE "Customers" (
    "id" uuid NOT NULL,
    "first_name" varchar(256),
    "last_name" varchar(256) NOT NULL,
    "tax_id" varchar(16) NOT NULL,
    "comments" varchar(1024),
    CONSTRAINT "PK_Customers" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "IX_Customers_tax_id" ON "Customers" ("tax_id");
```

---

## Implementation Order

1. **Create Application DTOs** (A1, A2) — Define `CustomerDto` and `CreateCustomerDto` records that serve as the contract between Application and Infrastructure layers.
2. **Create Repository Interface** (A8) — Define `ICustomerRepository` in the Application layer.
3. **Create Infrastructure Entity & Configuration** (I1, I2) — Define the `Customer` persistence entity and its Fluent API configuration (table name, PK, unique index, column constraints).
4. **Create Persistence Mapping Extensions** (I3) — Entity ↔ DTO mapping extension methods.
5. **Create Repository Implementation** (I4) — Implement full CRUD with duplicate key handling.
6. **Update DbContext** (I5) — Add `DbSet<Customer>` to `AppDbContext`.
7. **Register DI** (I6) — Add `ICustomerRepository` → `CustomerRepository` registration in `AddInfrastructure()`.
8. **Create Application Mappings** (A9) — Command ↔ DTO mapping extensions.
9. **Create Validators** (A10, A11) — FluentValidation validators for create and update commands.
10. **Create Commands & Handlers** (A3, A4, A5) — Write operations with Wolverine handlers.
11. **Create Queries & Handlers** (A6, A7) — Read operations with Wolverine handlers.
12. **Create API Request/Response Models** (P1, P2, P3) — API contract models.
13. **Create API Mapping Extensions** (P4) — Request ↔ Command, Dto ↔ Response mappings.
14. **Create Controller** (P5) — `CustomersController` with all 5 endpoints using `IMessageBus`.

---

## Assumptions

| # | Assumption | Justification | User Decision |
|---|------------|---------------|---------------|
| 1 | `first_name` is optional (nullable) — only `last_name` and `tax_id` are marked mandatory in the model | Model definition explicitly marks `last_name` and `tax_id` with `Traits: mandatory` but `first_name` has no such trait | |
| 2 | Duplicate `tax_id` should return HTTP 409 Conflict | Follows the same pattern as `ApplicationRepository` which returns `InvalidOperationException` for duplicate name violations, mapped to 409 by `ExceptionHandlingMiddleware` | |
| 3 | `tax_id` is a free-text string field, not validated against any tax ID format | Model defines `tax_id` simply as `string(16)` with no format constraints | |
| 4 | No authentication/authorization requirements are specified — controller uses the same pattern as `ApplicationsController` | The story says "as an administrator" but no auth mechanism is specified in the AC. We follow the existing controller pattern which has no `[Authorize]` attribute | |
| 5 | The feature name directory uses PascalCase `CustomerManagement` matching the existing `ApplicationManagement` pattern | Follows existing project convention | |
| 6 | The DB table name is `Customers` (plural) matching the `Applications` table convention | Follows existing project convention from `ApplicationEntityConfiguration` | |
| 7 | Wolverine handler discovery in `DependencyInjection.cs` already covers the Application assembly — no changes needed for handler registration | `AddApplication()` already calls `opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly)` which covers all new handlers | |

---

## Questions for Clarification

| # | Question | Impact | User Decision |
|---|----------|--------|---------------|
| Q1 | Should `first_name` be optional or mandatory? The model only marks `last_name` and `tax_id` as mandatory. | Determines the `IsRequired()` call in the entity configuration and `NotEmpty()` rule in validators | |
| Q2 | Should `tax_id` be validated against any specific format (e.g., regex for tax ID format), or is any string up to 16 characters acceptable? | Determines whether additional FluentValidation rules are needed beyond NotEmpty+MaxLength | |
| Q3 | Should there be a pagination mechanism for GET `/customers` (list all), or is returning all records acceptable for now? | Determines whether the query and repository need pagination/sorting parameters. Following `ApplicationRepository` pattern, there is no pagination currently | |
| Q4 | The story metadata says `ticket_num: 001` but the file is named `002_customers.story.md`. Which ticket number should be used? | Affects branch naming, commit messages, and directory naming | |

---

## Risks

| Risk | Mitigation |
|------|------------|
| Duplicate `tax_id` detection relies on parsing exception messages (`"duplicate key"`), which is fragile across database providers and cultures | Follows the established pattern in `ApplicationRepository`. Acceptable for now since PostgreSQL is the fixed provider. Consider a pre-check query if this becomes problematic |
| No pagination on GET `/customers` could cause performance issues with large datasets | Follows the existing `GetApplicationsQuery` pattern. If dataset grows, pagination can be added as a follow-up |
| `first_name` being nullable may cause issues in UI if a display name is always expected | Marked as a question (Q1) for user clarification before implementation |
