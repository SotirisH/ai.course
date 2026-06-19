# Reflection: Planning Stage - Customer Management

## Metadata
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Feature Name**: Customer Management
- **Stage Reflected**: Planning
- **Date**: 2026-06-19

## Friction Encountered

### Violations & Showstoppers
- No process violations. All planning steps (existing plan check, pre-scaffold detection, spec consistency check, file change list, branch creation) were followed.

### Instructional Contradictions & Documentation Bugs
- **Entity naming contradiction**: `architecture.md` line 146 says entity classes should use "Entity name + 'Entity'" (e.g., `Orders`, `Products`). However, the examples on lines 146-147 (`Orders`, `Products`) do NOT include an "Entity" suffix. The existing codebase uses `Application` (not `ApplicationEntity`). The instruction text conflicts with both its own examples and the actual codebase.
- **Story file numbering mismatch**: The work item file is named `docs/002_customers.story.md` but the internal `ticket_num` metadata is `001`. This inconsistency between the file name and metadata could cause confusion for automation scripts that parse file names.

### Process Friction / Workflow Gaps
- **Extensive file reading for pattern discovery**: 15+ files needed to be read to understand the existing patterns (ApplicationManagement feature). No "reference feature" summary document exists — each plan requires re-discovering the same conventions.
- **No entity listing in AGENTS.md**: The `list_dir` of relevant folders like `Entities/`, `Repositories/` etc. was needed to understand what already exists.

### Tooling Friction / Missing Capabilities
- **`list_dir` silently fails on valid paths**: The `list_dir` tool returned `ENOENT` for `Ai.Api\Controllers` and `Ai.Api\Models\Requests`, even though `Get-ChildItem` confirmed those directories exist and contain files. This required a fallback to `run_in_terminal` for every directory listing, adding overhead.
- **No batch directory listing tool**: Each directory required a separate `list_dir` or `run_in_terminal` call. A "list multiple directories" capability would reduce round-trips.

### Delays, Confusion & Inefficiencies
- **Brief confusion over ticket number**: The story file name (`002_customers.story.md`) vs metadata `ticket_num: 001` discrepancy caused a moment of cross-checking. The user also specified the plan path with `001`, confirming `001` is the correct ticket number.
- **Sequential file discovery**: Reading files one at a time to understand the pattern added latency. Several could have been read in parallel if they were identified earlier.

## Root Cause Analysis

- **Friction**: Entity naming rule contradiction in architecture docs
  - **Root Cause**: The architecture document was likely updated after the initial code was written, or the code didn't follow the written standard. The examples were probably copied from existing code without being updated to match the text rule.
  - **Underlying Assumption**: The architecture docs would always match the codebase.
  - **Process Gap**: No automated linting/sync between architecture docs and codebase naming.
  - **Classification**: Systemic

- **Friction**: `list_dir` silent failures on valid paths
  - **Root Cause**: Unknown tool implementation issue — possibly encoding, permission, or path resolution edge case in the `list_dir` implementation.
  - **Underlying Assumption**: `list_dir` would work reliably for all directory paths.
  - **Process Gap**: No fallback mechanism in the planner instructions for when `list_dir` fails.
  - **Classification**: One-time (tool-specific bug)

- **Friction**: Large number of file reads needed for pattern discovery
  - **Root Cause**: No "reference feature" or "pattern catalogue" document exists. Every planner agent must reverse-engineer conventions from raw code.
  - **Underlying Assumption**: Reading source code is sufficient for pattern discovery.
  - **Process Gap**: Missing documentation artifact for feature conventions.
  - **Classification**: Systemic

## Proposed Improvements

### Workflow/Process Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Create a "Convention Reference" doc that summarizes the existing feature pattern (ApplicationManagement) — file list, naming conventions, mapping patterns, validation patterns | 🟠 High | Low | High |
| Fix architecture.md line 146 to match actual codebase convention (remove "Entity" suffix instruction or clarify it applies differently) | 🟡 Medium | Low | Medium |
| Add a step to the planner workflow to read an existing feature as a "canonical reference" before listing files, reducing redundant discovery | 🟡 Medium | Low | Medium |

### Tooling Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Investigate and fix `list_dir` ENOENT bug for paths that `Get-ChildItem` confirms exist | 🟠 High | Unknown | High |
| Add `run_in_terminal` with `Get-ChildItem -Recurse` as a standard fallback in planner instructions when `list_dir` fails | 🟡 Medium | Low | Medium |

### Skill/Knowledge Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Document the "batch read" pattern: when discovering a new codebase, use `run_in_terminal` to `Get-ChildItem` all relevant directories first, then batch `read_file` calls for key files | 🟡 Medium | Low | Medium |
| Add a planner persona instruction to check for story-file-name vs metadata ticket_num consistency and alert the user | 🔵 Low | Low | Low |

## Action Items
- [ ] Fix `architecture.md` line 146: entity naming rule contradicts its own examples and the codebase
- [ ] Create `.ai/rules/convention-reference.md` summarising the ApplicationManagement feature pattern
- [ ] Rename `docs/002_customers.story.md` to `docs/001_customers.story.md` for consistency, or clarify the numbering scheme

## Time Spent (Actual)
- File discovery and pattern analysis: ~15 file reads, 6 terminal commands
- Plan document authoring: 1 create_file
- Reflection authoring: self-assessment + this document
- Total: ~25 minutes (agent time)

## Lessons Learned
- Always cross-validate `list_dir` results with `Get-ChildItem` when the tool returns ENOENT — the directories may actually exist.
- When the architecture doc's text contradicts its own code examples, trust the examples and the existing codebase.
- Reading a complete "reference feature" (ApplicationManagement) upfront — all files, not just the folder listing — saves many round-trips and catches edge cases early.
- The story file naming convention (002 vs 001) should be standardised to avoid confusion between file sequence numbers and ticket numbers.
