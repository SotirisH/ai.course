# Plan Reflections: Customer Management (Ticket 001)

## What Went Well

1. **Comprehensive pre-scaffold detection**: The scan across all layers discovered 3 pre-existing Infrastructure files (`Customers.cs`, `CustomerEntityConfiguration.cs`, and the `AppDbContext` change). This prevented unnecessary file creation and highlighted existing work that needs review.

2. **Pattern consistency**: The plan closely follows the existing Application Management feature pattern (Commands, Queries, DTOs, Validators, Repository, Controller), ensuring architectural consistency across the codebase.

3. **Error handling alignment**: The error handling strategy reuses the existing `ExceptionHandlingMiddleware` pattern matching — no new middleware or exception types needed.

4. **Spec consistency check**: Flagged the unusual `first_name` optionality and the lack of pagination as spec issues for user clarification.

## What Could Be Improved

1. **Entity naming inconsistency**: The existing `Customers.cs` entity uses plural naming ("Customers") while the existing `Application.cs` entity uses singular ("Application"). Neither uses the "Entity" suffix recommended by `architecture.md`. This inconsistency should be resolved before implementation — either standardize on singular (`Customer`) or plural (`Customers`), ideally with the "Entity" suffix.

2. **Redundant DataAnnotations**: The existing `Customers.cs` entity uses `[Key]` and `[MaxLength]` DataAnnotations alongside Fluent API configuration in `CustomerEntityConfiguration.cs`. The DataAnnotations are redundant and could cause confusion about which configuration takes precedence.

3. **Old plan directory with spaces**: The previous plan directory used a space in the name (`001-Customer Management`), which violates the kebab-case naming convention. Cleaned up during this planning stage.

## Lessons Learned

1. When performing pre-scaffold detection, check entity naming patterns carefully — the existing codebase may have inconsistencies that should be addressed before adding new features.

2. The kebab-case convention for directory names is important — spaces in directory names cause issues with git and cross-platform tooling.

## Process Improvements

- **Recommendation**: Add a naming convention checkpoint that explicitly compares entity names across all existing entities before creating new ones, to catch inconsistencies early.
