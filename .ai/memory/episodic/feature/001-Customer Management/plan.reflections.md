# Reflect & Adapt Document

**Stage:** Planning
**Feature:** Customer Management (ticket #001)
**Work Item Type:** feature

---

## 1. Assess Friction Encountered

### Violations & Showstoppers

- **None identified.** All process steps were followed as defined in the FeatureWorkflow prompt.

### Process Friction / Workflow Gaps

1. **Branch naming conflict**: The instruction specified `feature/{ticket_num}-{feature_name}` (e.g., `feature/001-Customer Management`), but Git rejected spaces and mixed-case in branch names. The actual branch was already created as `feature/001-customer-management` (kebab-case). The `coding-standards.md` states "Use kebab-case when creating branches" which contradicts the plan template's example. The planner should use kebab-case by default or fall back to it when Git rejects the original format.

2. **Pre-existing stale artifacts**: The branch `feature/001-customer-management` already existed with stale `.plan.md`, `.reflections.md`, and `compliance-checklist.md` files from a prior attempt. The plan's cleanup logic handled this correctly by checking for and removing existing files, but this caused a brief confusion about whether to keep/update/overwrite the existing plan.

3. **Pre-existing API models**: The API Models (`CreateCustomerRequest`, `UpdateCustomerRequest`, `CustomerResponse`) already existed in the codebase from a prior scaffolding run. The plan correctly identifies these as existing and marks them for review, but the workflow has no standard procedure for detecting and handling such pre-existing artifacts.

### Tooling Friction / Missing Capabilities

- **None identified.** All tools functioned as expected.

### Delays, Confusion, or Inefficiencies

- Branch creation failed once due to spaces in the branch name, requiring a manual fallback to kebab-case. Minimal delay (~30 seconds).

---

## 2. Identify Root Causes

| Issue | Root Cause | Systemic or One-Time? |
|-------|-----------|----------------------|
| Branch naming failure | The prompt's example branch name (`feature/{ticket_num}-{feature_name}`) used a space (`Customer Management`). Git disallows spaces. Also, `coding-standards.md` mandates kebab-case. The prompt template and coding-standards.md are inconsistent. | **Systemic** — the prompt template should be updated to use kebab-case. |
| Stale artifacts in existing branch | Prior incomplete workflow runs left artifacts behind. The cleanup logic worked but could be more proactive about detecting stale artifacts before the plan stage. | **One-Time** — this was a first-run issue for this feature. |
| Pre-existing API models | The prior scaffolding run created Customer API models but never completed the full feature. No process exists to inventory pre-existing files before planning. | **Systemic** — the planner should scan for pre-existing files as part of Step 4 (Identify required file changes). |

---

## 3. Propose Actionable Improvements

### Workflow / Process

| Improvement | Description |
|-------------|-------------|
| Update branch naming in prompt | Change the example in FeatureWorkflow.prompt.md from `feature/{ticket_num}-{feature_name}` to explicitly use kebab-case: `feature/{ticket_num}-{feature_name}` with a note that kebab-case must be used per coding-standards.md. |
| Pre-scaffold detection | Add a step in the "Identify required file changes" phase to scan all layers for files that already match the feature's naming pattern and flag them for review rather than blind creation. |

### Tooling

| Improvement | Description |
|-------------|-------------|
| Automated pre-flight check | Add a `run_in_terminal` step before Phase A to check for existing files matching the feature name across all layers, and report them back to the planner. |

### Skill / Knowledge

| Improvement | Description |
|-------------|-------------|
| Branch naming awareness | The planning agent should always derive branch names from `coding-standards.md` (kebab-case) rather than the prompt template example when a conflict exists. |

---

## 4. Prioritize Improvements

| Priority | Improvement | Category |
|----------|-------------|----------|
| 🟡 Medium | Update branch naming in prompt to use kebab-case | Workflow |
| 🟡 Medium | Pre-scaffold detection for existing feature files | Workflow |
| 🔵 Low | Automated pre-flight check for existing files | Tooling |
| 🟠 High | Branch naming awareness — prioritize coding-standards.md over prompt example | Skill/Knowledge |
