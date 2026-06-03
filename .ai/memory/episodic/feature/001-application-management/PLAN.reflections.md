# Reflect & Adapt: Application Management Feature — Planning

**Date**: 2026-06-02  
**Ticket**: 001  
**Agent**: planner

---

## Friction Log

### 1. Tooling Friction

| Issue | Root Cause | Impact |
|-------|-----------|--------|
| `create_file` cannot overwrite existing files | The tool enforces a no-overwrite policy with a file-conflict error | Required manual `Remove-Item` before re-creating the plan file. Adds an extra step to any "overwrite" workflow. |
| PowerShell rejects `&&` chaining | PowerShell uses `;` not `&&` for command chaining | Initial `git branch --show-current` failed. Had to retry with `;`. Repeated pattern across sessions. |
| `list_dir` on episodic/feature showed stale files | Old `PLAN.reflections.md`, `Implementation.reflections.md`, `compliance-checklist.md` from prior runs | Had to manually clean up to avoid confusion. No way to bulk-clean via tooling. |

### 2. Process Friction

| Issue | Root Cause | Impact |
|-------|-----------|--------|
| "Overwrite completely" duplicates effort | The existing plan was already comprehensive and well-aligned with rules | The new plan is structurally similar. The only meaningful differences are: (a) account for pre-existing `Application.cs` entity → changed CREATE to MODIFY, (b) account for pre-existing `Directory.Packages.props` → changed CREATE to REVIEW, (c) removed DELETE endpoint since not in work item spec. |
| Context file loading required 5 parallel reads | All context files are mandatory per planner instructions | No issue — all loaded successfully. But if one fails, the workflow stops. |

### 3. Work Item Ambiguities

| Issue | Details |
|-------|---------|
| "associated with related configuration IDs" in criteria but not in model | The acceptance criteria text mentions configuration IDs but the Applications model section only defines `id`, `name`, `comments`. Flagged as Q1. |
| No DELETE endpoint listed | Work item lists POST, PUT, GET/{id}, GET. Previous plan assumed DELETE. Flagged as Q2. |
| Entity bypasses validation in second constructor | `Application(Guid id, string name, string? comments)` doesn't call `Validate()`. Flagged as Q4. |

---

## Root Cause Analysis

1. **Overwrite workflow gap**: The planner agent's "Overwrite completely" option isn't natively supported by `create_file`. The agent must detect the conflict and use `run_in_terminal` to delete first. Consider adding an `--overwrite` parameter to the agent's file creation logic.

2. **PowerShell chaining**: The instructions should note that `;` is required on Windows/PowerShell, not `&&`. This is an environment-awareness gap.

3. **Stale artifacts**: Prior planning/implementation sessions left files that became misleading when re-entering planning. The planner should clean the target directory on "Overwrite."

---

## Proposed Improvements

| # | Action | Type |
|---|--------|------|
| 1 | Add a pre-step: when "Overwrite" is selected, clean the target directory (`Remove-Item *.md`) before creating new files | Process |
| 2 | Document that PowerShell uses `;` for command chaining (add to agent persona/instructions) | Documentation |
| 3 | When work item spec has internal contradictions (criteria vs model), surface them more prominently — maybe a dedicated "Spec Issues" section in the plan | Template |
| 4 | Consider separating "plan shell" (directory creation, cleanup) from "plan content" to avoid tool conflict | Architecture |

---

## Summary

The planning workflow completed successfully but with minor friction: a file-overwrite conflict requiring manual cleanup, PowerShell syntax correction, and some duplicate effort from regenerating a plan that was already well-aligned. The key value of the overwrite was re-anchoring the plan to the current state of the codebase (noting existing files like `Application.cs`, `Directory.Packages.props`, `appsettings.json`). Four open questions remain for user clarification before implementation can proceed.
