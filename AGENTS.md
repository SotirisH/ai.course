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
