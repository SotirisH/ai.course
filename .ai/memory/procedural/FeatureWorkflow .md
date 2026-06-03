# Workflow & Status
This document defines the development workflow, and commit practices.

# Agent Instructions
- You must first ensure that the "AGENTS.md" file is loaded into your memory.
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
Every work item follows a structured stage process to ensure quality, consistency, and continuous improvement.
The coding assistant and user must both understand and follow this process rigorously.

## Process Overview
1. **PLAN**: Analyze the work item, break it down into clear steps, and create a detailed implementation plan. This stage focuses on understanding the requirements and designing a solution before writing any code.
2. **IMPLEMENT**: Read the plan produced in Stage 1 and implement the feature in code, following the project's coding standards and architecture rules.

### Stage Definitions
These are the stages you need to follow in order to implement a feture. It is IMPORTANT to ask the user to review the output at the end of each stage. 
You proceed to the next stage only if you have the explicit user's approval.

# Stage 1: PLAN
Delegate planning to the **Planner** agent. Use `run_subagent` with `agentName: "planner"` and pass `workItemFile:{work_item_file}` in the task.

The Planner agent will:
- Extract metadata (`ticket_num`, `feature_name`, `work_item_type`) and check for existing plans
- Analyze the work item story & acceptance criteria
- Identify required file changes across Domain, Application, Infrastructure, and API layers
- Produce a **Plan Document** saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/{ticket_num}-{feature-name}.plan.md`
- Produce a **Reflect & Adapt Document** saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/plan.reflections.md`

After the Planner agent completes, review its output with the user. Only proceed to the next stage with the user's explicit approval.

# Stage 2: IMPLEMENT
Delegate implementation to the **C#Coder** agent. Use `run_subagent` with `agentName: "C#Coder"` and pass `implementationPlan:.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/{ticket_num}-{feature-name}.plan.md` in the task.

The C#Coder agent will:
- Analyze the implementation plan for completeness and clarity
- Implement the feature across all required layers (Domain, Application, Infrastructure, API)
- Produce a **Compliance Checklist** saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/compliance-checklist.md`
- Produce a **Reflect & Adapt Document** saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/Implementation.reflections.md`

After the C#Coder agent completes, review its output with the user. Only proceed to the next stage with the user's explicit approval.
