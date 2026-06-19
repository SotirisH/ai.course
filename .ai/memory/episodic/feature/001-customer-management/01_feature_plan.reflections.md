# Reflection: Planning Stage - Customer Management

## Metadata
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Feature Name**: Customer Management
- **Stage Reflected**: Planning
- **Date**: 2026-06-19

## Friction Encountered

### Violations & Showstoppers
- None. All planning steps were completed successfully.

### Instructional Contradictions & Documentation Bugs
- **Reflect & Adapt invocation mismatch**: The planner agent instructions (`.ai/agents/planner.agent.md`) direct invoking the Reflect & Adapt skill via `run_subagent` with `agentName: "reflect-and-adapt"`. However, `reflect-and-adapt` is defined as a **skill** (`.ai/skills/reflect-and-adapt/SKILL.md`), not an agent. The `run_subagent` tool only accepts registered agent names (`planner`, `C#Coder`, `TestPlanner`, `TestCoder`). This forced manual execution of the skill's instructions instead of agent delegation.
  - **Source**: planner.agent.md line "Invoke the **Reflect & Adapt** skill) with:" vs AGENTS.md agent table which lists only 4 agents.

### Process Friction / Workflow Gaps
- **grep_search silent failures**: The `grep_search` tool returned zero results for queries `ToDto|ApplyTo` and `ApplicationMappingExtensions|ToDto|ToCommand|MappingExtensions` despite these patterns clearly existing in the codebase (`ApplicationMappingExtensions.cs` contains `ToDto` and `ApplyTo` methods). This required a fallback to `file_search` with a glob pattern to locate the file, adding an extra round-trip.
- **Two-phase output (Phase A/B) adds complexity**: The planner instructions split output into Phase A (terminal operations) and Phase B (file creation) to avoid tool conflicts. While necessary, this adds cognitive overhead and an extra commit step.

### Tooling Friction / Missing Capabilities
- **`grep_search` reliability**: The tool appears to have indexing gaps — it failed to find text patterns that `read_file` later confirmed exist. This undermines trust in search results and forces redundant verification steps.
- **No skill invocation mechanism**: Skills (`.ai/skills/`) have no dedicated invocation tool. They must be manually read and executed, unlike agents which have `run_subagent`. This creates inconsistency in how reusable capabilities are invoked.

### Delays, Confusion & Inefficiencies
- **grep_search false negatives**: ~2 extra minutes spent verifying that files existed via alternative methods after grep returned no results.
- **Reflect & Adapt agent-not-found error**: ~1 minute diagnosing why `run_subagent` failed for `reflect-and-adapt`, then manually reading and executing the SKILL.md.

## Root Cause Analysis

- **Friction**: `run_subagent` failed for `reflect-and-adapt`
  - **Root Cause**: The planner agent instructions reference a skill as if it were an agent. Skills and agents are different primitives with different invocation mechanisms.
  - **Underlying Assumption**: That all named capabilities in the `.ai/` directory are agents invocable via `run_subagent`.
  - **Process Gap**: No validation step ensures that agent instructions only reference registered agent names.

- **Friction**: `grep_search` returned false negatives
  - **Root Cause**: Unknown — possibly an indexing delay, case-sensitivity issue, or file exclusion pattern. The tool's behavior is inconsistent.
  - **Underlying Assumption**: That `grep_search` provides exhaustive text search across all workspace files.
  - **Process Gap**: No fallback strategy documented for when search tools return unexpected empty results.

## Proposed Improvements

### Workflow/Process Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Fix planner agent to execute Reflect & Adapt skill inline rather than via `run_subagent` | 🟠 High | Low | High |
| Add a "search verification" step: if grep_search returns empty for a pattern you expect to exist, immediately fall back to file_search | 🟡 Medium | Low | Medium |
| Consider merging Phase A and Phase B into a single output step if tool conflicts can be avoided | 🔵 Low | Medium | Low |

### Tooling Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Investigate and fix `grep_search` false negatives — patterns like `ToDto` and `ApplyTo` should be findable | 🟠 High | Medium | High |
| Add a `run_skill` tool or extend `run_subagent` to support skills, providing a unified invocation mechanism | 🟡 Medium | High | Medium |

### Skill/Knowledge Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| Update planner.agent.md to clarify that Reflect & Adapt is a skill executed inline, not via `run_subagent` | 🟠 High | Low | High |
| Document the distinction between agents (invocable via `run_subagent`) and skills (manually executed) in AGENTS.md | 🟡 Medium | Low | Medium |

## Action Items
- [ ] Update `planner.agent.md` to replace `run_subagent` invocation of Reflect & Adapt with inline skill execution instructions
- [ ] Add agent vs. skill distinction to `AGENTS.md`
- [ ] Report `grep_search` false negatives for investigation

## Time Spent (Actual)
- Context loading (6 files): ~2 min
- Existing plan check & pre-scaffold detection: ~2 min
- Studying existing ApplicationManagement patterns (12 files read): ~5 min
- Spec consistency check & plan authoring: ~5 min
- Phase A (directory cleanup, branch setup, commit): ~1 min
- Phase B (plan file creation): ~2 min
- Reflect & Adapt (manual skill execution): ~3 min
- Total: ~20 minutes

## Lessons Learned
- Always verify `grep_search` results with `file_search` when searching for patterns you know should exist — the tool has reliability gaps.
- Skills and agents are separate primitives; check whether a capability is a skill (`.ai/skills/`) or agent (`.ai/agents/`) before choosing an invocation method.
- The existing `ApplicationManagement` feature is a clean, consistent reference pattern — studying it thoroughly upfront saves design time.
