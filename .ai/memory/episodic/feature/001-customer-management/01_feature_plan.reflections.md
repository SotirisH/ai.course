# Reflection: Planning Stage - Customer Management

## Metadata
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Feature Name**: Customer Management
- **Stage Reflected**: Planning
- **Date**: 2026-06-25

## Friction Encountered

### Violations & Showstoppers
- None. All planning steps were completed in order. The pre-scaffold detection confirmed no existing customer files — clean slate for implementation.

### Instructional Contradictions & Documentation Bugs
- **Ticket number discrepancy**: The work item file is named `docs/002_customers.story.md` but the metadata inside declares `ticket_num: 001`. The agent used `001` (from metadata) as instructed, but this mismatch between filename prefix and metadata field could cause confusion. Flagged as Q4 in the plan.
- **Parameter naming divergence**: The user passed the work item file as `implementation_plan_file` rather than the expected `{work_item_file}` parameter name. The agent correctly interpreted the intent, but the parameter naming is inconsistent between the workflow caller and the planner agent.

### Process Friction / Workflow Gaps
- **Reference implementation study overhead**: Reading 15+ reference files across 4 layers was essential for pattern fidelity but is a recurring cost. A "pattern reference card" summarizing the key conventions could reduce this.
- **Template is Handlebars-like**: The plan template (`plan-template.md`) uses `{{#each}}` / `{{SECTION_NUM}}` syntax. The agent must mentally translate this to markdown, which works but adds cognitive overhead and risks format drift.
- **Directory vs file confusion**: Calling `list_dir` on a file path (`IApplicationRepository.cs`, `ApplicationPersistenceMappingExtensions.cs`) returned "not found" errors that could be misinterpreted. The tool treats files and directories differently, and the error message doesn't distinguish between "path doesn't exist" and "path exists but is a file."

### Tooling Friction / Missing Capabilities
- **`grep_search` returned zero results for "ToDto|CreateApplicationDto"**: This was likely due to the regex pipe character needing escaping or the search scope being limited. Had to fall back to `file_search` with glob patterns to find mapping files.
- **Sequential file reads are slow**: Each reference file required a separate `read_file` call. A batch-read capability for small files in the same directory would speed up the pattern study phase considerably.

### Delays, Confusion & Inefficiencies
- **Branch already existed**: The `feature/001-customer-management` branch was already checked out from a prior run. This was handled correctly (just switched to it) but suggests the prior plan was overwritten mid-workflow.
- **CRLF warning on commit**: Git warned about CRLF→LF conversion on the plan file. Minor, but indicates Windows line-ending settings may need attention in `.gitattributes`.

## Root Cause Analysis

- **Friction**: Ticket number mismatch (001 vs 002)
  - **Root Cause**: The story file was likely created from a template or renumbered, but the internal metadata wasn't updated.
  - **Underlying Assumption**: That the filename prefix and metadata ticket_num would always agree.
  - **Process Gap**: No validation step in the workflow that cross-checks filename prefix against metadata.

- **Friction**: Parameter naming divergence (`implementation_plan_file` vs `work_item_file`)
  - **Root Cause**: The FeatureWorkflow prompt likely passes parameters with different names than what the planner agent expects.
  - **Underlying Assumption**: That all agents in the workflow use the same parameter names.
  - **Process Gap**: No parameter contract validation between workflow and sub-agents.

- **Friction**: Reference implementation study is time-consuming
  - **Root Cause**: Every new feature requires re-reading the same reference files to confirm patterns.
  - **Underlying Assumption**: That agents need to re-discover patterns each time.
  - **Process Gap**: No concise "pattern reference card" or cached pattern summary. Systemic.

- **Friction**: `grep_search` returning zero results
  - **Root Cause**: The `|` pipe character in regex may need escaping, or the search index may not include files outside specific scopes.
  - **Underlying Assumption**: That `grep_search` with `isRegexp: true` would match patterns with pipe operators as expected.
  - **Process Gap**: Unknown — could be a tool behavior issue. One-time until root cause is confirmed.

## Proposed Improvements

### Workflow/Process Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Add a "filename prefix vs metadata ticket_num" cross-check to the planning stage | 🟡 Medium | Low | Medium |
| Create a **Pattern Reference Card** (one-page summary) capturing the key conventions from the reference implementation to reduce repeated file reads | 🟠 High | Medium | High |
| Standardize parameter names between FeatureWorkflow and planner agent (`work_item_file`) | 🟡 Medium | Low | Low |

### Tooling Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Use `file_search` with globs before `grep_search` with complex regex to avoid silent zero-result failures | 🔵 Low | Low | Low |
| Batch-read support for multiple small files in the same directory | 🟡 Medium | Unknown | High |

### Skill/Knowledge Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Document the `grep_search` regex escaping requirements (pipe, parentheses) in AGENTS.md tooling section | 🔵 Low | Low | Medium |
| Add a note in AGENTS.md that `list_dir` on a file path returns "not found" — recommend `Test-Path` first or use `read_file` | 🔵 Low | Low | Low |

## Action Items
- [ ] Resolve Q4 (ticket_num: 001 vs filename 002) with the user during QA session
- [ ] Consider creating a Pattern Reference Card in `.ai/rules/` summarizing the ApplicationManagement reference patterns
- [ ] Fix parameter name in FeatureWorkflow to use `work_item_file` consistently

## Time Spent (Actual)
- Loading context files (AGENTS.md, persona, architecture, tech-stack, coding-standards, SKILL, work item, template): ~8 files
- Pre-scaffold detection scan: 1 terminal command
- Reading reference implementation files (15 files across 4 layers): ~20 tool calls
- Generating plan document: 1 create_file call
- Commit and branch management: 2 terminal commands
- Reflection generation: 1 create_file call
- Total: ~30 minutes

## Lessons Learned
- The reference implementation study, while time-consuming, is essential — every file pattern was faithfully replicated in the plan.
- The ticket_num mismatch between filename and metadata is a recurring risk that should be caught early in the workflow. A simple cross-check would prevent confusion.
- Pre-scaffold detection is lightweight and valuable — it confirmed a clean slate and avoided unnecessary "EDIT" annotations in the file change list.
- The plan template works well as a structural guide even though it uses Handlebars-like syntax — the key is to preserve the section structure rather than the template markup.
