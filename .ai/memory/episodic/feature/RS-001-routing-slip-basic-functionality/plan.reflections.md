# Plan Reflections: RS-001 — Routing Slip — Basic Functionality

## Violations & Showstoppers
None. The planning workflow executed without blockers.

## Process Friction / Workflow Gaps

| # | Issue | Root Cause | Proposed Improvement |
|---|-------|------------|---------------------|
| P1 | The `work_item_type: feature` field uses lowercase, but directory naming uses `feature/` — no issue, but the mapping between metadata value and directory path is implicit. | Metadata field format isn't formally documented. | Add a small mapping table to the planner persona or architecture docs: `feature → feature/`, `bug → bug/`, `chore → chore/`. |
| P2 | The work item file structure (Story, Metadata, Context, Acceptance Criteria) is well-defined but not formally templated. Future work items may drift in format. | No template enforcement. | Create a `work-item-template.md` in `.ai/templates/` and reference it in `AGENTS.md`. |

## Tooling Friction / Missing Capabilities

| # | Issue | Root Cause | Proposed Improvement |
|---|-------|------------|---------------------|
| T1 | `read_file` on a directory path (non-file) fails with a generic error ("File not found") rather than a clear signal that it's a directory. Tried to `read_file` on `src/Ai.Api.Domain/Exceptions` which is a folder. | Tool behavior: `read_file` doesn't distinguish between "directory" and "missing file." | Run `list_dir` first to confirm it's a directory; or the tool could return a distinct error code for directories. |
| T2 | No automated way to check if an existing plan file matches the `{ticket_num}*.plan.md` pattern in a given directory — requires manual filtering of `list_dir` results. | Pattern-based file search across memory directories requires two-step process. | Add a `{ticket_num}` prefix convention that's searchable with `file_search`. The current glob pattern `RS-001*` works but `file_search` didn't find the file because it didn't exist yet — confirmed correct. |

## Spec Quality Assessment

| Aspect | Rating | Notes |
|--------|--------|-------|
| Completeness | Good | Core concepts, acceptance criteria, and out-of-scope items are clear. |
| Ambiguity | Moderate | 7 clarification questions surfaced (Q1–Q7), mostly around state machine edge cases, void results, and builder validation. |
| Consistency | Good | Minor state diagram formatting issue (AC 1.5). Payload duality resolved via non-generic `IActivity` base interface. |

## Summary
The planning workflow was smooth. The main design challenge was reconciling the generic `IActivity<T, TResult>` with the need for a heterogeneous itinerary — resolved by introducing a non-generic `IActivity` base. This is a standard pattern (similar to `IEnumerable`/`IEnumerable<T>`). The user should answer Q1–Q7 before implementation begins.
