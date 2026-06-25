# Implementation Reflections — 02_feature_implementation

## Summary
Implemented the full Customer Management feature (CRUD+L) following the exact patterns established by the existing Application Management feature. All 5 endpoints (POST, GET all, GET by ID, PUT, DELETE) were created under the `/customers` route with proper error handling, validation, and Wolverine CQRS mediator pattern.

## Key Achievements
- Created 18 new files across all layers (Application, Infrastructure, API)
- Modified 4 existing files (AppDbContext, DependencyInjection, GlobalUsings, ApplicationsController)
- Compiled with 0 warnings, 0 errors on first successful build (after fixing one ambiguity issue)

## Issues Encountered

### 1. Ambiguous `ToCommand(Guid)` Extension Method
Both `ApplicationMappingExtensions` and `CustomerMappingExtensions` defined `ToCommand(Guid)` extension methods for deleting entities. Since both are in the same namespace (`Ai.Api.Mappers`), the compiler couldn't disambiguate. Fixed by inlining the command construction directly in both controllers' Delete actions instead of using the extension method.

**Lesson**: Avoid defining extension methods with identical signatures in the same namespace across different static classes. Use explicit construction or unique method names.

### 2. Empty File from Timeout
The `CustomerPersistenceMappingExtensions.cs` file was created as empty due to a timeout. Fixed by inserting the content via edit.

## Deviation from Plan
- **P5 Controller Delete**: The plan specified using `id.ToCommand()` but this caused ambiguity with existing extension methods. Used `new DeleteCustomerCommand { Id = id }` instead.

## Coding Standards Applied
- All records use non-positional (class-like) syntax
- All async methods have Async suffix
- Primary constructors for DI
- Fluent API for EF Core configurations
- Mapping via extension methods
- Wolverine CQRS pattern with co-located command/query + handler files
- FluentValidation validators for input validation
