# Agent Instructions
## Tooling
- The current PowerShell version 7.6.2. If you need to run PowerShell commands, please use this version.
  PowerShell uses `;` for command chaining, not `&&` or `||`. For example, to run two commands sequentially, you would use command1; command2.

##  Skill / Knowledge
- Prefer `list_dir` `over file_search` for known locations
  e.g.  When the folder structure is well-known (e.g., Application/Mappings/), `list_dir` followed by targeted `read_file` is more reliable than `file_search` with glob patterns.

## File Path Convention
- **Cache the workspace root path**: The workspace root is the absolute path shown in `<workspace_info>`.
  Store this as `{workspace_root}` and use it to resolve ALL relative paths to absolute paths before passing them to any tool
  (especially `run_subagent`). For example, if `{workspace_root}` is `I:\GitRepo\ai.course` and a relative path is `.ai/memory/foo.md`,
  resolve it to `{workspace_root}/.ai/memory/foo.md`.
- When an agent receives a relative path like `.ai/agents/planner/persona.md`, resolve it against `{workspace_root}`.
- When an agent receives a parameter like `workItemFile:{path}`, if the path is relative, resolve it against `{workspace_root}` before using it.

# Available Agents

The following agents are available for use in the Feature Workflow (`FeatureWorkflow.prompt.md`).
Each agent has a dedicated role, persona, and LLM model. They must be invoked via `run_subagent`.

| Agent Name   | File                                  | Model                      | Role                                                                 |
|--------------|---------------------------------------|----------------------------|----------------------------------------------------------------------|
| `planner`    | `.ai/agents/planner.agent.md`         | `deepseek/deepseek-v4-pro`   | Stage 1 — Feature Plan: analyzes work items, produces implementation plan |
| `C#Coder`    | `.ai/agents/C#Coder.agent.md`         | `deepseek/deepseek-v4-flash` | Stage 2 — Feature Implementation: implements the feature in C# code  |
| `TestPlanner`| `.ai/agents/testplanner.agent.md`     | `deepseek/deepseek-v4-pro`   | Stage 3 — Test Planning: produces test strategy, Gherkin scenarios, and test file map |
| `TestCoder`  | `.ai/agents/testcoder.agent.md`       | `deepseek/deepseek-v4-flash` | Stage 4 — Test Implementation: implements all test scenarios as C# test code |
