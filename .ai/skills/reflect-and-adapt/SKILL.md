---
name: "Reflect & Adapt"
description:  Generates a structured post-stage reflection document using a 5-dimension friction assessment, root cause analysis, and prioritized improvements.
    Triggered when any of these phrases appear: "reflect-and-adapt", "reflect & adapt", "/reflect-and-adapt", "Run Reflect & Adapt" or anything related to "Reflect & Adapt" 
---

# Skill: Reflect & Adapt

## Parameters
- $outputFile
  - description: "The path to the work item file that has the details about the feature we want to implement"
  - required: true
  - fallback: If it is not passed or it has an empty valuse then ask the user to provide it.  

## Pre-Flight: Output File Check

1. Use `run_in_terminal` to check if the output file exists:
   ```
   Test-Path "$outputFile"
   ```
2. **If the file exists:**
   - Read the existing file using `read_file` and present a brief summary to the user.
   - Ask the user:
     > *"A reflection file already exists at `$outputFile`. Do you want to **Keep** it, **Overwrite** it, or **Update** it with new insights?"*
   - **"Keep"**: STOP. Do not generate a new reflection.
   - **"Update"**: Read the existing file and append/merge new findings into it.
   - **"Overwrite"**: Proceed to generate a fresh reflection (the old file will be replaced).
3. **If the file does not exist:** Proceed directly to generation.

## Assessment

Conduct a self-assessment of the stage that just completed across these **five** dimensions:
1. **Violations & Showstoppers** — Skipped steps, blockers, misunderstood/missed requirements
2. **Instructional Contradictions & Documentation Bugs** — Conflicting instructions within agent definitions, parameter mismatches between caller and callee, broken file references, missing dependencies, or ambiguous guidance that forced the agent to guess. For each deviation between what the instructions said to do and what was actually done, trace the deviation back to the specific source of the conflict (e.g., "line X said A but line Y said B").
3. **Process Friction / Workflow Gaps** — Unclear steps, missing docs, redundant work, unaccounted dependencies
4. **Tooling Friction / Missing Capabilities** — Tool limits, manual steps needing automation, missing integrations, context-gathering inefficiencies
5. **Delays, Confusion, or Inefficiencies** — Where progress stalled, misunderstandings, rework, communication gaps

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

Generate the reflection document using the template at  [reflect-adapt-output-template.md](reflect-adapt-output-template.md) as a structural guide.

Save to: `$outputFile`

### Completion Criteria
- [ ] Output file checked for prior existence; user choice respected
- [ ] All five friction dimensions assessed
- [ ] Root causes identified with classification (one-time / systemic)
- [ ] Improvements proposed and prioritized
- [ ] Action items and lessons learned captured

## Notes
- Be concise but specific
- Focus on actionable insights, not generic observations
- Prefer systemic fixes over one-off patches
- Avoid repeating obvious facts — prioritize learning value
