# Workflow & Status

This document defines the development workflow, and commit practices.

## Agent Instructions
### You must first ensure that the "AGENTS.md" file is loaded into your memory.

### You must then collect all user inputs from the `User Input` section below. For each bullet point:
1. ASK the user to provide the value for the **Key** (bolded text before the colon)
2. Wait for the user's response for each key individually
3. Store the response as the value for that Key
4. Do not proceed with the main task until all User Input values are collected.
   Once stored, you can reference values by their Key name in later steps.

### User Input:

- **Work Item File**: "{work_item_file}"

## Development Process

Every work item follows a structured stage process to ensure quality, consistency, and continuous improvement.
The coding assistant and user must both understand and follow this process rigorously.

* Note:Only the first stage (PLAN) is implemented.
### Process Overview

1. **PLAN**: Analyze the work item, break it down into clear steps, and create a detailed implementation plan. This stage focuses on understanding the requirements and designing a solution before writing any code.
2. **BUILD & ASSESS**: (Not implemented yet.)  Implement the solution according to the plan, then assess the implementation against the requirements and coding standards. This stage emphasizes writing clean, maintainable code and verifying that it meets the specified criteria.
3. **REFLECT & ADAPT**: (Not implemented yet) After implementation, reflect on the process and outcome. Identify what went well, what could be improved, and adapt future plans and practices based on these insights. This stage promotes continuous learning and improvement.

### Stage Definitions

#### Stage 1: PLAN

On this stage you read and analyze the {work_item_file}. Do not write any code yet. Instead, break down the work item into clear, actionable steps. 
Create a detailed implementation plan that outlines how you will approach the task, 
what components you will need to create or modify, and how you will ensure that the solution meets the requirements.

Steps:
1. Extract the values of {ticket_num} and {work_item_type} from the "Metadata" section.
2. Read the  "Story" & acceptance criteria from {work_item_type}
3. Identify required file changes across layers:
   - Domain (Entities, Interfaces)
   - Application (DTOs, Validators, Interfaces)
   - Infrastructure (Repositories, DbContext)
   - API (Controllers, Requests/Responses)
4. Save Execution Plan
   - Create the execution plan document. If the plan already exists, ask me if i want to update it with any new insights or overwrite its contents.
   - Save to the ".ai/memory/episodic" directory
   - Format: `{work_item_type}//{ticket_num}-{feature-name}.plan.md`
   - Include:
     - Story summary
     - Acceptance criteria (Given-When-Then)
     - Test strategy
     - File change list
     - implementation details
     - Implementation order
     - All the assumptions made during planning. For each assumption, include a justification on the logic you used to make this assumption.
     - All the questions that need to be answered before implementation if there is any ambiguity in the work item
