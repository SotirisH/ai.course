---
name: "Capture Planner Learnings"
description:  Extracts confirmed assumptions and user-decided questions from a planner-generated implementation plan and appends them to a persistent learnings file for future planning sessions.
    Triggered when any of these phrases appear: "capture planner learnings", "save confirmed assumptions", "capture learnings", "/capture-planner-learnings", or any request to persist planner assumptions to learnings.
---

# Skill: Capture Planner Learnings

## Purpose

After the Planner agent generates an implementation plan and the user reviews, confirms assumptions, and answers all questions, this skill extracts those confirmed decisions and appends them to `.ai/memory/procedural/learnings/planner.learnings.md`. This builds a cumulative knowledge base that future planning sessions can reference to avoid repeating the same questions and assumptions.

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `$planFile` | Yes | Path to the planner-generated plan file (e.g., `.ai/memory/episodic/feature/001-customer-management/001-customer-management.plan.md`) |
| `$learningsFile` | No | Path to the learnings output file. Defaults to `.ai/memory/procedural/learnings/planner.learnings.md` |

If `$planFile` is not provided or is empty, ask the user to provide it.

## Pre-Flight: Plan File Validation

1. Use `Test-Path "$planFile"` to verify the plan file exists.
2. **If the file does not exist**: STOP and tell the user the plan file was not found.
3. **If the file exists**: Read the plan file and locate:
   - The `## Assumptions` section — a table with columns `#`, `Assumption`, `Justification`, `User Decision`
   - The `## Questions for Clarification` section — a table with columns `#`, `Question`, `Impact`, `User Decision`
4. **If neither section exists**: STOP and tell the user the plan file doesn't contain Assumptions or Questions sections.

## Extraction Rules

### From the Assumptions Table

Only extract assumptions where `User Decision` contains **✅ Confirmed** (or any text containing "Confirmed"). Exclude any assumption where `User Decision` is empty or does not contain "Confirmed".

For each confirmed assumption, capture:
- The assumption text (column: `Assumption`)
- The justification (column: `Justification`)
- The user decision text (column: `User Decision`)

### From the Questions Table

Only extract questions where `User Decision` is non-empty. A question is considered "answered" if the `User Decision` column has content.

For each answered question, capture:
- The question text (column: `Question`)
- The impact (column: `Impact`)
- The user decision text (column: `User Decision`)

### Metadata Extraction

From the plan's `## Metadata` section, capture:
- **Ticket**: (e.g., `001`)
- **Feature Name**: (e.g., `Customer Management`)
- **Work Item Type**: (e.g., `feature`)

## Learnings File Structure

### If the learnings file does NOT exist

Create `.ai/memory/procedural/learnings/planner.learnings.md` with this initial structure, then append the new entry:

```markdown
# Planner Learnings

> Cumulative knowledge captured from confirmed assumptions and user-decided questions across all planning sessions.
> Each entry represents a decision made during a Planner stage that future plans can reference.

---

```

### If the learnings file DOES exist

Read the existing file to understand its structure and current content. Then append the new entry.

### Entry Format

Each entry is appended as a top-level `##` section:

```markdown
## {ticket_num} — {feature_name}

- **Ticket**: {ticket_num}
- **Feature**: {feature_name}
- **Type**: {work_item_type}
- **Date Captured**: {current_date}

### Confirmed Assumptions

| # | Assumption | Justification | User Decision |
|---|------------|---------------|---------------|
| 1 | {assumption text} | {justification} | {user decision} |
| ... | ... | ... | ... |

### Answered Questions

| # | Question | Impact | User Decision |
|---|----------|--------|---------------|
| 1 | {question text} | {impact} | {user decision} |
| ... | ... | ... | ... |

---
```

## Deduplication

Before appending, check if an entry for the same `{ticket_num} — {feature_name}` already exists in the learnings file:
- **If it exists**: Ask the user: *"An entry for `{ticket_num} — {feature_name}` already exists in the learnings file. Do you want to **Replace** it or **Skip** this entry?"*
  - **Replace**: Remove the existing entry for this ticket/feature and append the new one.
  - **Skip**: STOP without appending.
- **If it does not exist**: Proceed to append.

## Output

1. Confirm the number of assumptions extracted (e.g., "Extracted 10 confirmed assumptions and 3 answered questions.")
2. Confirm the learnings file path and that the entry was appended.
3. Display a brief summary of the captured learnings for user verification.

## Completion Criteria

- [ ] Plan file validated and readable
- [ ] Confirmed assumptions extracted from Assumptions table
- [ ] Answered questions extracted from Questions table
- [ ] Metadata (ticket, feature, type) extracted from plan
- [ ] Learnings file created or updated at `.ai/memory/procedural/learnings/planner.learnings.md`
- [ ] Entry deduplicated (replaced or skipped if already exists)
- [ ] Summary displayed to user

## Notes

- The learnings file is designed to be referenced by future Planner sessions — the Planner persona should check this file before generating new assumptions.
- Only **confirmed** assumptions and **answered** questions are captured. Unresolved items are excluded.
- The file accumulates over time (append mode). Use the deduplication check to avoid duplicates when re-running for the same feature.
