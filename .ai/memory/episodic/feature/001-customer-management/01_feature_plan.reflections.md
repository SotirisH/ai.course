# Continuous Improvement Reflection: 001-customer-management Plan

## What Went Well

1. **Pattern Reuse**: The existing `ApplicationManagement` feature provided a clean, consistent template. All layers (DTOs, Commands/Queries, Repository, Controller, Mappers, Validators) follow identical patterns, reducing cognitive load and implementation risk.

2. **Pre-Scaffold Detection**: Confirmed zero existing customer-related files across all layers, so no collision or refactoring concerns.

3. **Spec Consistency Check**: Identified the `first_name` mandatory ambiguity early — this prevents a rework cycle if the user intended it to be optional.

4. **Bottom-Up Implementation Order**: The 13-step order respects dependency chains, ensuring each layer compiles before the next depends on it.

## What Could Be Improved

1. **Pagination Not Addressed**: The story doesn't mention pagination, but `GET /customers` without it is a known anti-pattern for production APIs. Flagged as a question (Q2) rather than proactively including it.

2. **No Domain Exception Types**: The plan reuses `InvalidOperationException` for not-found and duplicate-key scenarios. A dedicated `CustomerNotFoundException` and `DuplicateTaxIdException` in the Domain layer would be more expressive and enable cleaner middleware-based error mapping. However, this would deviate from the existing Application Management pattern.

3. **Tax ID Format Validation**: The story doesn't specify a tax ID format. Without format validation, the unique constraint is the only guard. A regex-based validator would be more robust but requires user input on the expected format.

## Lessons Learned

1. **Always check the existing codebase patterns first**: The `ApplicationManagement` feature was the perfect reference implementation. Time spent reading existing code (entity, repository, controller, mappers, validators) directly informed every design decision.

2. **Trait omissions in specs are common**: The `first_name` field missing "Traits: mandatory" while `last_name` has it is a classic spec inconsistency. Always cross-reference fields against acceptance criteria.

3. **The architecture rules are well-followed in this codebase**: The separation between API models, Application DTOs, and Infrastructure entities is clean and consistent. The plan mirrors this exactly.

## Action Items

- [ ] Get user answers to Q1–Q5 before implementation begins
- [ ] Consider adding `CustomerNotFoundException` to Domain layer if the team wants to move toward richer domain exceptions (separate discussion)
- [ ] After implementation, verify the plan's file count (20 files) against actual created files
