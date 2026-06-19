# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## 1. Story Summary

As an administrator, I want to be able to manage customers in the system. This feature provides full CRUD (Create, Read, Update, Delete, List) operations on Customer records via a RESTful API.

---

## 2. Acceptance Criteria

| # | Criteria |
|---|----------|
| AC1 | **Given** valid customer data, **When** an admin sends `POST /customers`, **Then** the customer is created and returned with a `201 Created` status |
| AC2 | **Given** an existing customer ID, **When** an admin sends `PUT /customers/{id}` with updated data, **Then** the customer is updated and returned |
| AC3 | **Given** an existing customer ID, **When** an admin sends `GET /customers/{id}`, **Then** the customer details are returned |
| AC4 | **Given** customers exist in the system, **When** an admin sends `GET /customers`, **Then** a list of all customers is returned |
| AC5 | **Given** an existing customer ID, **When** an admin sends `DELETE /customers/{id}`, **Then** the customer is deleted and `204 No Content` is returned |
| AC6 | **Given** a duplicate tax_id, **When** `POST /customers` or `PUT /customers/{id}` is called, **Then** a `409 Conflict` response is returned |
| AC7 | **Given** a non-existent customer ID, **When** `GET /customers/{id}`, `PUT /customers/{id}`, or `DELETE /customers/{id}` is called, **Then** a `404 Not Found` response is returned |
| AC8 | **Given** invalid input (e.g., missing last_name, tax_id too long), **When** any endpoint is called, **Then** a `400 Bad Request` with validation errors is returned |

---

## 3. Spec Issues

| # | Issue | Severity |
|---|-------|----------|
| SI-1 | Acceptance criteria text says "create, update, retrieve, and list" but the endpoint list also includes `DELETE /customers/{id}`. The endpoint list is treated as authoritative — DELETE is included. | Low — resolved by treating endpoint list as canonical |
| SI-2 | `first_name` is not marked as mandatory in the model definition, while `last_name` is mandatory. This asymmetry is intentional per the spec and will be honored. | Low — informational |

---

## 4. File Change List

### 4.1 Domain Layer (`src/Ai.Api.Domain/`)

| Action | File | Purpose |
|--------|------|---------|
| — | No changes | Domain layer is thin (exceptions only). Existing `DomainException` is sufficient. |

### 4.2 Application Layer (`src/Ai.Api.Application/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | Command + handler for creating a customer |
| CREATE | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | Command + handler for updating a customer |
| CREATE | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | Command + handler for deleting a customer |
| CREATE | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | Query + handler for retrieving by ID |
| CREATE | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | Query + handler for listing all customers |
| CREATE | `Features/CustomerManagement/DTOs/CustomerDto.cs` | Internal DTO for query results |
| CREATE | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | DTO for create operations |
| CREATE | `Interfaces/Repositories/ICustomerRepository.cs` | Repository interface |
| CREATE | `Validators/CreateCustomerCommandValidator.cs` | FluentValidation validator for create command |
| CREATE | `Validators/UpdateCustomerCommandValidator.cs` | FluentValidation validator for update command |
| CREATE | `Mappings/CustomerMappingExtensions.cs` | Extension methods for command ↔ DTO mapping |

### 4.3 Infrastructure Layer (`src/Ai.Api.Infrastructure/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Persistence/Entities/Customer.cs` | Persistence entity (ORM mapping) |
| CREATE | `Persistence/Configurations/CustomerEntityConfiguration.cs` | EF Core Fluent API configuration (unique index on tax_id, max lengths, required fields) |
| CREATE | `Persistence/Repositories/CustomerRepository.cs` | Repository implementation |
| CREATE | `Persistence/CustomerPersistenceMappingExtensions.cs` | Extension methods for entity ↔ DTO mapping |
| MODIFY | `Persistence/Context/AppDbContext.cs` | Add `Customers` DbSet |
| MODIFY | `DependencyInjection.cs` | Register `ICustomerRepository` → `CustomerRepository` |

### 4.4 API Layer (`src/Ai.Api/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Controllers/CustomersController.cs` | API controller with 5 endpoints |
| CREATE | `Models/Requests/CreateCustomerRequest.cs` | POST request model |
| CREATE | `Models/Requests/UpdateCustomerRequest.cs` | PUT request model |
| CREATE | `Models/Responses/CustomerResponse.cs` | Response model for all endpoints |

### 4.5 NuGet Packages

No new NuGet packages are required. All necessary packages (`WolverineFx`, `WolverineFx.FluentValidation`, `WolverineFx.RuntimeCompilation`, `FluentValidation`, `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`) are already present in `Directory.Packages.props`.

---

## 5. Implementation Details

### 5.1 Customer Model Mapping

| Field | C# Type | DB Type | Constraints |
|-------|---------|---------|-------------|
| `id` | `Guid` | `uuid` | Primary key, generated via `Guid.CreateVersion7()` |
| `first_name` | `string` | `varchar(256)` | Optional |
| `last_name` | `string` | `varchar(256)` | **Required** |
| `tax_id` | `string` | `varchar(16)` | **Required**, **Unique** (unique index) |
| `comments` | `string?` | `varchar(1024)` | Optional |

### 5.2 Commands (CQRS naming checkpoint)

| Command | Format Check | ✅/❌ |
|---------|-------------|------|
| `CreateCustomerCommand` | Verb + Noun + "Command" | ✅ |
| `UpdateCustomerCommand` | Verb + Noun + "Command" | ✅ |
| `DeleteCustomerCommand` | Verb + Noun + "Command" | ✅ |

### 5.3 Queries (CQRS naming checkpoint)

| Query | Format Check | ✅/❌ |
|-------|-------------|------|
| `GetCustomerByIdQuery` | "Get" + Noun + "Query" | ✅ |
| `GetCustomersQuery` | "Get" + Noun + "Query" | ✅ |

### 5.4 Records (syntax check)

All DTOs, commands, queries, requests, responses must use **class-like syntax** — positional syntax is prohibited per `coding-standards.md`.

### 5.5 Wolverine Integration

Follows the existing pattern from Application Management:
- `AddApplication()` in Application layer registers Wolverine with FluentValidation middleware and handler discovery
- `AddInfrastructure()` in Infrastructure layer registers DbContext, repositories, and `AlwaysUseServiceLocationFor<AppDbContext>()`
- No changes needed to `DependencyInjection.cs` in Application layer — Wolverine auto-discovers handlers in the assembly

### 5.6 Error Handling Strategy

| Scenario | Layer | Exception | HTTP Status |
|----------|-------|-----------|-------------|
| Duplicate tax_id | Infrastructure | `DbUpdateException` → caught in repository → `InvalidOperationException` | 409 Conflict |
| Not found | Infrastructure/Repository | Returns null → handler throws `InvalidOperationException` | 404 Not Found |
| Validation failure | Application | `FluentValidation.ValidationException` | 400 Bad Request |
| Missing required field | Application | `FluentValidation.ValidationException` | 400 Bad Request |

### 5.7 Mapping Flow

```
Request:  API Request → Command/Query → DTO → Persistence Entity → DB
Response: DB → Persistence Entity → DTO → API Response
```

### 5.8 API Endpoints

| Method | Route | Handler | Response |
|--------|-------|---------|----------|
| `POST` | `/customers` | `CreateCustomerCommand` | `201 Created` + `CustomerResponse` |
| `PUT` | `/customers/{id:guid}` | `UpdateCustomerCommand` | `200 OK` + `CustomerResponse` |
| `GET` | `/customers/{id:guid}` | `GetCustomerByIdQuery` | `200 OK` + `CustomerResponse` |
| `GET` | `/customers` | `GetCustomersQuery` | `200 OK` + `List<CustomerResponse>` |
| `DELETE` | `/customers/{id:guid}` | `DeleteCustomerCommand` | `204 No Content` |

---

## 6. Implementation Order

| Step | Layer | Task | Depends On |
|------|-------|------|------------|
| 1 | Application | Create `CustomerDto` record | — |
| 2 | Application | Create `CreateCustomerDto` record | — |
| 3 | Application | Create `ICustomerRepository` interface | Steps 1, 2 |
| 4 | Application | Create `CustomerMappingExtensions` (command → DTO) | Steps 1, 2 |
| 5 | Application | Create `CreateCustomerCommandValidator` | Step 2 |
| 6 | Application | Create `UpdateCustomerCommandValidator` | Step 1 |
| 7 | Application | Create `CreateCustomerCommand` + handler | Steps 3, 4 |
| 8 | Application | Create `UpdateCustomerCommand` + handler | Steps 3, 4 |
| 9 | Application | Create `DeleteCustomerCommand` + handler | Step 3 |
| 10 | Application | Create `GetCustomerByIdQuery` + handler | Step 3 |
| 11 | Application | Create `GetCustomersQuery` + handler | Step 3 |
| 12 | Infrastructure | Create `Customer` persistence entity | — |
| 13 | Infrastructure | Create `CustomerEntityConfiguration` (Fluent API) | Step 12 |
| 14 | Infrastructure | Create `CustomerPersistenceMappingExtensions` | Steps 1, 2, 12 |
| 15 | Infrastructure | MODIFY `AppDbContext` — add `Customers` DbSet | Step 12 |
| 16 | Infrastructure | Create `CustomerRepository` | Steps 3, 12, 14, 15 |
| 17 | Infrastructure | MODIFY `DependencyInjection` — register `ICustomerRepository` | Step 16 |
| 18 | API | Create `CreateCustomerRequest` model | — |
| 19 | API | Create `UpdateCustomerRequest` model | — |
| 20 | API | Create `CustomerResponse` model | Step 1 |
| 21 | API | Create `CustomersController` | Steps 7-11, 18-20 |

---

## 7. Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | Database provider is PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` already in Directory.Packages.props; PostgreSQL is the default per tech-stack.md |
| A2 | No authentication/authorization in initial implementation | Story says "administrator" but no auth requirements in acceptance criteria; can be added later as a cross-cutting concern |
| A3 | `GET /customers` returns all records without pagination | Simpler initial implementation; pagination can be added later if needed |
| A4 | `tax_id` uniqueness is enforced at DB level via unique index | Most reliable way to guarantee uniqueness under concurrency; follows same pattern as `Name` uniqueness in Application Management |
| A5 | PUT performs a full update (not partial) | Standard REST PUT semantics; PATCH not mentioned in requirements |
| A6 | `first_name` is optional, `last_name` is required | Per model definition: only `last_name` is marked "mandatory" |
| A7 | EF Core migrations will be generated after entity/configurations are in place | Standard EF Core workflow; separate step from code creation |
| A8 | Mapping uses manual extension methods (no AutoMapper) | Architecture doc favors manual mapping; follows existing Application Management pattern |
| A9 | `ICustomerRepository` lives in Application layer (not Domain) | Per architecture doc: "Not repository interfaces, these should be in Application layer" |
| A10 | DELETE endpoint is included despite not being in the prose acceptance criteria | The endpoint list in the work item explicitly includes `DELETE /customers/{id}`; endpoint list is authoritative |
| A11 | Wolverine handler discovery is automatic — no changes needed to `Application/DependencyInjection.cs` | Existing `AddApplication()` already calls `opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly)` which covers all handlers in the Application assembly |

---

## 8. Open Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should `GET /customers` support sorting or filtering? | Query handler design |
| Q2 | Is soft delete required, or is hard delete acceptable? | Repository and DB design |
| Q3 | Should the `PUT` endpoint allow partial updates (PATCH semantics) or full replacement? | Command and handler design |
