# Planner Learnings

> Cumulative knowledge captured from confirmed assumptions and user-decided questions across all planning sessions.
> Each entry represents a decision made during a Planner stage that future plans can reference.

---

## 001 — Customer Management

- **Ticket**: 001
- **Feature**: Customer Management
- **Type**: feature
- **Date Captured**: 2026-06-25

### Confirmed Assumptions

| # | Assumption | Justification | User Decision |
|---|------------|---------------|---------------|
| 1 | `first_name` is optional (nullable) — only `last_name` and `tax_id` are marked mandatory in the model | Model definition explicitly marks `last_name` and `tax_id` with `Traits: mandatory` but `first_name` has no such trait | ✅ Confirmed — first_name is optional |
| 2 | Duplicate `tax_id` should return HTTP 409 Conflict | Follows the same pattern as `ApplicationRepository` which returns `InvalidOperationException` for duplicate name violations, mapped to 409 by `ExceptionHandlingMiddleware` | ✅ Confirmed — 409 Conflict for duplicate tax_id |
| 3 | `tax_id` is a free-text string field, not validated against any tax ID format | Model defines `tax_id` simply as `string(16)` with no format constraints | ✅ Confirmed — free-text, no format validation |
| 4 | No authentication/authorization requirements are specified — controller uses the same pattern as `ApplicationsController` | The story says "as an administrator" but no auth mechanism is specified in the AC. We follow the existing controller pattern which has no `[Authorize]` attribute | ✅ Confirmed — no auth for now |
| 5 | The feature name directory uses PascalCase `CustomerManagement` matching the existing `ApplicationManagement` pattern | Follows existing project convention | ✅ Confirmed — CustomerManagement |
| 6 | The DB table name is `Customers` (plural) matching the `Applications` table convention | Follows existing project convention from `ApplicationEntityConfiguration` | ✅ Confirmed — Customers |
| 7 | Wolverine handler discovery in `DependencyInjection.cs` already covers the Application assembly — no changes needed for handler registration | `AddApplication()` already calls `opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly)` which covers all new handlers | ✅ Confirmed — no DI changes needed for handlers |

### Answered Questions

| # | Question | Impact | User Decision |
|---|----------|--------|---------------|
| Q1 | Should `first_name` be optional or mandatory? The model only marks `last_name` and `tax_id` as mandatory. | Determines the `IsRequired()` call in the entity configuration and `NotEmpty()` rule in validators | Optional (as story implies) |
| Q2 | Should `tax_id` be validated against any specific format (e.g., regex for tax ID format), or is any string up to 16 characters acceptable? | Determines whether additional FluentValidation rules are needed beyond NotEmpty+MaxLength | Any string up to 16 chars (no format validation) |
| Q3 | Should there be a pagination mechanism for GET `/customers` (list all), or is returning all records acceptable for now? | Determines whether the query and repository need pagination/sorting parameters. Following `ApplicationRepository` pattern, there is no pagination currently | No pagination for now (follow existing pattern) |
| Q4 | The story metadata says `ticket_num: 001` but the file is named `002_customers.story.md`. Which ticket number should be used? | Affects branch naming, commit messages, and directory naming | Use 001 (from metadata) |

---
