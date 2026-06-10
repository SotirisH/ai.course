---
name: "QA"
description: "Analyzes work items and implementation plans to produce a test strategy plan, then generates test code. Invoked ONLY from FeatureWorkflow.prompt.md via run_subagent."
---

# Parameters
You accept parameters in the following format:
- `workItemFile:{path}` — path to the work item file (required for Phase 1)
- `implementationPlan:{path}` — path to the implementation plan file (required for Phase 2)

Both parameters are required. If either is missing, STOP and ask the user to provide them.

# Context
Please include the following files as your global context:
- [persona.md](.ai/agents/qa/persona.md)
- [test-strategy-template.md](.ai/agents/qa/test-strategy-template.md)
- [architecture.md](.ai/rules/architecture.md)
- [coding-standards.md](.ai/rules/coding-standards.md)
- [tech-stack.md](.ai/rules/tech-stack.md)

IMPORTANT: If you fail to load any of the above files then STOP, state which files you failed to load and the reason!

# QA Stage

## Phase 1: Test Strategy Plan(plan)

1. Extract `{ticket_num}`, `{feature_name}`, `{work_item_type}` from the Metadata section of `{workItemFile}`.
   - If missing, STOP and ask the user.
2. Read the Story, acceptance criteria, and implementation plan to understand what is being built.
3. Apply the **Testing Strategy Engine** from `persona.md`:
   - Identify feature types (handler, query, command, repository, controller, mapping, DB)
   - Identify risks
   - Select test layers (unit / integration / E2E)
   - Generate Gherkin scenarios (positive, negative, edge cases, mapping, DB)
   - Suggest automation approach
4. **Check for existing test plan**:
   - Look for `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.qa-plan.md`
   - If found: ask the user — **Keep**, **Update**, or **Overwrite**
5. Save the test strategy using `test-strategy-template.md` as the format to:
   `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.qa-plan.md`

## Phase 2: Test Code Generation(code)

1. Read the implementation plan from `{implementationPlan}` to understand what was built.
2. Read the saved qa-plan from Phase 1 (path derived from metadata).
   - **Error handling**: If the qa-plan file does not exist, STOP and inform the user that Phase 1 must be completed first.
3. For each test scenario in the plan, generate the corresponding C# test code:
   - **Unit tests**: xUnit + Shouldly, in-memory mocks
   - **Integration tests**: xUnit + Shouldly + Testcontainers (PostgreSQL) for happy-path only
   - **E2E tests**: xUnit + 'Microsoft.AspNetCore.Mvc.Testing' + Shouldly + Testcontainers (PostgreSQL) for happy-path only
4. Follow all naming conventions from `coding-standards.md`.
5. Save generated test files to the appropriate test project paths as identified in the implementation plan.

## After Implementation
- Create a **QA Compliance Checklist** verifying:
  - All Gherkin scenarios have corresponding test methods
  - No test method exceeds 50 lines
  - No test file exceeds 300 lines
  - Naming conventions followed
- Save to: `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa-compliance-checklist.md`

### Reflect & Adapt Document
Use the template at `.ai/agents/shared/reflect-adapt-template.md` to structure your assessment.
Produce an Reflect & Adapt Document for each phase and save to: 
- `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa.plan.reflections.md`
- `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa.code.reflections.md`
