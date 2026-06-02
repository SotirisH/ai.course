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

### Stage Definitions
These are the stages you need to follow in order to implement a feture. It is IMPORTANT to ask the user to review the output at the end of each stage. 
You proceed to the next stage only if you have the explicit user's approval.

# Stage 1: PLAN
On this stage you read and analyze the {work_item_file}. Do not write any code yet. Instead, break down the work item into clear, actionable steps.
Create a detailed implementation plan that outlines how you will approach the task,
what components you will need to create or modify, and how you will ensure that the solution meets the requirements.

## Steps
1. Extract the values of {ticket_num},{feature_name} and {work_item_type} from the "Metadata" section of {work_item_file}.
   - *Error handling*: If Metadata section is missing or values cannot be extracted, see [Error Handling](#error-handling) section.
2. **Check for existing plan file**. Steps:
   1. list_dir on `.ai/memory/episodic/{work_item_type}/`
   2. Filter results in code to find files matching `{ticket_num}*.plan.md` pattern
- If directory doesn't exist, no existing plans to look for existing plan files matching the pattern: `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name}.plan.md`
   - If a matching plan file exists:
     - Ask the user if they want to **Keep existing plan**, **Update with new insights**, or **Overwrite completely**
     - If user selects "Keep existing plan": Skip remaining PLAN steps. In Edit mode, verify the existing plan is committed to the feature branch. In Ask mode, output the existing plan as the response.
     - If user selects "Update" or "Overwrite": Proceed to step 3.
   - If no matching plan file exists: Proceed to step 3.
3. Read the "Story" & acceptance criteria from {work_item_type} in {work_item_file}
   - *Error handling*: If {work_item_type} is invalid or not found, ask the user to clarify.
4. Identify required file changes across layers:
   - Domain (Entities, Interfaces)
   - Application (DTOs, Validators, Interfaces)
   - Infrastructure (Repositories, DbContext)
   - API (Controllers, Requests/Responses)

## Output
Two files will be generated as output of this stage:
### **Output A**:Plan Document
- Create the execution plan document.
- Save the plan document to the appropriate location based on the current mode:
  - **In Edit mode**: Save to the ".ai/memory/episodic" directory, commit to feature branch.
  - **In Ask mode**: Output the plan as a response (skip file save and Git operations).
- Format: `{work_item_type}/{ticket_num}-{feature_name}.plan.md` (derive {feature_name} from the work item's story title, e.g., "Application Management" → "application-management")
- Generate the following sections:
  - Story summary
  - Acceptance criteria (Given-When-Then)
  - Test strategy and file changes identified.
    - In **Edit** mode: feature branch created and plan committed.
    - In **Ask** mode: plan output as response (file creation and Git operations skipped).
  - File change list
  - Implementation details
  - Implementation order
  - All the assumptions made during planning. For each assumption, include a justification on the logic you used to make this assumption.
  - All the questions that need to be answered before implementation if there is any ambiguity in the work item
**Completion Criteria:**
- **Edit Mode**:
  - [ ]  Test strategy and file changes identified
  - [ ]  Existing plan check completed
  - [ ]  Feature branch created (if not already active)
  - [ ]  Plan saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/{ticket_num}-{feature-name}.plan.md`
  - [ ]  Plan committed to feature branch

### **Output B**: Reflect & Adapt Document
* Assess the friction encountered during the workflow execution, including
  - Violations & Showstoppers
  - Process Friction/Workflow Gaps
  - Tooling Friction/Missing Capabilities
  - anything else that caused delays, confusion, or inefficiencies during the workflow execution.
* Identify Root Causes for any issues encountered.
* Idintify specific areas where the workflow could be improved, and propose actionable changes to address these issues. This promotes continuous learning and improvement.

Save your assessment in a document within `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/{stage_name}.reflections.md`.

**Completion Criteria:**
- **Edit Mode**:
  - [ ]  Reflection document saved to `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature-name}/` directory
  - [ ]  Reflection committed to feature branch
  - [ ]  Workflow/process improvements implemented and committed (if applicable)
