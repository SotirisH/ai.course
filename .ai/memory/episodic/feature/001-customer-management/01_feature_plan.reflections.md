# Reflection: Customer Management Feature Plan

## What Went Well
- The existing `ApplicationManagement` feature provided an excellent template — all conventions (naming, mapping extension locations, error handling via `InvalidOperationException`, Wolverine `IMessageBus` pattern, FluentValidation, unique-index-for-uniqueness) were immediately clear.
- The pre-scaffold scan confirmed a clean slate: no pre-existing customer files to conflict with.
- The `ExceptionHandlingMiddleware` already handles all the error patterns we need (404 via "was not found", 409 via "already exists", 400 via `ValidationException`).

## Challenges
- **Ticket number vs file name mismatch**: The story file is `002_customers.story.md` but `ticket_num` is `001`. The branch already uses `001`. I deferred to `001` for consistency with the branch.
- **`first_name` optional ambiguity**: The story model explicitly marks `last_name` as "mandatory" and `tax_id` as "mandatory, unique" but says nothing about `first_name`. I assumed optional — but this needs user confirmation.
- **Custom exception vs existing pattern tension**: A `CustomerNotFoundException` is semantically correct for Clean Architecture, but the existing error handling pattern uses `InvalidOperationException` with substring matching. I included the custom exception as a domain artifact but defaulted to `InvalidOperationException` for actual error routing to stay consistent.

## Key Insights
- The `Application` entity uses both DataAnnotations (`[Key]`, `[MaxLength]`) on the entity class *and* Fluent API configuration (`IEntityTypeConfiguration`). The `Customer` entity should follow the same dual approach.
- The `Mappings/` folder in Application and `Mappers/` folder in API are intentionally named differently — must follow this convention precisely.
- The Infrastructure `DependencyInjection.cs` needs modification to register `ICustomerRepository` (not just adding the entity/DbSet).

## Deviations from Original (Pre-existing) Plan
- Added Infrastructure `DependencyInjection.cs` modification (I6) — the old plan omitted this.
- Renamed `CustomerMappingExtensions.cs` in Application layer to be in `Mappings/` (not `Mappers/`) to match existing codebase pattern.
- Clarified that entities use both DataAnnotations + Fluent API, not just Fluent API as the architecture doc suggests.
- Added explicit assumption about `IMessageBus.InvokeAsync<T>` pattern matching `ApplicationsController`.
