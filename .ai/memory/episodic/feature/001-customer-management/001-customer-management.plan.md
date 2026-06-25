# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. The system should allow administrators to create, update, retrieve, delete, and list Customers. Each Customer must have a unique identifier and a unique `tax_id`.

---

## Acceptance Criteria (Given-When-Then)

### AC1 — Create Customer
- **Given** an administrator provides valid customer data (first_name, last_name, tax_id, optional comments)
- **When** a `POST /customers` request is made
- **Then** a new Customer is created, persisted to the database, and the response returns `201 Created` with the full Customer resource including a newly generated GUID `id`

### AC2 — Update Customer
- **Given** a Customer exists with a specific `id`
- **When** a `PUT /customers/{id}` request is made with updated data
- **Then** the Customer is updated, persisted, and the response returns `200 OK` with the updated Customer resource
- **And** if the Customer does not exist, the response returns `404 Not Found`

### AC3 — Get Customer by ID
- **Given** a Customer exists with a specific `id`
- **When** a `GET /customers/{id}` request is made
- **Then** the response returns `200 OK` with the Customer resource
- **And** if the Customer does not exist, the response returns `404 Not Found`

### AC4 — List All Customers
- **Given** one or more Customers exist
- **When** a `GET /customers` request is made
- **Then** the response returns `200 OK` with a list of all Customer resources

### AC5 — Delete Customer
- **Given** a Customer exists with a specific `id`
- **When** a `DELETE /customers/{id}` request is made
- **Then** the Customer is removed from the database and the response returns `204 No Content`
- **And** if the Customer does not exist, the response returns `404 Not Found`

### AC6 — Uniqueness Constraint (tax_id)
- **Given** a Customer already exists with a specific `tax_id`
- **When** an attempt is made to create or update another Customer with the same `tax_id`
- **Then** the response returns `409 Conflict`

### AC7 — Validation
- **Given** required fields are missing or exceed length limits
- **When** a `POST` or `PUT` request is made
- **Then** the response returns `400 Bad Request` with details about the validation errors

---

## Spec Consistency Check

| Check | Status | Detail |
|-------|--------|--------|
| `tax_id` mandatory + unique | ✅ Story + model agree | Model specifies `mandatory` and `unique` |
| `last_name` mandatory | ✅ Story + model agree | Model specifies `mandatory` |
| Endpoints match model | ✅ | All 5 REST endpoints listed with matching model fields |
| `id` as GUID | ✅ | Model specifies `guid` |
| String length limits | ✅ | `first_name`: 256, `last_name`: 256, `tax_id`: 16, `comments`: 1024 |

**No spec issues detected.** The work item is internally consistent.

---

## File Change List

### Domain Layer (`src/Ai.Api.Domain/`)
| Action | File | Notes |
|--------|------|-------|
| No changes needed | N/A | Domain layer only contains exceptions/enums. Existing `DomainException` is sufficient. |

### Application Layer (`src/Ai.Api.Application/`)
| # | Action | File | Notes |
|---|--------|------|-------|
| A1 | CREATE | `Interfaces/Repositories/ICustomerRepository.cs` | Defines `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` — accepts/returns Customer DTOs |
| A2 | CREATE | `Features/CustomerManagement/DTOs/CustomerDto.cs` | `record` with Id, FirstName, LastName, TaxId, Comments |
| A3 | CREATE | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | `record` with FirstName, LastName, TaxId, Comments (no Id) |
| A4 | CREATE | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | `record` + `CreateCustomerCommandHandler` in same file (Wolverine convention) |
| A5 | CREATE | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | `record` + `UpdateCustomerCommandHandler` in same file |
| A6 | CREATE | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | `record` + `DeleteCustomerCommandHandler` in same file |
| A7 | CREATE | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | `record` + `GetCustomerByIdQueryHandler` in same file |
| A8 | CREATE | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | `record` + `GetCustomersQueryHandler` in same file (list all) |
| A9 | CREATE | `Validators/CreateCustomerCommandValidator.cs` | FluentValidation — last_name required, tax_id required + max 16, first_name max 256, comments max 1024 |
| A10 | CREATE | `Validators/UpdateCustomerCommandValidator.cs` | FluentValidation — same rules + id not empty |
| A11 | CREATE | `Mappings/CustomerMappingExtensions.cs` | Extension methods: `CreateCustomerCommand → CreateCustomerDto`, `UpdateCustomerCommand → CustomerDto` (apply to existing) |

### Infrastructure Layer (`src/Ai.Api.Infrastructure/`)
| # | Action | File | Notes |
|---|--------|------|-------|
| I1 | CREATE | `Persistence/Entities/Customer.cs` | EF Core entity: `Id`, `FirstName`, `LastName`, `TaxId`, `Comments` |
| I2 | CREATE | `Persistence/Configurations/CustomerEntityConfiguration.cs` | Fluent API: table "Customers", PK on Id, unique index on TaxId, required/maxlength constraints |
| I3 | CREATE | `Persistence/Repositories/CustomerRepository.cs` | Implements `ICustomerRepository`. Maps between `Customer` entity and DTOs. Handles duplicate key detection for TaxId. |
| I4 | CREATE | `Persistence/CustomerPersistenceMappingExtensions.cs` | Extension methods: `Customer → CustomerDto`, `CreateCustomerDto → Customer`, `CustomerDto.ApplyTo(Customer)` |
| I5 | EDIT | `Persistence/Context/AppDbContext.cs` | Add `DbSet<Customer> Customers` property |
| I6 | EDIT | `DependencyInjection.cs` | Register `ICustomerRepository → CustomerRepository` as Scoped |

### API / Presentation Layer (`src/Ai.Api/`)
| # | Action | File | Notes |
|---|--------|------|-------|
| P1 | CREATE | `Models/Requests/CreateCustomerRequest.cs` | `record` with FirstName, LastName, TaxId, Comments |
| P2 | CREATE | `Models/Requests/UpdateCustomerRequest.cs` | `record` with FirstName, LastName, TaxId, Comments (id from route) |
| P3 | CREATE | `Models/Responses/CustomerResponse.cs` | `record` with Id, FirstName, LastName, TaxId, Comments |
| P4 | CREATE | `Mappers/CustomerMappingExtensions.cs` | Extension methods: request→command, command←id (delete), dto→response |
| P5 | CREATE | `Controllers/CustomersController.cs` | Full CRUD controller: POST, PUT, GET{id}, GET, DELETE. Uses Wolverine `IMessageBus`. Follows `ApplicationsController` pattern. |

---

## Implementation Details

### 1. Customer Model Mapping

| Work Item Field | C# Property | DB Column | Type | Constraints |
|-----------------|-------------|-----------|------|-------------|
| `id` | `Id` | `Id` | `Guid` | PK, generated with `Guid.CreateVersion7()` |
| `first_name` | `FirstName` | `first_name` | `string(256)` | optional |
| `last_name` | `LastName` | `last_name` | `string(256)` | mandatory (`.IsRequired()`) |
| `tax_id` | `TaxId` | `tax_id` | `string(16)` | mandatory (`.IsRequired()`), unique index |
| `comments` | `Comments` | `comments` | `string(1024)` | optional |

### 2. API Endpoints

| Method | Route | Handler | Request | Response |
|--------|-------|---------|---------|----------|
| `POST` | `/customers` | `CreateCustomerCommand` | `CreateCustomerRequest` (body) | `201 Created` + `CustomerResponse` |
| `PUT` | `/customers/{id:guid}` | `UpdateCustomerCommand` | `UpdateCustomerRequest` (body) + id (route) | `200 OK` + `CustomerResponse` |
| `GET` | `/customers/{id:guid}` | `GetCustomerByIdQuery` | id (route) | `200 OK` + `CustomerResponse` |
| `GET` | `/customers` | `GetCustomersQuery` | — | `200 OK` + `IReadOnlyList<CustomerResponse>` |
| `DELETE` | `/customers/{id:guid}` | `DeleteCustomerCommand` | id (route) | `204 No Content` |

### 3. Validation Rules (FluentValidation)

| Field | Create | Update | Rule |
|-------|--------|--------|------|
| `Id` | N/A | Required | `NotEmpty()` |
| `LastName` | Required | Required | `NotEmpty()`, `MaximumLength(256)` |
| `TaxId` | Required | Required | `NotEmpty()`, `MaximumLength(16)` |
| `FirstName` | Optional | Optional | `MaximumLength(256)` |
| `Comments` | Optional | Optional | `MaximumLength(1024)` |

### 4. Error Handling

All exceptions are caught by the existing `ExceptionHandlingMiddleware`:
- `ValidationException` → `400 Bad Request`
- `InvalidOperationException` with "was not found" → `404 Not Found`
- `InvalidOperationException` with "already exists" → `409 Conflict`
- Unhandled → `500 Internal Server Error`

The repository will throw `InvalidOperationException` for not-found and duplicate-tax-id scenarios, which the existing middleware already handles via pattern matching.

### 5. Repository Pattern

Follow the existing `ApplicationRepository` pattern exactly:
- `GetByIdAsync` — uses `AsNoTracking()`, maps entity → DTO
- `GetAllAsync` — uses `AsNoTracking()`, maps list
- `AddAsync` — maps DTO → entity, saves, catches `DbUpdateException` for duplicate key
- `UpdateAsync` — loads entity tracking, applies DTO fields, saves, catches duplicate key
- `DeleteAsync` — loads entity, removes, saves

### 6. Database Schema (EF Core Migration)

Running the migration will produce a `Customers` table:
```sql
CREATE TABLE "Customers" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "first_name" character varying(256),
    "last_name" character varying(256) NOT NULL,
    "tax_id" character varying(16) NOT NULL,
    "comments" character varying(1024)
);
CREATE UNIQUE INDEX "IX_Customers_tax_id" ON "Customers" ("tax_id");
```

---

## Implementation Order

1. **DTOs** (A2, A3) — data contracts first
2. **Repository interface** (A1) — defines what persistence must provide
3. **Entity + Configuration** (I1, I2) — database model
4. **Persistence mapping extensions** (I4) — entity ↔ DTO mapping
5. **AppDbContext update** (I5) — add `DbSet<Customer>`
6. **Repository implementation** (I3) — wire it up
7. **DI registration** (I6) — register `ICustomerRepository`
8. **Commands & Queries** (A4–A8) — Wolverine handlers
9. **Application mapping extensions** (A11) — command ↔ DTO mapping
10. **Validators** (A9–A10) — FluentValidation
11. **API Request/Response models** (P1–P3) — API contracts
12. **API mapping extensions** (P4) — request ↔ command, DTO ↔ response
13. **Controller** (P5) — `CustomersController`
14. **EF Core Migration** — generate and apply migration
15. **Manual smoke test** — verify all endpoints

---

## Assumptions

| # | Assumption | Justification |
|---|------------|---------------|
| 1 | Naming convention `CustomersController` (plural) | Matches existing `ApplicationsController` pattern and REST conventions |
| 2 | Route prefix `/customers` (lowercase, plural) | Matches existing `/applications` pattern |
| 3 | DTO property names use PascalCase (`FirstName`, `LastName`, `TaxId`) | C# conventions; DB column names use snake_case via Fluent API configuration |
| 4 | `tax_id` uniqueness enforced at database level via unique index | Matches existing `Name` unique index pattern in `ApplicationEntityConfiguration` |
| 5 | `Customer` entity class name (not `CustomerEntity`) | Matches existing `Application` entity naming (no "Entity" suffix). Architecture doc says "Entity name + 'Entity'" but existing code uses `Application` without suffix — following existing convention. |
| 6 | No special authorization/authentication required beyond existing setup | Work item says "administrator" but no auth is specified. The existing `ApplicationsController` has no auth attributes. Following existing pattern. |
| 7 | Wolverine `IMessageBus` is used in the controller | Matches existing `ApplicationsController` pattern |
| 8 | DTOs use `record` types with standard class-like syntax | Per coding-standards.md hard rule: no positional record syntax |
| 9 | `Guid.CreateVersion7()` for primary key generation | Per architecture.md DTO Design guidelines |
| 10 | The existing `ExceptionHandlingMiddleware` pattern-matches `InvalidOperationException` messages for 404/409 | The existing "was not found" and "already exists" patterns cover the Customer use case without modification |

---

## Questions for Clarification

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should there be a search/filter capability for `GET /customers` (e.g., by `tax_id` or `last_name`), or is the simple list-all sufficient for now? | Determines whether `GetCustomersQuery` needs optional filter parameters. Currently planned as simple list-all per the work item. |
| Q2 | Should `DELETE` be a hard delete or soft delete (e.g., `IsDeleted` flag)? | Affects entity design, repository logic, and list query filtering. The existing `ApplicationRepository` uses hard delete — assumed same here. |
| Q3 | Is any authentication/authorization middleware expected for these endpoints beyond what's already in the pipeline? | Work item mentions "administrator" — may need `[Authorize]` attributes if authentication is added later. |

---

## Risks

| Risk | Mitigation |
|------|------------|
| `DbUpdateException` duplicate key detection uses string matching on `InnerException.Message` | Same approach used in `ApplicationRepository`. PostgreSQL-specific but works. Could be improved with provider-specific exception handling in future. |
| No dedicated `NotFoundException` or `ConflictException` — relies on `InvalidOperationException` message patterns in middleware | Existing pattern throughout codebase. Consistent but fragile. Could be improved with custom exception types later. |