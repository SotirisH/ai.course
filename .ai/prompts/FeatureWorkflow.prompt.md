# Feature Workflow 
This document defines the development workflow, and commit practices.

# Agent Instructions
- You must first ensure that the "AGENTS.md" file is loaded into your memory.
- You must then collect all user inputs from the `User Input` section below. Use the `ask_questions` tool to collect the required data.

# User Input:
- **Work Item File**:`{work_item_file}`

# Development Process
Every work item follows a structured 4-stage process to ensure quality, consistency, and continuous improvement.
The coding assistant and user must both understand and follow this process rigorously.

## PreProcess Overview
Before Stage 1 begins, extract metadata from the work item file:
1. Read `{work_item_file}` and parse the `## Metadata` section for:
    - `ticket_num`
    - `feature_name`
    - `work_item_type`
2. Derive `{feature_name_kebab}`: lowercase + hyphens for spaces
*Error handling*: If Metadata section is missing or values cannot be extracted or the values are empty then ⛔STOP and ask the user to provide them.

## Process Overview
1. **FEATURE PLAN**: Analyze the work item, break it down into clear steps, and create a detailed implementation plan. No code is written.
2. **FEATURE IMPLEMENTATION**: Read the plan from Stage 1 and implement the feature in code across all layers.
3. **TEST PLANNING**: Analyze the work item and implementation plan to produce a complete test strategy with Gherkin scenarios and a mapped test file list. No test code is written.
4. **TEST IMPLEMENTATION**: Read the test plan from Stage 3 and implement all test scenarios as C# test code.

**Stage Definitions**
Each stage is delegated to a dedicated agent via `run_subagent`. After each stage completes, present the output to the user and ask for **explicit approval** before proceeding to the next stage.

---
### Stage 1: FEATURE PLAN
**Agent**: `Planner`
Delegate planning to the **Planner** agent. Use `run_subagent` with `agentName: "planner"` and pass  in the task:
- `implementation_plan_file:{workspace_root}/{work_item_file}`
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
