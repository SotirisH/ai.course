# Reflect & Adapt Document

**Feature:** Customer Management (Ticket #001)
**Stage:** Test Plan
**Date:** 2026-06-11

---

## 1. Assess Friction Encountered

### Violations & Showstoppers

| Issue | Detail |
|-------|--------|
| **DELETE endpoint missing** | The implementation plan (Section 5.2) specifies `DeleteCustomerCommand.cs` as a CREATE action, but no such file exists. The `CustomersController` has no `DELETE` action. The repository also lacks a `DeleteAsync` method (the `ICustomerRepository` interface in the codebase shows no `DeleteAsync`). This means all DELETE-related tests in the plan are blocked. |
| **10 recorded acceptance criteria — plan only covers 8** | The work item story lists `DELETE /customers/{id}` in the endpoint list (line 20 of the story) but neither the spec issues nor the acceptance criteria in the implementation plan address it fully. The controller and handler are missing. |
| **Repository interface incomplete** | The `ICustomerRepository` interface in the codebase is missing the `DeleteAsync(Guid id, CancellationToken)` method that the implementation plan describes. |

### Process Friction / Workflow Gaps

| Gap | Description |
|-----|-------------|
| **No existing test project structure** | The `tests/` directory is empty. There are no existing test projects to reference for namespace conventions, project configurations, or test patterns. This is expected for an early-stage project but adds discovery overhead. |
| **Plan-to-code drift** | The implementation plan describes certain elements (DeleteCustomerCommand, ICustomerRepository.DeleteAsync) that are documented but not yet coded. The test planner must cross-reference plan vs. actual code, creating an extra verification step. |
| **Application Mapping Extensions location ambiguity** | The implementation plan says `Mappings/CustomerMappingExtensions.cs` in the Application layer, but the actual file is at `Mappings/CustomerMappingExtensions.cs` — which is correct. However, the API layer has `Mappers/CustomerMappingExtensions.cs` (note: `Mappers` vs `Mappings` — both are in different layers with different purposes). No actual friction here but worth noting consistency is maintained. |

### Tooling Friction / Missing Capabilities

| Issue | Detail |
|-------|--------|
| **No existing test project files** | No `.csproj`, no `GlobalUsings.cs`, no `Usings.cs` for tests exists. The test plan cannot reference existing test conventions. |

### Delays, Confusion, or Inefficiencies

| Issue | Detail |
|-------|--------|
| **Missing DELETE endpoint** | ~30% of the test scenarios (5 out of ~16 Gherkin scenarios) depend on DELETE functionality that doesn't exist yet. This creates a dependency chain issue: test writing cannot proceed for DELETE until implementation is complete. |

---

## 2. Identify Root Causes

| Issue | Root Cause | Classification |
|-------|-----------|---------------|
| DELETE endpoint missing | The implementation plan was written before the code was fully implemented. Step 21 in the implementation order (Delete action on controller) may not have been reached yet, or the plan supersedes actual code. | **Systemic** — This will repeat if implementation order is not strictly followed before the QA planning stage begins. |
| Plan-to-code drift | The test planning stage runs in parallel with or after implementation, but there is no verification gate to confirm "plan matches code" before test planning starts. | **Systemic** — A cross-check step between implementation plan and actual codebase is needed. |
| Tests directory empty | The project is early-stage. Test projects haven't been scaffolded yet, which is normal. However, no scaffolding templates or project guides exist to accelerate this. | **One-time** — Once test projects are created, future features can reference them. |
| `ICustomerRepository` missing `DeleteAsync` | The interface was implemented without the delete method, possibly because DELETE was deprioritized or the implementation plan wasn't fully executed. | **Unclear** — May be intentional (delete out of MVP scope) or an oversight. |

---

## 3. Propose Actionable Improvements

### Workflow / Process

| # | Improvement | Priority |
|---|-------------|----------|
| P1 | **Add a "plan vs. code" validation gate** before test planning. The test planner should automatically flag any mismatches between the implementation plan's file change list and the actual files on disk. This prevents testing planning against planned-but-unimplemented features. | 🟠 High |
| P2 | **Include a "test project scaffold" task in the implementation plan.** When a feature is the first to introduce tests for a given layer, the plan should include creating the `.csproj`, `GlobalUsings.cs`, and fixture classes so test planning can reference real project paths. | 🟠 High |
| P3 | **Define a standard reflection capture point** at the test planning stage to capture spec gaps (like DELETE missing) that may have been missed during implementation planning. This feeds back into the implementation loop. | 🟡 Medium |

### Tooling

| # | Improvement | Priority |
|---|-------------|----------|
| T1 | **Automated plan-to-code verification script** — A PowerShell script that reads the `## File Change List` section from any `*.plan.md`, checks each path on disk, and outputs a diff of "planned vs. actual" files. This would have caught the missing `DeleteCustomerCommand.cs` immediately. | 🟠 High |
| T2 | **Test project scaffolding template** — A reusable template for test `.csproj` files (xUnit + Shouldly + Moq/Mvc.Testing/Testcontainers) that can be copied per test layer to avoid manual NuGet setup. | 🟡 Medium |

### Skill / Knowledge

| # | Improvement | Priority |
|---|-------------|----------|
| K1 | **Document a "test project bootstrap" guide** in the testing strategy doc covering how to create the first test project for a solution, including: adding to `.slnx`, configuring `Directory.Build.props` inheritance, and setting up `GlobalUsings.cs`. | 🔵 Low |

---

## 4. Prioritized Improvement Summary

| Priority | Count | Key Items |
|----------|-------|-----------|
| 🔴 Critical | 0 | — |
| 🟠 High | 3 | P1 (validation gate), P2 (test scaffold task), T1 (plan-to-code verification) |
| 🟡 Medium | 2 | P3 (reflection capture), T2 (test scaffolding template) |
| 🔵 Low | 1 | K1 (test project bootstrap guide) |
