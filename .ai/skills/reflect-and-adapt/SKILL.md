---
name: "Reflect & Adapt"
description: "Generates a post-stage reflection document assessing friction, root causes, and actionable improvements. Triggered by the phrase 'Reflect & Adapt'."
parameters:
  - name: outputFile
    description: "Absolute path where the reflection document should be saved. Used as both input (to check for prior existence) and output (where the reflection is written)."
    required: true
---

# Skill: Reflect & Adapt

**Trigger**: This skill is activated when the phrase **"Reflect & Adapt"** appears in the conversation. It is designed to run at the end of every workflow stage.

## Pre-Flight: Output File Check

1. Use `run_in_terminal` to check if the output file exists:
   ```
   Test-Path "{outputFile}"
   ```
2. **If the file exists:**
   - Read the existing file using `read_file` and present a brief summary to the user.
   - Ask the user:
     > *"A reflection file already exists at `{outputFile}`. Do you want to **Keep** it, **Overwrite** it, or **Update** it with new insights?"*
   - **"Keep"**: STOP. Do not generate a new reflection.
   - **"Update"**: Read the existing file and append/merge new findings into it.
   - **"Overwrite"**: Proceed to generate a fresh reflection (the old file will be replaced).
3. **If the file does not exist:** Proceed directly to generation.

## Assessment

Conduct a self-assessment of the stage that just completed across these four dimensions:
1. **Violations & Showstoppers** — Skipped steps, blockers, misunderstood/missed requirements
2. **Process Friction / Workflow Gaps** — Unclear steps, missing docs, redundant work, unaccounted dependencies
3. **Tooling Friction / Missing Capabilities** — Tool limits, manual steps needing automation, missing integrations, context-gathering inefficiencies
4. **Delays, Confusion, or Inefficiencies** — Where progress stalled, misunderstandings, rework, communication gaps

## Root Cause Analysis

For each friction point identified:
- Explain **why** it happened (first-principles thinking)
- Identify the **incorrect assumption** that led to it
- Highlight the **process gap** that allowed it
- Classify as **one-time** or **systemic**

## Actionable Improvements

Propose improvements across three categories:

| Category | Examples |
|----------|----------|
| **Workflow / Process** | Stage flow changes, new checks/gates, documentation updates, simplification |
| **Tooling** | New tools, automation, integrations |
| **Skill / Knowledge** | Missing training, rules/persona updates, new reusable skills |

## Prioritization

Classify each improvement:
- 🔴 **Critical** — must fix immediately
- 🟠 **High** — strong impact, fix soon
- 🟡 **Medium** — moderate benefit
- 🔵 **Low** — minor improvement

## Output

Generate the reflection document using the template at `.ai/agents/shared/reflect-adapt-output-template.md` as a structural guide.

Save to: `{outputFile}`

### Completion Criteria
- [ ] Output file checked for prior existence; user choice respected
- [ ] All four friction dimensions assessed
- [ ] Root causes identified with classification (one-time / systemic)
- [ ] Improvements proposed and prioritized
- [ ] Action items and lessons learned captured

## Notes
- Be concise but specific
- Focus on actionable insights, not generic observations
- Prefer systemic fixes over one-off patches
- Avoid repeating obvious facts — prioritize learning value
