# Workflow & Status
This document defines the development workflow, and commit practices.

# Agent Instructions
- You must first ensure that the "AGENTS.md" file is loaded into your memory.
- **Cache the workspace root path**: The workspace root is the absolute path shown in `<workspace_info>`.
Store this as `{workspace_root}` and use it to resolve ALL relative paths to absolute paths before passing them to any tool 
(especially `run_subagent`). For example, if `{workspace_root}` is `I:\GitRepo\ai.course` and a relative path is `.ai/memory/foo.md`, 
resolve it to `{workspace_root}/.ai/memory/foo.md`.
- You must then collect all user inputs from the `User Input` section below. Use the `ask_questions` tool to collect the required data. 
For each bullet point:
  1. ASK the user to provide the value for the **Key** (bolded text before the colon)
  2. Wait for the user's response for each key individually
  3. Bolded text before colon = **Key** to collect from user
  4. Text in curly braces `{variable_name}` = variable to store the Key's value for later use
  5. Do not proceed further until all User Input values are collected. Once stored, you can reference values by their Key name in later steps.

# User Input:
- **Work Item File**:`{work_item_file}`

# Development Process
Every work item follows a structured 4-stage process to ensure quality, consistency, and continuous improvement.
Each stage is owned by a dedicated agent running on its own LLM model.
The coding assistant and user must both understand and follow this process rigorously.

## Process Overview
1. **FEATURE PLAN**: Analyze the work item, break it down into clear steps, and create a detailed implementation plan. No code is written.
2. **FEATURE IMPLEMENTATION**: Read the plan from Stage 1 and implement the feature in code across all layers.
3. **TEST PLANNING**: Analyze the work item and implementation plan to produce a complete test strategy with Gherkin scenarios and a mapped test file list. No test code is written.
4. **TEST IMPLEMENTATION**: Read the test plan from Stage 3 and implement all test scenarios as C# test code.

### Stage Definitions
Each stage is delegated to a dedicated agent via `run_subagent`. After each stage completes, present the output to the user and wait for **explicit approval** before proceeding to the next stage.

---

# Stage 1: FEATURE PLAN
**Agent**: `Planner` | **Model**: `deepseek/deepseek-v4-pro`

Delegate planning to the **Planner** agent. Use `run_subagent` with `agentName: "planner"` and pass `workItemFile:{workspace_root}/{work_item_file}` in the task.

The Planner agent will:
- Extract metadata (`ticket_num`, `feature_name`, `work_item_type`) and check for existing plans
- Analyze the work item story & acceptance criteria
- Identify required file changes across Domain, Application, Infrastructure, and API layers
- Produce a **Plan Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.plan.md`
- Produce a **Reflect & Adapt Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/plan.reflections.md`

After the Planner agent completes, **extract the metadata variables** by reading the plan file at `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.plan.md`:
- Read the `## Metadata` section to capture `{ticket_num}`, `{feature_name}`, and `{work_item_type}` values.
- Store these values for use in all subsequent stages.

⛔ **STOP — Present Stage 1 output to the user. Only proceed to Stage 2 with explicit user approval.**

---

# Stage 2: FEATURE IMPLEMENTATION
**Agent**: `C#Coder` | **Model**: `deepseek/deepseek-v4-flash`

Delegate implementation to the **C#Coder** agent. Use `run_subagent` with `agentName: "C#Coder"` and pass `implementationPlan:{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.plan.md` in the task.

The C#Coder agent will:
- Analyze the implementation plan for completeness and clarity
- Implement the feature across all required layers (Domain, Application, Infrastructure, API)
- Produce a **Compliance Checklist** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/compliance-checklist.md`
- Produce a **Reflect & Adapt Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/Implementation.reflections.md`

⛔ **STOP — Present Stage 2 output to the user. Only proceed to Stage 3 with explicit user approval.**

---

# Stage 3: TEST PLANNING
**Agent**: `TestPlanner` | **Model**: `deepseek/deepseek-v4-pro`

Delegate test planning to the **TestPlanner** agent. Use `run_subagent` with `agentName: "TestPlanner"` and pass:
- `workItemFile:{workspace_root}/{work_item_file}`
- `implementationPlan:{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.plan.md`

The TestPlanner agent will:
- Analyze the work item requirements and the full implementation plan
- Identify feature types, risks, and applicable test layers
- Generate Gherkin scenarios (positive, negative, edge cases, mapping, DB)
- Map every scenario to a test file, class name, and method name
- Produce a **Test Plan Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.qa-plan.md`
- Produce a **Reflect & Adapt Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa.plan.reflections.md`

⛔ **STOP — Present Stage 3 output to the user. Only proceed to Stage 4 with explicit user approval.**

---

# Stage 4: TEST IMPLEMENTATION
**Agent**: `TestCoder` | **Model**: `deepseek/deepseek-v4-flash`

Delegate test code generation to the **TestCoder** agent. Use `run_subagent` with `agentName: "TestCoder"` and pass:
- `testPlan:{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/{ticket_num}-{feature_name}.qa-plan.md`

The TestCoder agent will:
- Read the test plan and derive the implementation plan path from its `## Metadata` section
- Implement all Gherkin scenarios as C# test code using the Test File Map from the test plan
- Produce a **QA Compliance Checklist** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa-compliance-checklist.md`
- Produce a **Reflect & Adapt Document** saved to `{workspace_root}/.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}/qa.code.reflections.md`

⛔ **STOP — Present Stage 4 output to the user. The workflow is complete.`
