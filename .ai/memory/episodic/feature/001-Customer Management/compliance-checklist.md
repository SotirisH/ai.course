# Compliance Checklist — Customer Management (Feature 001)

## Record Definition Syntax
- [✅] `CustomerDto` — sealed record, class-like syntax (no positional syntax)
- [✅] `CreateCustomerDto` — sealed record, class-like syntax
- [✅] `CreateCustomerCommand` — sealed record, class-like syntax
- [✅] `UpdateCustomerCommand` — sealed record, class-like syntax
- [✅] `GetCustomerByIdQuery` — sealed record, class-like syntax
- [✅] `GetCustomersQuery` — sealed record, class-like syntax
- [✅] `CreateCustomerRequest` — sealed record, class-like syntax
- [✅] `UpdateCustomerRequest` — sealed record, class-like syntax
- [✅] `CustomerResponse` — sealed record, class-like syntax

## Naming Conventions (per architecture.md)
- [✅] Commands: `CreateCustomerCommand`, `UpdateCustomerCommand` — verb + noun + "Command"
- [✅] Queries: `GetCustomerByIdQuery`, `GetCustomersQuery` — "Get" + noun + "Query"
- [✅] Handlers: `CreateCustomerCommandHandler`, `UpdateCustomerCommandHandler`, etc.
- [✅] DTOs: `CustomerDto`, `CreateCustomerDto` — descriptive + "Dto"
- [✅] Repository interface: `ICustomerRepository` — "I" prefix
- [✅] Entity class: `Customers` — matches database table name (plural)
- [✅] Controller: `CustomersController` — entity name + "Controller"
- [✅] Configuration class: `CustomerEntityConfiguration` — descriptive + "Configuration"
- [✅] Repository class: `CustomerRepository` — entity name + "Repository"

## File Placement
- [✅] DTOs in `Features/CustomerManagement/DTOs/`
- [✅] Commands/Queries in `Features/CustomerManagement/Commands/` and `Features/CustomerManagement/Queries/`
- [✅] Repository interface in `Interfaces/Repositories/`
- [✅] Validators in `Validators/`
- [✅] Application mappings in `Mappings/`
- [✅] Entity in `Persistence/Entities/`
- [✅] Configuration in `Persistence/Configurations/`
- [✅] Repository in `Persistence/Repositories/`
- [✅] API request models in `Models/Requests/`
- [✅] API response models in `Models/Responses/`
- [✅] Controller in `Controllers/`
- [✅] API mappers in `Mappers/`

## Async/Await Patterns
- [✅] All I/O methods use async/await
- [✅] All async methods have "Async" suffix
- [✅] Cancellation tokens passed through all async chains
- [✅] No async void methods
- [✅] No `.Result` or `.Wait()` used

## Error Handling
- [✅] `InvalidOperationException` with "was not found" → 404 (handled by middleware)
- [✅] `InvalidOperationException` with "already exists" → 409 (handled by middleware)
- [✅] FluentValidation exception → 400 (handled by middleware)
- [✅] Duplicate key violations caught and wrapped in `InvalidOperationException`
- [✅] DbUpdateException filtered with `IsDuplicateKeyViolation` helper

## API Conventions (per architecture.md)
- [✅] Uses `ActionResult<T>` instead of `IActionResult`
- [✅] Uses `[ApiConventionMethod]` with `DefaultApiConventions`
- [✅] Uses `[FromBody]` and `[FromRoute]` explicitly
- [✅] Route constraint `{id:guid}` used
- [✅] CancellationToken in all endpoints
- [✅] Uses `IMessageBus` (Wolverine mediator)

## Entity Framework
- [✅] Entity configured via `IEntityTypeConfiguration` - `CustomerEntityConfiguration`
- [✅] Table name: `Customers`
- [✅] Primary key on `Id`
- [✅] `TaxId` has unique index
- [✅] `LastName` and `TaxId` configured as `IsRequired()`
- [✅] MaxLength constraints applied to all string properties

## DI Registration
- [✅] `ICustomerRepository` registered as scoped in `DependencyInjection.cs`
- [✅] No changes needed to Application DI — Wolverine auto-discovers handlers

## GlobalUsings
- [✅] `Ai.Api.Application.Features.CustomerManagement.DTOs` added to Application GlobalUsings
- [✅] `Ai.Api.Application.Features.CustomerManagement.Commands` added to Application GlobalUsings
- [✅] API GlobalUsings already has `Ai.Api.Models.Responses` — no change needed

## Mapping Extensions
- [✅] Application: `CreateCustomerCommand → CreateCustomerDto`, `UpdateCustomerCommand.ApplyTo(CustomerDto)`
- [✅] Infrastructure: `CreateCustomerDto → Customers`, `Customers → CustomerDto`, `CustomerDto.ApplyTo(Customers)`
- [✅] API: `CreateCustomerRequest → CreateCustomerCommand`, `UpdateCustomerRequest → UpdateCustomerCommand`, `CustomerDto → CustomerResponse`, `IEnumerable<CustomerDto> → List<CustomerResponse>`

## Build
- [✅] Solution builds with 0 warnings and 0 errors
