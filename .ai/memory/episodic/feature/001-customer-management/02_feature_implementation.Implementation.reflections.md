# Implementation Reflections — Customer Management Feature

## Stage: 02_feature_implementation
## Date: 2026-06-25

---

### What Went Well

1. **Pattern Consistency**: The existing `ApplicationManagement` feature provided an excellent reference implementation. All files were replicated with identical structure, naming conventions, and error handling patterns. This made the implementation straightforward and predictable.

2. **Pre-existing Infrastructure**: Several files were already in place before this stage began:
   - `Customer.cs` entity (I1)
   - `CustomerEntityConfiguration.cs` (I2)
   - `Customers` DbSet in `AppDbContext.cs` (I5)
   - `CreateCustomerRequest.cs` (P1), `UpdateCustomerRequest.cs` (P2), `CustomerResponse.cs` (P3)
   This reduced the number of files to create from 21 to 15, accelerating implementation.

3. **Clean Compilation**: The solution compiled with zero errors and zero warnings on the first build attempt. This validates that all cross-layer references, Wolverine handler discovery, and FluentValidation registrations were correct.

4. **Architecture Adherence**: Every layer boundary was respected:
   - Domain layer: No changes needed (no new domain concepts)
   - Application layer: DTOs, interfaces, commands, queries, handlers, validators, mappings
   - Infrastructure layer: Repository implementation, persistence mappings, DI registration
   - API layer: Request/response models, API mappings, controller

### Design Decisions

1. **Error Handling**: Used `InvalidOperationException` consistently with the existing pattern. The `ExceptionHandlingMiddleware` maps these to:
   - 404 when message contains "was not found"
   - 409 when message contains "already exists"
   This avoids creating custom exception types for simple CRUD operations.

2. **Mapping Strategy**: Three separate mapping extension files, each at the appropriate layer boundary:
   - `Application/Mappings/` — Command ↔ DTO mappings
   - `Infrastructure/Persistence/` — Entity ↔ DTO mappings
   - `Api/Mappers/` — Request ↔ Command, DTO ↔ Response mappings
   This maintains clean separation and avoids coupling between layers.

3. **GlobalUsings**: Added `CustomerManagement.Commands` and `CustomerManagement.DTOs` to the Application layer's `GlobalUsings.cs`, consistent with the existing pattern for `ApplicationManagement`.

4. **Duplicate Key Detection**: Followed the same fragile-but-established pattern of parsing exception messages for "duplicate key". This is acceptable since PostgreSQL is the fixed database provider.

### Potential Improvements (Future)

1. **Pagination**: GET `/customers` returns all records. If the dataset grows, pagination should be added.
2. **Duplicate Key Detection**: Consider a pre-check query instead of parsing exception messages for cross-database portability.
3. **Soft Delete**: Consider implementing soft delete (e.g., `IsDeleted` flag) instead of hard deletion for audit purposes.

### File Changes Summary

| Action | Count | Files |
|--------|-------|-------|
| Created | 15 | All Application layer files + Infrastructure mappings/repo + API mappings/controller |
| Edited | 2 | `DependencyInjection.cs`, `GlobalUsings.cs` |
| Pre-existing | 7 | Entity, Config, DbContext, API request/response models |