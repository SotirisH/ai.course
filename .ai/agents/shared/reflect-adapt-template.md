# Reflect & Adapt Document

1. Assess Friction Encountered

- Violations & Showstoppers
    - Identify skipped steps or process violations
    - List blockers that prevented progress
    - Highlight misunderstood or missed requirements

- Process Friction / Workflow Gaps
    - Identify unclear or ambiguous steps
    - Note missing or insufficient documentation
    - Highlight redundant or cumbersome workflow parts
    - Identify unaccounted dependencies between stages

- Tooling Friction / Missing Capabilities
    - Document tool limitations
    - Identify manual steps that could be automated
    - Note missing capabilities or integrations
    - Highlight inefficiencies in gathering context

- Delays, Confusion, or Inefficiencies
    - Identify where progress stalled
    - Note misunderstandings requiring clarification
    - Highlight rework caused by earlier decisions
    - Identify communication gaps

2. Identify Root Causes

For each issue:
- Explain why it happened (first-principles thinking)
- Identify incorrect assumptions
- Highlight process gaps that allowed it
- Classify as one-time or systemic

3. Propose Actionable Improvements

- Workflow / Process
    - Suggest stage or flow improvements
    - Add checks, gates, or validation steps
    - Recommend documentation updates
    - Simplify overly complex steps

- Tooling
    - Suggest new tools or enhancements
    - Propose automation opportunities
    - Recommend integrations

- Skill / Knowledge
    - Identify missing knowledge or training
    - Suggest updates to rules or personas
    - Propose new reusable skills

4. Prioritize Improvements

Classify each improvement:
- 🔴 Critical — must fix immediately
- 🟠 High — strong impact, fix soon
- 🟡 Medium — moderate benefit
- 🔵 Low — minor improvement

## Output

Generate a reflection document and save to: `.ai/memory/procedural/reflections/{work_item_type}-{feature_name}-{stage_name}.reflections.md`

## Output Format
Use the [reflect-adapt-output-template.md](reflect-adapt-output-template.md) as a template

## Notes
- Be concise but specific
- Focus on actionable insights, not generic observations
- Prefer systemic fixes over one-off patches
- Avoid repeating obvious facts — prioritize learning value
