# Reflection: Customer Management Planning

## What went well

1. **Established pattern reuse**: The `ApplicationManagement` feature provided a clean, consistent template for all layers — DTOs, commands/queries, validators, repository, entity configuration, and controller structure. This eliminated guesswork.
2. **Pre-scaffold detection confirmed greenfield**: No existing customer-related files in any layer, so no collision risk.
3. **Spec consistency check found meaningful issues**: The `first_name` mandatory ambiguity (issue #1) and lack of pagination (issue #2) are worth surfacing to the user.

## What could be improved

1. **Story model is sparse**: Only 4 fields with minimal trait annotations. This leaves many design decisions (format validation, pagination, filtering) as assumptions that need user confirmation.
2. **The ticket number in the story file is `001`** while the file is named `002_customers.story.md` — this is a minor inconsistency but could cause confusion if multiple story files share ticket numbers.

## Decisions made

1. **`FirstName` treated as optional** — Unlike the previous plan version that assumed mandatory, this version respects the story as written: only fields explicitly marked "Traits: mandatory" are required.
2. **`TaxId` treated as user-supplied** — No auto-generation trait in the model.
3. **Duplicate detection via PostgreSQL unique index + `DbUpdateException`** — Same pattern as `ApplicationRepository`.
4. **No Domain layer changes** — `InvalidOperationException` is reused for not-found and duplicate-key scenarios.

## Open risks

1. If `first_name` should actually be mandatory, the optional implementation means the database won't enforce it (no `IsRequired()`) and validation won't catch it — but this is a one-line fix.
2. Without pagination, `GET /customers` could return unbounded results at scale — but the story doesn't specify it, so it's left as a question.
3. The `tax_id` uniqueness constraint in PostgreSQL will generate a specific error message pattern that `IsDuplicateKeyViolation` looks for — this relies on the `"duplicate key"` substring being present in the exception message, which is PostgreSQL-specific but consistent with the existing pattern.
