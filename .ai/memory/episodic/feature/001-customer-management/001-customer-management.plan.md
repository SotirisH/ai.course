# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## 1. Story Summary

As an administrator, I want to be able to manage customers in the system. This feature provides full CRUD (Create, Read, Update, Delete) operations on Customer records via a RESTful API.

---

## 2. Acceptance Criteria

| # | Criteria |
|---|----------|
| AC1 | **Given** valid customer data, **When** an admin sends `POST /customers`, **Then** the customer is created and returned with a `201 Created` status |
| AC2 | **Given** an existing customer ID, **When** an admin sends `PUT /customers/{id}` with updated data, **Then** the customer is updated and returned |
| AC3 | **Given** an existing customer ID, **When** an admin sends `GET /customers/{id}`, **Then** the customer details are returned |
| AC4 | **Given** customers exist in the system, **When** an admin sends `GET /customers`, **Then** a list of all customers is returned |
| AC5 | **Given** a duplicate `tax_id`, **When** `POST /customers` or `PUT /customers/{id}` is called, **Then** a `409 Conflict` response is returned |
| AC6 | **Given** a non-existent customer ID, **When** `GET /customers/{id}`, `PUT /customers/{id}`, or `DELETE /customers/{id}` is called, **Then** a `404 Not Found` response is returned |
| AC7 | **Given** invalid input (e.g., missing `last_name`, `tax_id` too long), **When** any endpoint is called, **Then** a `400 Bad Request` with validation errors is returned |
| AC8 | **Given** an existing customer ID, **When** an admin sends `DELETE /customers/{id}`, **Then** the customer is deleted and `204 No Content` is returned |

---

## 3. Spec Consistency Check

| # | Issue | Severity |
|---|-------|----------|
| SI-1 | The model defines `first_name` as `string(256)` without `Traits: mandatory`, making it optional (nullable). This is unusual for a customer's first name and may be an oversight. | Low — clarification optional |
| SI-2 | The `GET /customers` endpoint does not specify any filtering, sorting, or pagination. If the customer list grows large, this may become a performance concern. | Low — out of scope for MVP |
| SI-3 | The story mentions "administrator" but no authentication/authorization requirements are specified in the acceptance criteria. | Low — follows existing pattern |
| SI-4 | The `DELETE /customers/{id}` line in the work item has a stray backtick at the end (`{id}``). This is a formatting typo in the story file, not a functional issue. | Trivial |

---

## 4. Naming Convention Checkpoint

| Concept | Proposed Name | Convention Rule | ✅/❌ |
|---------|--------------|-----------------|------|
| Feature folder | `CustomerManagement` | PascalCase, singular/plural per feature | ✅ |
| Command | `CreateCustomerCommand` | Verb + noun + "Command" | ✅ |
| Command | `UpdateCustomerCommand` | Verb + noun + "Command" | ✅ |
| Command | `DeleteCustomerCommand` | Verb + noun + "Command" | ✅ |
| Query | `GetCustomerByIdQuery` | "Get" + noun + "Query" | ✅ |
| Query | `GetCustomersQuery` | "Get" + noun + "Query" | ✅ |
| DTO | `CustomerDto` | Descriptive + "Dto" suffix | ✅ |
| DTO | `CreateCustomerDto` | Descriptive + "Dto" suffix | ✅ |
| Repository interface | `ICustomerRepository` | "I" prefix + descriptive | ✅ |
| Repository impl | `CustomerRepository` | Entity name + "Repository" | ✅ |
| Entity | `Customer` | Singular, no "Entity" suffix — matches existing `Application` entity pattern | ✅ |
| DbContext property | `Customers` | Entity name (plural) | ✅ |
| Controller | `CustomersController` | Entity name (plural) + "Controller" | ✅ |
| Request model | `CreateCustomerRequest` | Descriptive + "Request" | ✅ |
| Request model | `UpdateCustomerRequest` | Descriptive + "Request" | ✅ |
| Response model | `CustomerResponse` | Descriptive + "Response" | ✅ |

---

## 5. File Change List

### 5.1 Domain Layer (`src/Ai.Api.Domain/`)

| Action | File | Purpose |
|--------|------|---------|
| ✅ Already exists | `Exceptions/DomainException.cs` | Base domain exception — reuse for business rule violations |

No new Domain files needed.

### 5.2 Application Layer (`src/Ai.Api.Application/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | Command + handler for creating a customer |
| CREATE | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | Command + handler for updating a customer |
| CREATE | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | Command + handler for deleting a customer |
| CREATE | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | Query + handler for retrieving by ID |
| CREATE | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | Query + handler for listing all customers |
| CREATE | `Features/CustomerManagement/DTOs/CustomerDto.cs` | Internal DTO for query results |
| CREATE | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | Internal DTO for create operations (no Id) |
| CREATE | `Interfaces/Repositories/ICustomerRepository.cs` | Repository interface defined in Application layer |
| CREATE | `Validators/CreateCustomerCommandValidator.cs` | FluentValidation validator for create command |
| CREATE | `Validators/UpdateCustomerCommandValidator.cs` | FluentValidation validator for update command |
| CREATE | `Mappings/CustomerMappingExtensions.cs` | Extension methods for Command ↔ DTO mapping |

### 5.3 Infrastructure Layer (`src/Ai.Api.Infrastructure/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Persistence/Entities/Customer.cs` | Persistence entity (singular, matching `Application` pattern) |
| CREATE | `Persistence/Configurations/CustomerEntityConfiguration.cs` | EF Core Fluent API configuration |
| CREATE | `Persistence/Repositories/CustomerRepository.cs` | Repository implementation |
| CREATE | `Persistence/CustomerPersistenceMappingExtensions.cs` | Extension methods for Entity ↔ DTO mapping |
| MODIFY | `Persistence/Context/AppDbContext.cs` | Add `DbSet<Customer> Customers` property |
| MODIFY | `DependencyInjection.cs` | Register `ICustomerRepository → CustomerRepository` |

### 5.4 API Layer (`src/Ai.Api/`)

| Action | File | Purpose |
|--------|------|---------|
| CREATE | `Models/Requests/CreateCustomerRequest.cs` | POST request model |
| CREATE | `Models/Requests/UpdateCustomerRequest.cs` | PUT request model |
| CREATE | `Models/Responses/CustomerResponse.cs` | Response model |
| CREATE | `Controllers/CustomersController.cs` | API controller with 5 endpoints |
| CREATE | `Mappers/CustomerMappingExtensions.cs` | Extension methods for Request↔Command and DTO↔Response mapping |

### 5.5 NuGet Packages

All required packages are already in `Directory.Packages.props` (same stack as Application Management). No new NuGet packages needed.

---

## 6. Implementation Details

### 6.1 Customer Model Mapping

| Field | DTO | Entity | DB Column | API Response |
|-------|-----|--------|-----------|-------------|
| `id` (guid) | `Guid Id` | `Guid Id` (PK) | `Id` | `Guid Id` |
| `first_name` string(256) | `string? FirstName` | `string? FirstName` max 256 | `first_name` | `string? FirstName` |
| `last_name` string(256) mandatory | `string LastName` | `string LastName` max 256, required | `last_name` | `string LastName` |
| `tax_id` string(16) mandatory, unique | `string TaxId` | `string TaxId` max 16, required, unique index | `tax_id` | `string TaxId` |
| `comments` string(1024) | `string? Comments` | `string? Comments` max 1024 | `comments` | `string? Comments` |

### 6.2 Commands

| Command | Properties | Handler Logic |
|---------|-----------|---------------|
| `CreateCustomerCommand` | `FirstName`, `LastName`, `TaxId`, `Comments` | Map to `CreateCustomerDto` → call `repository.AddAsync()` |
| `UpdateCustomerCommand` | `Id`, `FirstName`, `LastName`, `TaxId`, `Comments` | Check existence → merge fields → call `repository.UpdateAsync()` |
| `DeleteCustomerCommand` | `Id` | Check existence → call `repository.DeleteAsync()` |

### 6.3 Queries

| Query | Properties | Handler Logic |
|-------|-----------|---------------|
| `GetCustomerByIdQuery` | `Id` | Call `repository.GetByIdAsync()`; throw `InvalidOperationException` if null |
| `GetCustomersQuery` | (none) | Call `repository.GetAllAsync()` |

### 6.4 Repository Interface (`ICustomerRepository`)

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

### 6.5 Validation Rules (FluentValidation)

**CreateCustomerCommandValidator:**
- `FirstName` — MaxLength(256)
- `LastName` — NotEmpty, MaxLength(256)
- `TaxId` — NotEmpty, MaxLength(16)
- `Comments` — MaxLength(1024)

**UpdateCustomerCommandValidator:**
- `Id` — NotEmpty
- `FirstName` — MaxLength(256)
- `LastName` — NotEmpty, MaxLength(256)
- `TaxId` — NotEmpty, MaxLength(16)
- `Comments` — MaxLength(1024)

### 6.6 DB Configuration (`CustomerEntityConfiguration`)

- Table: `Customers`
- Primary key: `Id`
- `LastName` — `.IsRequired()`, `.HasMaxLength(256)`
- `TaxId` — `.IsRequired()`, `.HasMaxLength(16)`, **unique index** (`.HasIndex(x => x.TaxId).IsUnique()`)
- `FirstName` — `.HasMaxLength(256)` (nullable)
- `Comments` — `.HasMaxLength(1024)` (nullable)

### 6.7 Wolverine Integration

No changes needed to the existing Wolverine setup in `Application/DependencyInjection.cs` — handler discovery via `typeof(DependencyInjection).Assembly` will automatically pick up the new Customer handlers.

### 6.8 Error Handling Strategy

| Scenario | Layer | Exception | HTTP Status |
|----------|-------|-----------|-------------|
| Duplicate `tax_id` | Infrastructure | `DbUpdateException` → caught, rethrown as `InvalidOperationException` with "already exists" message | 409 Conflict |
| Not found | Application/Handler | `InvalidOperationException` with "was not found" | 404 Not Found |
| Validation failure | Application (FluentValidation) | `ValidationException` | 400 Bad Request |

The existing `ExceptionHandlingMiddleware` already handles all these patterns via pattern matching on `InvalidOperationException` messages.

### 6.9 Mapping Flow

```
Request: CreateCustomerRequest → CreateCustomerCommand → CreateCustomerDto → Customer (entity) → DB
Response: DB → Customer (entity) → CustomerDto → CustomerResponse
```

---

## 7. Implementation Order

| Step | Layer | Task | Depends On |
|------|-------|------|------------|
| 1 | Application | Create `ICustomerRepository` interface | — |
| 2 | Application | Create `CustomerDto` record | — |
| 3 | Application | Create `CreateCustomerDto` record | — |
| 4 | Application | Create mapping extensions (Command↔Dto) | Steps 2, 3 |
| 5 | Application | Create validators | Steps 1, 4 |
| 6 | Application | Create `CreateCustomerCommand` + handler | Steps 1, 2, 4 |
| 7 | Application | Create `UpdateCustomerCommand` + handler | Steps 1, 2, 4 |
| 8 | Application | Create `DeleteCustomerCommand` + handler | Steps 1, 2 |
| 9 | Application | Create `GetCustomerByIdQuery` + handler | Steps 1, 2 |
| 10 | Application | Create `GetCustomersQuery` + handler | Steps 1, 2 |
| 11 | Infrastructure | Create `Customer` entity | — |
| 12 | Infrastructure | Create `CustomerEntityConfiguration` | Step 11 |
| 13 | Infrastructure | Modify `AppDbContext` — add `Customers` DbSet | Step 11 |
| 14 | Infrastructure | Create persistence mapping extensions | Steps 2, 3, 11 |
| 15 | Infrastructure | Create `CustomerRepository` | Steps 1, 11, 13, 14 |
| 16 | Infrastructure | Modify `DependencyInjection.cs` — register repository | Step 15 |
| 17 | API | Create `CreateCustomerRequest` model | — |
| 18 | API | Create `UpdateCustomerRequest` model | — |
| 19 | API | Create `CustomerResponse` model | — |
| 20 | API | Create API mapping extensions (Request↔Command, Dto↔Response) | Steps 2, 6-10, 17-19 |
| 21 | API | Create `CustomersController` | Steps 6-10, 20 |
| 22 | — | Create EF Core migration for `Customers` table | Step 13 |

---

## 8. Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | Database provider is PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` already in `Directory.Packages.props`; existing `AppDbContext` uses `UseNpgsql()`. Per `tech-stack.md`, PostgreSQL is the default. |
| A2 | No authentication/authorization in initial implementation | Story says "administrator" but no auth requirements in acceptance criteria; follows the same pattern as the Application Management feature. |
| A3 | `GET /customers` returns all records without pagination | Simpler initial implementation; same pattern as `GetApplicationsQuery`. Pagination can be added later if needed. |
| A4 | `tax_id` uniqueness is enforced at DB level via unique index | Most reliable way to guarantee uniqueness under concurrency; follows the same pattern as `Name` uniqueness in Application Management. |
| A5 | PUT performs a full update (not partial) | Standard REST PUT semantics; PATCH not mentioned in requirements; same pattern as Application Management. |
| A6 | `first_name` is optional (nullable) per the spec | Model definition shows `string(256)` without `Traits: mandatory`, implying it's nullable. This is intentionally following the spec as written. |
| A7 | `comments` is optional (nullable) per the spec | Model definition shows `string(1024)` without `Traits: mandatory`, implying it's nullable. |
| A8 | Mapping uses manual extension methods (no AutoMapper) | Architecture doc favors manual mapping; AutoMapper only for dynamic objects. Follows existing pattern from Application Management. |
| A9 | `ICustomerRepository` lives in Application layer (not Domain) | Per architecture doc: "Not repository interfaces, these should be in Application layer." |
| A10 | Hard delete (not soft delete) | Story mentions `DELETE /customers/{id}` but does not mention soft delete; follows same pattern as Application Management. |
| A11 | Wolverine handler auto-discovery will cover new Customer handlers | The existing `DependencyInjection.cs` uses `opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly)` which auto-discovers all handlers in the Application assembly — no additional registration needed. |
| A12 | Entity named `Customer` (singular, no "Entity" suffix) | The existing `Application` entity uses singular naming without "Entity" suffix. Following this established pattern for consistency. |
| A13 | Entity uses both DataAnnotations and Fluent API configuration | The existing `Application` entity uses `[Key]` and `[MaxLength]` DataAnnotations alongside Fluent API in `ApplicationEntityConfiguration`. Following the same dual-configuration pattern for consistency, even though it's redundant. |

---

## 9. Open Questions

| # | Question | Impact |
|---|----------|--------|
| Q1 | Should `first_name` be mandatory despite the model definition not listing it as such? (See Spec Issue SI-1) | Validator design, DB schema, API contract |
| Q2 | Should `GET /customers` support sorting or filtering in the initial implementation? | Query handler design |
| Q3 | Is the `tax_id` expected to be a specific format (e.g., SSN, VAT, or free text)? | Validator design |
