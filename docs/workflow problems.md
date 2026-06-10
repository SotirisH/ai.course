# Workflow Audit Report

**Date:** 2026-06-10
**Scope:** `.ai/agents/`, `.ai/rules/`, `.ai/prompts/FeatureWorkflow.prompt.md`
**Goal:** Identify vague areas, inconsistencies, and duplications to ensure the 3-stage C# API workflow (Plan → Implement → QA) runs smoothly.

---

## 🔴 Broken References (will cause hard stops)

### 1. `qa.agent.md` — Wrong context file paths

- **File:** `.ai/agents/qa.agent.md` (lines 14–15)
- **Problem:** References `principal-qa/persona.md` and `principal-qa/test-strategy-template.md`, but the actual folder is `.ai/agents/qa/`.
- **:** Change both paths to `.ai/agents/qa/persona.md` and `.ai/agents/qa/test-strategy-template.md`.

User response: Agree with the fix

### 2. `C#Coder.agent.md` — Double-slash typo in paths

- **File:** `.ai/agents/C#Coder.agent.md` (lines 13–14)
- **Problem:** `.ai//rules/tech-stack.md` and `.ai//rules/architecture.md` contain a double slash.
- **Fix:** Remove the extra slash → `.ai/rules/tech-stack.md` and `.ai/rules/architecture.md`.

User response: Agree with the fix

### 3. `index.md` — Inconsistent quote styles on paths

- **File:** `.ai/index.md`
- **Problem:** `"memory/procedural/about.md"` uses quotes; `rules/tech-stack.md` does not. Inconsistent and may confuse path resolution.
- **Fix:** Use a consistent format for all paths (no quotes, or quotes on all).

User response: quotes on all

### 4. `testing.md` in rules is empty

- **File:** `.ai/rules/testing.md`
- **Problem:** The file exists but is completely empty. Its presence is misleading — agents or developers may expect content there.
- **Fix:** Either populate it with canonical testing rules, or delete it and remove any references.

User response: File is already renamed to `.ai/rules/testing-strategy.md` . check if you need to do any modifications to [qa.agent.md](../.ai/agents/qa.agent.md) or [persona.md](../.ai/agents/qa/persona.md)

---

## 🟠 Inconsistencies (will cause confusion mid-workflow)

### 5. Line-limit conflict between `coding-standards.md` and `C#Coder.agent.md`

- **Files:** `.ai/rules/coding-standards.md` vs `.ai/agents/C#Coder.agent.md` (lines 42–44)
- **Problem:**
  - `coding-standards.md` says: max **50 lines per function**, max **300 lines per file**
  - `C#Coder.agent.md` compliance checklist checks for: **100 lines per function**, **400 lines per file**
- **Impact:** The agent will generate a passing checklist against a looser standard than the actual coding rules.
- **Fix:** Align both to the same numbers. Recommend making `coding-standards.md` the single source of truth and updating the agent checklist to match.
  User response: coding-standards.md is the single source of truth. The checklist checks for: max 50 lines per function, max 300 lines per file

### 6. `qa.agent.md` — `implementationPlan` parameter passed but never declared

- **Files:** `.ai/prompts/FeatureWorkflow.prompt.md` (line 59) vs `.ai/agents/qa.agent.md` (lines 7–10)
- **Problem:** The workflow passes `implementationPlan:{path}` to the QA agent, but `qa.agent.md` only declares `workItemFile` as a parameter. The `implementationPlan` is silently ignored.
- **Fix:** Declare `implementationPlan:{path}` as a second parameter in `qa.agent.md` and use it in Phase 2 to read the implementation plan directly.
  User response: Does the qa.agent.md needs the implementationPlan:{path}? Please start a QA session with me


### 7. Path separator inconsistency in `FeatureWorkflow.prompt.md`

- **File:** `.ai/prompts/FeatureWorkflow.prompt.md`
- **Problem:** Mixes Windows backslash `\` and forward slash `/` in path constructions (e.g., `{workspace_root}\.ai\memory\...` vs `.ai/memory/...`).
- **Fix:** Standardize on one separator throughout. Since the target OS is Windows, use backslash consistently in absolute paths, or use forward slash consistently (which works on both).
User response: use forward slash consistently

### 8. `{feature_name}` vs `{feature-name}` variable naming inconsistency

- **Files:** `planner.agent.md`, `FeatureWorkflow.prompt.md`, `C#Coder.agent.md`
- **Problem:** The variable is sometimes written as `{feature_name}` (underscore) and sometimes `{feature-name}` (hyphen). This causes path mismatches between what the Planner writes and what the Coder/QA agents try to read.
- **Fix:** Standardize on `{feature_name}` (underscore) as the variable name everywhere. Use kebab-case only in the actual file/folder path strings (e.g., `{ticket_num}-{feature_name}` → rendered as `001-application-management`).
  User response: Agreed with th fix
---

## 🟡 Vague / Ambiguous Areas (will cause agent hesitation or wrong decisions)

### 9. `planner.agent.md` Step 2 — Contradictory directory-check logic

- **File:** `.ai/agents/planner.agent.md` (lines 33–40)
- **Problem:** Step 2 says "list_dir on `.ai/memory/episodic/{work_item_type}/`", then immediately says "If directory doesn't exist, no existing plans. Look for existing plan files matching the pattern...". The fallback instruction contradicts the primary instruction — you can't list a directory that doesn't exist, and the fallback re-states the same search differently.
- **Fix:** Rewrite as a clear conditional: (a) If directory doesn't exist → proceed to Step 3. (b) If directory exists → search for matching plan file. (c) If plan file found → ask user. (d) If not found → proceed to Step 3.
  User response: Agreed with th fix

### 10. `C#Coder.agent.md` — No `workItemFile` parameter; metadata must come from plan

- **File:** `.ai/agents/C#Coder.agent.md` (lines 21–25)
- **Problem:** The Coder only receives `implementationPlan`. It must extract `ticket_num`, `feature_name`, and `work_item_type` from the plan file. However, the plan file format is not enforced to include these in a machine-readable section. If the Planner formats them differently, the Coder will fail or guess.
- **Fix:** Enforce a mandatory `## Metadata` section at the top of every plan file (output by the Planner) with these three fields in a fixed format. Document this in `planner.agent.md` Output A.
  User response: Agreed with th fix
- 
### 11. `FeatureWorkflow.prompt.md` — Orchestrator never extracts metadata variables

- **File:** `.ai/prompts/FeatureWorkflow.prompt.md`
- **Problem:** Stage 2 and Stage 3 use `{ticket_num}`, `{feature_name}`, and `{work_item_type}` in path constructions, but the workflow orchestrator never extracts these values itself. It relies entirely on the Planner's internal state, which is not passed back to the orchestrator.
- **Fix:** After Stage 1 completes, add an explicit step where the orchestrator reads the plan file and extracts these three values before constructing paths for Stage 2 and Stage 3.
  User response: Agreed with th fix

### 12. `planner.agent.md` — "Edit Mode vs Ask Mode" undefined

- **File:** `.ai/agents/planner.agent.md` (lines 38, 69–70, 89–94, 109)
- **Problem:** The agent spec references "Edit mode" and "Ask mode" in multiple places, but these modes are never defined anywhere in the workflow files. An agent reading this has no way to determine which mode it is in.
- **Fix:** Either (a) define Edit/Ask mode in a shared glossary or at the top of `planner.agent.md`, or (b) remove the distinction and replace with explicit conditional logic (e.g., "If file tools are available, create the branch; otherwise, output the plan as text").
  User response: remove the distinction and the conditional logic. I expect all those to run on agent/edit mode always
### 13. `qa.agent.md` Phase 2 — No recovery path if Phase 1 fails

- **File:** `.ai/agents/qa.agent.md` (lines 41–49)
- **Problem:** Phase 2 says "Read the saved qa-plan from Phase 1" but provides no error handling if the file wasn't saved or Phase 1 was incomplete.
- **Fix:** Add an explicit check: "If the qa-plan file does not exist, STOP and inform the user that Phase 1 must be completed first."
  User response: Agreed with th fix
### 14. `planner.agent.md` Phase A — Tooling workaround baked into spec

- **File:** `.ai/agents/planner.agent.md` (lines 56–71)
- **Problem:** "Output is split into two phases to avoid tool conflicts" is a workaround for a specific tooling limitation baked into the agent spec. If the tool environment changes, this silently breaks without explanation.
- **Fix:** Add a brief comment explaining *why* the split exists (e.g., `# Note: Phase A must complete before Phase B to prevent create_file conflicts with run_in_terminal`), so future maintainers understand the intent.
  User response: Agreed with th fix
---

## 🔵 Duplications (noise that increases token cost and drift risk)

### 15. Naming convention rules duplicated in 3+ places

- **Files:** `architecture.md`, `planner.agent.md` (Step 4), `C#Coder.agent.md` (Before Implementation)
- **Problem:** Commands/Queries naming (`Verb+Noun+Command`, `Verb+Noun+Query`) is defined in full in all three files.
- **Fix:** Keep the canonical definition in `architecture.md` only. Replace inline repetitions in `planner.agent.md` and `C#Coder.agent.md` with: *"Apply naming conventions as defined in `architecture.md` — Naming Conventions section."*
  User response: Agreed with th fix
### 16. Record positional-syntax prohibition duplicated in 3 places

- **Files:** `coding-standards.md` (line 35), `coder/persona.md` (line 21), `C#Coder.agent.md` (line 30)
- **Problem:** "Never use positional syntax for records" is stated verbatim in all three files.
- **Fix:** Keep in `coding-standards.md` only. Reference it from the other two files.
  User response: Agreed with th fix
### 17. OpenAPI/Scalar setup check duplicated

- **Files:** `C#Coder.agent.md` (lines 32–36), `tech-stack.md` (line 35)
- **Problem:** The full OpenAPI + Scalar setup checklist (packages, `AddOpenApi()`, `MapOpenApi()`, `MapScalarApiReference()`) is re-stated in the Coder agent instead of referencing the tech-stack rule.
- **Fix:** Replace the inline checklist in `C#Coder.agent.md` with: *"Verify OpenAPI + Scalar setup per `tech-stack.md` — API section."*
  User response: Agreed with th fix
### 18. Reflect & Adapt section duplicated verbatim across all three agents

- **Files:** `planner.agent.md` (lines 97–105), `C#Coder.agent.md` (lines 47–55), `qa.agent.md` (lines 59–61)
- **Problem:** The Reflect & Adapt output structure (Violations, Process Friction, Tooling Friction, Root Causes, Improvements) is copy-pasted across all three agents with minor wording differences.
- **Fix:** Extract to a shared template at `.ai/agents/shared/reflect-adapt-template.md` and replace all three copies with a reference to it.
  User response: Agreed with th fix
### 19. Test layer definitions duplicated across 4 files

- **Files:** `architecture.md` (lines 29–50), `tech-stack.md` (lines 42–61), `qa/testing-structure.md` (full file), `qa/persona.md` (lines 79–113)
- **Problem:** Unit/Integration/E2E test layer definitions, tools, and characteristics are repeated across all four files with slight variations.
- **Fix:** Designate `qa/testing-structure.md` as the single canonical testing reference. Replace content in `architecture.md` and `tech-stack.md` with a one-line reference. Condense `qa/persona.md` to reference the strategy engine steps only (not re-define the layers).
  User response: Agreed with th fix
---

## Summary Table


| #  | Severity        | File(s)                                                       | Issue                                            | Action                                        |
| -- | --------------- | ------------------------------------------------------------- | ------------------------------------------------ | --------------------------------------------- |
| 1  | 🔴 Broken       | `qa.agent.md`                                                 | Wrong context folder`principal-qa/`              | Fix paths to`qa/`                             |
| 2  | 🔴 Broken       | `C#Coder.agent.md`                                            | Double-slash in rule paths                       | Remove extra slash                            |
| 3  | 🔴 Broken       | `index.md`                                                    | Inconsistent path quote styles                   | Standardize format                            |
| 4  | 🔴 Broken       | `testing.md`                                                  | File is empty                                    | Populate or delete                            |
| 5  | 🟠 Inconsistent | `coding-standards.md` + `C#Coder.agent.md`                    | Line limits conflict (50/300 vs 100/400)         | Align to one standard                         |
| 6  | 🟠 Inconsistent | `qa.agent.md` + `FeatureWorkflow.prompt.md`                   | `implementationPlan` param undeclared in QA      | Declare and use it                            |
| 7  | 🟠 Inconsistent | `FeatureWorkflow.prompt.md`                                   | Mixed path separators`\` and `/`                 | Standardize separator                         |
| 8  | 🟠 Inconsistent | Multiple                                                      | `{feature_name}` vs `{feature-name}`             | Standardize to underscore                     |
| 9  | 🟡 Vague        | `planner.agent.md`                                            | Contradictory directory-check logic              | Rewrite as clear conditional                  |
| 10 | 🟡 Vague        | `C#Coder.agent.md`                                            | Metadata must come from plan, no enforced format | Add mandatory Metadata section to plan output |
| 11 | 🟡 Vague        | `FeatureWorkflow.prompt.md`                                   | Orchestrator never extracts metadata variables   | Add extraction step after Stage 1             |
| 12 | 🟡 Vague        | `planner.agent.md`                                            | "Edit Mode / Ask Mode" undefined                 | Define or remove the distinction              |
| 13 | 🟡 Vague        | `qa.agent.md`                                                 | No recovery if Phase 1 fails                     | Add explicit file-existence check             |
| 14 | 🟡 Vague        | `planner.agent.md`                                            | Tooling workaround unexplained                   | Add explanatory comment                       |
| 15 | 🔵 Duplicate    | `architecture.md`, `planner.agent.md`, `C#Coder.agent.md`     | Naming conventions repeated                      | Canonical in`architecture.md` only            |
| 16 | 🔵 Duplicate    | `coding-standards.md`, `coder/persona.md`, `C#Coder.agent.md` | Record syntax rule repeated                      | Canonical in`coding-standards.md` only        |
| 17 | 🔵 Duplicate    | `C#Coder.agent.md`, `tech-stack.md`                           | OpenAPI/Scalar checklist repeated                | Reference`tech-stack.md` from agent           |
| 18 | 🔵 Duplicate    | All 3 agents                                                  | Reflect & Adapt section copy-pasted              | Extract to shared template                    |
| 19 | 🔵 Duplicate    | 4 files                                                       | Test layer definitions repeated                  | Canonical in`testing-structure.md` only       |

---
