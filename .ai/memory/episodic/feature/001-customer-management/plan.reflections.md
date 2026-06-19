# Reflect & Adapt — Plan Stage

**Stage**: Planning  
**Feature**: 001 — Customer Management  
**Date**: 2025-01-20

---

## Assessment

### 1. Violations & Showstoppers

- **None identified.** All steps in the planning workflow were followed: metadata extraction, existing plan check, pre-scaffold detection, spec consistency check, file change list, and output generation.

### 2. Process Friction / Workflow Gaps

- **Reflection template missing**: The skill references `.ai/agents/shared/reflect-adapt-output-template.md` but this file does not exist. The reflection is being generated without a structural template. **Systemic** — affects all stages.
- **Pre-scaffold detection ran twice**: The workflow asks to scan for existing files, but the initial `list_dir` on the episodic directory already revealed the existing plan. The pre-scaffold scan on `src/` was still useful for detecting existing code files (none found). **Minor** — no real waste.

### 3. Tooling Friction / Missing Capabilities

- **No friction.** All required tools (`read_file`, `run_in_terminal`, `create_file`, `list_dir`, `ask_questions`) worked as expected.

### 4. Delays, Confusion, or Inefficiencies

- **Existing plan overwrite flow**: The user chose "Overwrite completely" which required a `Remove-Item` step. This worked cleanly. No confusion.
- **Branch already existed**: The `feature/001-customer-management` branch was already checked out, so no branch creation was needed. This is expected for a re-plan.

---

## Root Cause Analysis

| Friction | Why | Incorrect Assumption | Process Gap | Type |
|----------|-----|---------------------|-------------|------|
| Missing reflection template | Template file was referenced but never created | That all referenced files in skills would exist | No validation step to check skill dependencies exist before invoking them | Systemic |

---

## Actionable Improvements

| Category | Improvement | Priority |
|----------|-------------|----------|
| Workflow / Process | Add a pre-flight check in the Reflect & Adapt skill to gracefully handle missing template files (fall back to inline structure) | 🟡 Medium |
| Tooling | Create the missing `.ai/agents/shared/reflect-adapt-output-template.md` template file | 🟠 High |

---

## Lessons Learned

- The existing `ApplicationManagement` feature provides an excellent reference pattern. All file structures, naming conventions, and architectural patterns are consistent and well-documented.
- The spec consistency check is valuable — it caught the "administrator" auth implication that the ACs don't address, which was surfaced as a question.
- Pre-scaffold detection confirmed no existing customer-related code, so all files are clean `CREATE` operations.

---

## Action Items

- [ ] Create `.ai/agents/shared/reflect-adapt-output-template.md` template file
- [ ] Update Reflect & Adapt skill to handle missing template gracefully
