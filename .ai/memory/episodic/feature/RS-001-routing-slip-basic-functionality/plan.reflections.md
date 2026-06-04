# Plan Reflections: RS-001 — Routing Slip — Basic Functionality

## 1. Violations & Showstoppers

| # | Issue | Severity | Resolution |
|---|-------|----------|------------|
| **V1** | `IActivity<T, Result>` has shadowed generic `T` — method-level `T` hides class-level `T`. This is a compiler warning (CS0693) and a design bug. | **Showstopper** | Corrected signature to `IActivity<TArgument, TResult>` with `ExecuteAsync(TArgument payload, ...)`. Documented as Spec Issue S1. |
| **V2** | No execution entry point defined. The story mentions an "engine" but no interface or class is specified. | **High** | Introduced `IRoutingSlipEngine` / `RoutingSlipEngine` as Assumption A5, flagged as Spec Issue S5. |

No other showstoppers. The work item is well-scoped for Phase 1.

## 2. Process Friction / Workflow Gaps

| # | Friction | Impact | Proposed Improvement |
|---|----------|--------|---------------------|
| **P1** | The work item uses a generic `IActivity<T, Result>` syntax that is technically broken. The spec mixes two different generic naming conventions (`T` vs `TArgument` vs `TResult`). | Medium — required time to detect and reason about the shadowing bug. | Add a "Spec Validation" pre-check in the planner persona: always validate interface generics for shadowing before proceeding. |
| **P2** | The `Compensated` state appears in §1.1's contract list but is explicitly out of scope in "Out of Scope." This creates ambiguity about whether to include the enum member. | Low | The planner should have a rule: "If a contract element is listed but marked out of scope, include the definition but never produce it." |
| **P3** | Variables are mentioned in §1.3 ("output variables returned by the activity") but never defined structurally. | Medium — forced the planner to invent a design. | Work items should include at minimum a type sketch for any concept mentioned in acceptance criteria. |
| **P4** | The builder API example in §1.4 passes request DTOs to activity constructors but doesn't show how `AddActivity` passes the argument — the builder method signature is left implicit. | Low | Include the `AddActivity` signature in future work items' code examples. |

## 3. Tooling Friction / Missing Capabilities

| # | Friction | Impact | Proposed Improvement |
|---|----------|--------|---------------------|
| **T1** | The `read_file` tool doesn't expand relative paths (e.g., `.ai/agents/planner/persona.md` fails; must use absolute paths like `I:\GitRepo\ai.course\.ai\agents\planner\persona.md`). | Minor — wasted 4 tool calls. | Planner should always resolve workspace root from `environment_info` and prepend it to relative paths. This is a known limitation. |
| **T2** | `list_dir` output doesn't include a structured machine-readable format. Filtering for `RS-001*.plan.md` requires manual human inspection or code-in-thought. | Low | Acceptable for now; the directory is small. |

## 4. Root Causes

1. **Spec shadowing bug (V1):** The work item author likely copy-pasted a generic interface pattern without testing it against a compiler. The intent is clear but the syntax is wrong.
2. **Missing engine entry point (V2):** The work item focuses on the data contract (routing slip, itinerary) but underspecifies the behavioral contract. This is common in Phase 1 of multi-phase features where the engine is implied.

## 5. Actionable Workflow Improvements

| # | Change | Where |
|---|--------|-------|
| **W1** | Add a step in the planning stage: "Validate all interface/type signatures against compiler shadowing rules." | `planner` persona instructions |
| **W2** | Add a rule: "For any concept listed in acceptance criteria but missing a type definition, flag it as a Spec Issue AND create a reasonable assumption." | `planner` persona instructions |
| **W3** | Pre-resolve relative paths in tool calls by concatenating the workspace root. | Agent prompt or tool wrapper |

## 6. Summary

The RS-001 work item is clear in intent and well-scoped for Phase 1. The two main issues — the shadowed generic in `IActivity` and the missing engine specification — were resolved with documented assumptions. The resulting plan introduces 13 new files across the `Core.RoutingSlip` project, adheres to Clean Architecture conventions adapted for a standalone class library, and respects all coding standards (records use class-like syntax, async methods have `Async` suffix, no positional records, etc.).
