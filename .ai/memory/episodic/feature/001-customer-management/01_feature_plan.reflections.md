# Reflect & Adapt: Customer Management Planning

## What Went Well

1. **Existing pattern reuse**: The `ApplicationManagement` feature provides a clean, complete reference implementation. Every file, naming convention, and architectural decision has a concrete example to follow.

2. **Spec clarity**: The work item was concise and unambiguous. The model definition, endpoints, and acceptance criteria were all clearly specified with no internal contradictions.

3. **Pre-scaffold detection**: Confirmed zero existing customer-related files across all layers. This means no legacy code to review or refactor — a clean greenfield feature within the existing solution.

4. **Middleware compatibility**: The existing `ExceptionHandlingMiddleware` already handles the exact exception types and message patterns the new handlers will throw. No changes needed to error handling infrastructure.

## What Could Be Improved

1. **Model spec asymmetry**: `first_name` is optional while `last_name` is mandatory. This is unusual for a "person name" model. Flagged as Q1 for user clarification.

2. **No pagination in spec**: The list endpoint (`GET /customers`) has no pagination, filtering, or sorting. For a management feature, this could become a performance issue at scale. Flagged as Q3.

3. **No tax_id format validation**: The spec only says `string(16)` but doesn't specify a format. Real tax IDs typically have format rules. Flagged as Q2.

## Adaptation Actions

- Follow the exact `ApplicationManagement` pattern for all files — same structure, same error handling, same Wolverine usage.
- Use `InvalidOperationException` (not `DomainException`) for not-found and duplicate scenarios to leverage existing middleware mapping.
- The `tax_id` unique constraint requires special attention in both the Fluent API configuration and the repository's duplicate-key detection logic.

## Lessons Learned

- The codebase has a very consistent pattern. Deviation from it would create maintenance burden.
- The `ApplicationRepository` already demonstrates handling of unique constraints with the `IsDuplicateKeyViolation` pattern — this should be exactly replicated for `tax_id`.
- No tests directory exists yet for this solution. Testing strategy is well-documented but test projects haven't been scaffolded.
