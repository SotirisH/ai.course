# Customer Management — Implementation Plan

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system. The system must support full CRUD operations (Create, Read, Update, Delete) plus list-all for Customers. Each Customer has a unique identifier (Guid), a mandatory last name, a mandatory and unique tax ID, an optional first name, and optional comments.

---

## Acceptance Criteria

**Given** an administrator is using the system  
**When** they interact with the `/customers` endpoints  
**Then** they should be able to:

| Operation | HTTP Method | Endpoint              | Description                       |
|-----------|-------------|-----------------------|-----------------------------------|
| Create    | POST        | `/customers`          | Create a new customer             |
| Update    | PUT         | `/customers/{id}`     | Update an existing customer       |
| Retrieve  | GET         | `/customers/{id}`     | Get a single customer by ID       |
| List      | GET         | `/customers`          | Get all customers                 |
| Delete    | DELETE      | `/customers/{id}`     | Delete a customer by ID           |

---

## Customer Model

| Field       | C# Name    | Type          | Constraints                        |
|-------------|------------|---------------|------------------------------------|
| id          | Id         | Guid (PK)     | Generated via `Guid.CreateVersion7()` |
| first_name  | FirstName  | string(256)   | Optional                           |
| last_name   | LastName   | string(256)   | **Mandatory**                      |
| tax_id      | TaxId      | string(16)    | **Mandatory, Unique**              |
| comments    | Comments   | string(1024)  | Optional                           |

---

## Spec Consistency Check

| Check | Status | Detail |
|-------|--------|--------|
| Story ↔ ACs | ✅ Match | All CRUD operations covered |
| ACs ↔ Model | ✅ Match | All fields in model are used in operations |
| Auth | ⚠️ Flagged | Story says "as an administrator" but no auth mechanism is specified in ACs. Following existing pattern (no auth middleware on controllers). See **Questions**. |
| Endpoints | ✅ Match | All 5 endpoints defined in ACs |
| Model traits | ✅ Match | `last_name` marked mandatory, `tax_id` marked mandatory+unique, `first_name` and `comments` have no mandatory trait → optional |

---

## File Change List

### Domain Layer (`Ai.Api.Domain`)

| Action | File | Notes |
|--------|------|-------|
| No changes | — | Existing `DomainException` is sufficient; no new enums, events, or exceptions needed |

### Application Layer (`Ai.Api.Application`)

| Action | File | Notes |
|--------|------|-------|
| CREATE | `Features/CustomerManagement/DTOs/CustomerDto.cs` | Return DTO with all 5 fields |
| CREATE | `Features/CustomerManagement/DTOs/CreateCustomerDto.cs` | Input DTO (no Id) |
| CREATE | `Features/CustomerManagement/Commands/CreateCustomerCommand.cs` | Command + Handler (CQRS, same file) |
| CREATE | `Features/CustomerManagement/Commands/UpdateCustomerCommand.cs` | Command + Handler |
| CREATE | `Features/CustomerManagement/Commands/DeleteCustomerCommand.cs` | Command + Handler |
| CREATE | `Features/CustomerManagement/Queries/GetCustomerByIdQuery.cs` | Query + Handler |
| CREATE | `Features/CustomerManagement/Queries/GetCustomersQuery.cs` | Query + Handler |
| CREATE | `Interfaces/Repositories/ICustomerRepository.cs` | Repository interface |
| CREATE | `Mappings/CustomerMappingExtensions.cs` | Command→Dto mapping extensions |
| CREATE | `Validators/CreateCustomerCommandValidator.cs` | FluentValidation |
| CREATE | `Validators/UpdateCustomerCommandValidator.cs` | FluentValidation |

### Infrastructure Layer (`Ai.Api.Infrastructure`)

| Action | File | Notes |
|--------|------|-------|
| CREATE | `Persistence/Entities/Customer.cs` | EF Core entity |
| CREATE | `Persistence/Configurations/CustomerEntityConfiguration.cs` | Fluent API config (unique index on TaxId) |
| CREATE | `Persistence/Repositories/CustomerRepository.cs` | Full CRUD + duplicate handling |
| CREATE | `Persistence/CustomerPersistenceMappingExtensions.cs` | Entity↔Dto mapping |
| MODIFY | `Persistence/Context/AppDbContext.cs` | Add `DbSet<Customer> Customers` |
| MODIFY | `DependencyInjection.cs` | Register `ICustomerRepository` |

### API Layer (`Ai.Api`)

| Action | File | Notes |
|--------|------|-------|
| CREATE | `Controllers/CustomersController.cs` | 5 endpoints, Wolverine mediator |
| CREATE | `Models/Requests/CreateCustomerRequest.cs` | Request model |
| CREATE | `Models/Requests/UpdateCustomerRequest.cs` | Request model |
| CREATE | `Models/Responses/CustomerResponse.cs` | Response model |
| CREATE | `Mappers/CustomerMappingExtensions.cs` | Request↔Command, Dto↔Response mappings |

---

## Implementation Details

### DTOs

```csharp
// CustomerDto — full return DTO
public sealed record CustomerDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

// CreateCustomerDto — input-only DTO (no Id)
public sealed record CreateCustomerDto
{
    public string? FirstName { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Comments { get; init; }
}
```

### Entity Configuration (Fluent API)

- Table: `Customers`
- `Id`: Primary key
- `FirstName`: MaxLength 256 (optional)
- `LastName`: Required, MaxLength 256
- `TaxId`: Required, MaxLength 16, **Unique Index**
- `Comments`: MaxLength 1024 (optional)

### Unique Constraint Handling

The `CustomerRepository` must handle `DbUpdateException` for duplicate key violations on `TaxId` uniqueness. Follow the existing `ApplicationRepository` pattern with `IsDuplicateKeyViolation()`.

### Validation Rules

| Field | Create | Update |
|-------|--------|--------|
| Id | N/A | NotEmpty |
| FirstName | MaxLength(256) | MaxLength(256) |
| LastName | NotEmpty, MaxLength(256) | NotEmpty, MaxLength(256) |
| TaxId | NotEmpty, MaxLength(16) | NotEmpty, MaxLength(16) |
| Comments | MaxLength(1024) | MaxLength(1024) |

### DI Registration

In `Ai.Api.Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
```

### Controller Pattern

Follow the existing `ApplicationsController` pattern:
- Use `IMessageBus` (Wolverine mediator) for command/query dispatch
- `[ApiConventionMethod]` on Post, Get (by id), Put, Delete
- `[ProducesResponseType]` for 409 Conflict on Create and Update
- `CreatedAtAction` for Create, `Ok` for Get/Update, `NoContent` for Delete
- Route constraint: `{id:guid}` for single-resource endpoints

---

## Implementation Order

1. **Domain Layer**: No changes needed
2. **Application Layer — DTOs**: `CustomerDto`, `CreateCustomerDto`
3. **Application Layer — Interfaces**: `ICustomerRepository`
4. **Application Layer — Mappings**: `CustomerMappingExtensions`
5. **Application Layer — Validators**: `CreateCustomerCommandValidator`, `UpdateCustomerCommandValidator`
6. **Application Layer — Commands & Queries**: All 5 handlers
7. **Infrastructure Layer — Entity**: `Customer`
8. **Infrastructure Layer — Configuration**: `CustomerEntityConfiguration`
9. **Infrastructure Layer — Mapping Extensions**: `CustomerPersistenceMappingExtensions`
10. **Infrastructure Layer — Repository**: `CustomerRepository`
11. **Infrastructure Layer — DbContext**: Add `DbSet<Customer>` to `AppDbContext`
12. **Infrastructure Layer — DI**: Register `ICustomerRepository`
13. **API Layer — Models**: `CreateCustomerRequest`, `UpdateCustomerRequest`, `CustomerResponse`
14. **API Layer — Mappers**: `CustomerMappingExtensions`
15. **API Layer — Controller**: `CustomersController`
16. **Generate & apply EF Core migration**

---

## Assumptions

| # | Assumption | Justification |
|---|------------|---------------|
| A1 | No authorization/authentication required | Story says "as an administrator" but no auth mechanism is specified in ACs. Existing controllers have no auth attributes. |
| A2 | Snake_case model fields → PascalCase in C# | Standard C# naming convention. Database column names will follow snake_case per EF Core conventions or explicit config. |
| A3 | TaxId uniqueness enforced at DB level via unique index | Model specifies `unique` trait. Following existing pattern (unique index on `Name` in Application). |
| A4 | `Guid.CreateVersion7()` for ID generation | Per architecture rules for performance with sequential GUIDs in databases. |
| A5 | No domain events or enums needed | Story doesn't mention status, lifecycle, or events. |
| A6 | Feature DTOs stay in feature-level `DTOs/` folder | Following existing `ApplicationManagement` pattern. No other features share these DTOs yet. |
| A7 | `ICustomerRepository` follows same contract as `IApplicationRepository` | Consistency across the codebase. |
| A8 | `InvalidOperationException` used for "not found" and "duplicate" scenarios | Aligns with existing `ApplicationRepository` pattern and `ExceptionHandlingMiddleware`. |
| A9 | Wolverine mediator used for all command/query dispatch | Per architecture rules and existing controller pattern. |
| A10 | No `[ApiConventionMethod]` on `GetAll` | Following existing `ApplicationsController` pattern (only Post/Get(id)/Put/Delete have the convention attribute). |
| A11 | `FirstName` is optional | Model spec says `first_name: datatype: string(256)` without the "mandatory" trait, unlike `last_name`. |
| A12 | Entity class uses DataAnnotations for basic constraints + Fluent API configuration class for advanced config | Following existing `Application` entity pattern (`[Key]`, `[MaxLength]` on entity + `IEntityTypeConfiguration<T>` for indexes and `IsRequired`). |

---

## Questions

| # | Question | Context |
|---|----------|---------|
| Q1 | Should authorization be implemented? The story says "as an administrator" but no auth is specified in the ACs. | If yes, what auth mechanism (JWT, API key, etc.)? This would add new files across all layers. |
| Q2 | Should the `GetAll` (list) endpoint support pagination, sorting, or filtering? | The ACs don't mention it, but listing all customers could become a performance concern at scale. |
| Q3 | Should `TaxId` be validated for format (e.g., regex pattern for tax ID format)? | The model only specifies `string(16)` with no format constraint. |
