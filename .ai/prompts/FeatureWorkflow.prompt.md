# Feature Workflow 
This document defines the development workflow, and commit practices.

# Agent Instructions
- You must first ensure that the "[AGENTS.md](./AGENTS.md)" file is loaded into your memory.
- You must then collect all user inputs from the `User Input` section below. Use the `ask_questions` tool to collect the required data.
- The user input has the format `header`:{variable}. Use the `header` as the prompt question text.

# User Input:
- **Work item file**:`{work_item_file}`

# Development Process
Every work item follows a structured 4-stage process to ensure quality, consistency, and continuous improvement.
The coding assistant and user must both understand and follow this process rigorously.

## PreProcess Overview
Before Stage 1 begins, extract metadata from the work item file.
Derive `{feature_name_kebab}`: lowercase + hyphens for spaces from the {feature_name}
*Error handling*: If Metadata section is missing or values cannot be extracted or the values are empty then ⛔STOP and ask the user to provide them.

## Process Overview
1. **FEATURE PLAN**: Analyze the work item, break it down into clear steps, and create a detailed implementation plan. No code is written.
2. **FEATURE IMPLEMENTATION**: Read the plan from Stage 1 and implement the feature in code across all layers.
3. **TEST PLANNING**: Analyze the work item and implementation plan to produce a complete test strategy with Gherkin scenarios and a mapped test file list. No test code is written.
4. **TEST IMPLEMENTATION**: Read the test plan from Stage 3 and implement all test scenarios as C# test code.

**Stage Definitions**
Each stage is delegated to a dedicated agent via `run_subagent`. After each stage completes, stop the execution, present the output to the user and ask for **explicit approval** before proceeding to the next stage.

---
### Stage 1: FEATURE PLAN
**Steps**
1. Delegate planning to the **Planner** agent. Use `run_subagent` with `agentName: "planner"` and pass  in the task: `implementation_plan_file:{workspace_root}/{work_item_file}`
2. Step 1b: QA Session — Assumptions & Questions Review
   1. Read the plan file at `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/{ticket_num}-{feature_name_kebab}.plan.md`.
   2. Locate the `## Assumptions` table and the `## Questions for Clarification` table.
   3. For **each assumption**, present it to the user and ask for their decision. Capture the response into the `User Decision` column:
      - If the user confirms the assumption, write: `✅ Confirmed — <user feedback>`
      - If the user rejects it, write: `❌ Rejected — <user correction>`
      - If the user modifies it, write: `🔄 Modified — <user modification>`
   4. For **each question**, present it to the user and ask for their answer. Capture the response into the `User Decision` column.
   5. After all assumptions and questions are reviewed, present a summary of all decisions to the user for final confirmation.
   6. If the user requests changes to any decision, re-prompt and update the column until confirmed.
   7. Save the updated plan file with all `User Decision` columns filled.

3. Capture Planner Learnings and pass `$planFile`: `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/{ticket_num}-{feature_name_kebab}.plan.md`
After all three steps  are complete, present the output to the user and ask for **explicit approval** before proceeding to Stage 2.
---

### Stage 2: FEATURE IMPLEMENTATION
**Agent**: `C#Coder`
Delegate implementation to the **C#Coder** agent. Use `run_subagent` with `agentName: "C#Coder"` and pass  in the task:
- `implementation_plan_file:{workspace_root}/{work_item_file}`
- `stage`:"02_feature_implementation"

---
### Stage 3: TEST PLANNING
**Agent**: `TestPlanner`
Delegate test planning to the **TestPlanner** agent. Use `run_subagent` with `agentName: "TestPlanner"` and pass:
- `workItemFile:{workspace_root}/{work_item_file}`
- `implementationPlan:{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/{ticket_num}-{feature_name_kebab}.plan.md`
---

### Stage 4: TEST IMPLEMENTATION
**Agent**:  `C#Coder`

Delegate test code generation to the **TestCoder** agent. Use `run_subagent` with `agentName: "TestCoder"` and pass:
`testPlan:{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/{ticket_num}-{feature_name_kebab}.qa-plan.md`

The Coder agent will:
- Implement every Gherkin scenario from the test plan as a C# test method.
- Follow all naming conventions from `coding-standards.md` exactly.
- Write clean, maintainable test code — no shortcuts, no skipped scenarios.
- Use the correct test framework and tooling for each test layer as specified below.
- Do NOT redesign or re-strategize — execute the plan as written. If the plan is ambiguous, STOP and ask.
- Produce a **QA Compliance Checklist** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/qa-compliance-checklist.md`
- Produce a **Reflect & Adapt Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/qa.code.reflections.md`
