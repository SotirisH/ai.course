# Skill: Parse Work Item Metadata

## Description

Reads the `## Metadata` section of a work item file (e.g., a story/feature markdown file) and extracts `ticket_num`, `feature_name`, and `work_item_type` into variables for downstream use.

## Trigger Phrases

- "parse metadata"
- "extract metadata"
- "read work item metadata"
- "/parse-metadata"

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `$workItemFile` | Yes | Path to the work item markdown file (e.g., `docs/002_customers.story.md`) |

## Variables Set

| Variable | Description | Example |
|----------|-------------|---------|
| `$ticket_num` | The ticket number extracted from metadata | `001` |
| `$feature_name` | The feature name extracted from metadata | `Customer Management` |
| `$work_item_type` | The work item type extracted from metadata | `feature` |

## Procedure

### 1. Verify the work item file exists
### 2. Read the File
### 3. Locate the Metadata Section named `## Metadata`
### 4. Extract Values by parsing each key-value pair from the metadata block
### 5. Report Results

### 6. Use the Variables

The variables `{ticket_num}`, `{feature_name}, and `{work_item_type} are now available for downstream tasks (e.g., routing to a planner, coder, or test agent).

## Error Handling

- If the file path does not exist, the skill stops with an error.
- If the `## Metadata` section is missing entirely, the skill stops with an error.
- If a specific key (`ticket_num`, `feature_name`, or `work_item_type`) is missing from the metadata block, it is gracefully set to an empty string (no error).

## Example Usage

Given a file `docs/002_customers.story.md` with:

```markdown
# Story

As an administrator, I want to be able to manage customers in the system

## Metadata

work_item_type: feature
ticket_num: 001
feature_name: Customer Management
```

After running this skill:

- `{work_item_type}` = `feature`
- `{ticket_num}` = `001`
- `{feature_name}` = `Customer Management`
