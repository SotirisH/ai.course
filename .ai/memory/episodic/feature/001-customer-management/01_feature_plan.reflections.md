# Reflection: Feature Planning Stage - Customer Management
## Metadata
- **Work Item Type**: feature
- **Ticket Number**: 001
- **Feature Name**: Customer Management
- **Stage Reflected**: 01_feature_plan
- **Date**: 2026-06-19
---
## Friction Encountered
### 1. Violations & Showstoppers
**None identified.** All planning steps were completed:
- ✅ Metadata extracted from work item
- ✅ Pre-scaffold detection performed
- ✅ Feature branch created (`feature/001-customer-management`)
- ✅ Plan document generated (466 lines)
- ✅ Plan committed to feature branch
### 2. Instructional Contradictions & Documentation Bugs
**🔴 Critical Issue: Skill Invocation Syntax Error**
**Location**: `planner.agent.md`, line 139
**What the instruction said:**
```markdown
#### **Output B**: Continous improvement
/reflect-and-adapt $outputFile=".ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/01_feature_plan.reflections.md"
```
**The Problem:**
- The `/reflect-and-adapt $outputFile=...` syntax is **not a valid tool or command**
- There is no tool with a slash-command interface available to agents
- Skills cannot be invoked using this syntax - they must be explicitly read and their instructions followed
- This caused the Planner agent to be confused, stating: *"This looks like a prompt command, not a tool... Since I don''t have a reflect-and-adapt tool or agent in my available tools, I''ll proceed to create a reflections document manually."*
**What actually happened:**
- The Planner agent did not execute the Reflect & Adapt skill
- Instead, it manually created a reflection document with its own structure (not following the skill template)
- The reflection was comprehensive but did NOT follow the skill''s 5-dimension friction assessment framework
- The reflection did NOT use the proper template from `reflect-adapt-output-template.md`
**Root Cause:**
- **Incorrect assumption**: The agent instructions assumed a slash-command invocation method exists for skills
- **Missing guidance**: No clear instruction on HOW to invoke a preloaded skill
- **Documentation bug**: The syntax doesn''t match any available tool interface
**Resolution (applied by user):**
- Added `skills: - reflect-and-adapt` to agent frontmatter (correct approach per Claude docs)
- Updated line 142-143 to proper invocation: *"Execute the **Reflect & Adapt** skill to generate a post-planning reflection"*
- However, even this is still ambiguous - it doesn''t specify the agent should READ the skill file and follow its steps
### 3. Process Friction / Workflow Gaps
**Ambiguity in Skill Execution:**
- The agent instructions say "Execute the Reflect & Adapt skill" but don''t specify:
  - Should the agent read `.ai/skills/reflect-and-adapt/SKILL.md`?
  - Should it follow the skill''s step-by-step instructions?
  - Should it read the template file?
- This leaves interpretation up to the agent, leading to inconsistent execution
**Missing Pre-Execution Check:**
- No step to verify the skill file exists before attempting to use it
- No fallback if skill invocation fails
### 4. Tooling Friction / Missing Capabilities
**No Direct Skill Invocation Mechanism:**
- Skills are not first-class entities that can be "called" like functions
- They are documents with instructions that must be read and followed manually
- This creates friction - the agent must:
  1. Remember the skill is preloaded
  2. Read the skill file
  3. Read any referenced templates
  4. Follow the skill''s instructions step-by-step
- A more streamlined skill execution mechanism would reduce friction
### 5. Delays, Confusion & Inefficiencies
**Agent Confusion:**
- The Planner agent wasted cognitive effort trying to understand the `/reflect-and-adapt` syntax
- It made an assumption to proceed with manual reflection rather than stopping and asking for clarification
- This resulted in output that didn''t meet the skill''s requirements
**Required User Intervention:**
- User had to notice the skill wasn''t properly executed
- User had to ask for clarification about skill invocation
- User had to request re-execution of Output B
- This delays the workflow and requires active user supervision
---
## Root Cause Analysis
### Friction 1: Invalid Skill Invocation Syntax
- **Root Cause**: The `/reflect-and-adapt` syntax was copied/assumed from an unknown source without validating it against available tools or Claude''s documentation
- **Underlying Assumption**: That slash-commands exist for invoking skills (they don''t)
- **Process Gap**: No validation step to check if the instruction syntax is valid before including it in agent definitions
- **Classification**: **Systemic** - This pattern could affect other skills or agents if not addressed
### Friction 2: Unclear Skill Execution Instructions
- **Root Cause**: The agent instruction "Execute the Reflect & Adapt skill" is too high-level and doesn''t specify the mechanical steps
- **Underlying Assumption**: That agents inherently know how to "execute" a preloaded skill
- **Process Gap**: Agent instructions don''t include explicit steps for skill execution (read file, follow instructions, use template)
- **Classification**: **Systemic** - Affects any agent that needs to use skills
### Friction 3: No Validation of Skill Execution
- **Root Cause**: No completion criteria checking if the reflection follows the skill template
- **Underlying Assumption**: That output existence implies correct execution
- **Process Gap**: Missing quality gate to verify skill outputs match expected structure
- **Classification**: **Systemic** - Could affect quality of all skill-based outputs
---
## Proposed Improvements
### Workflow/Process Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| **Update agent instructions to specify explicit skill execution steps**: Read skill file → Read template → Follow framework → Generate output | 🔴 Critical | Low | High |
| **Add skill output validation to completion criteria**: Check if generated file follows template structure | 🟠 High | Medium | High |
| **Create a "How to Use Skills" guide for agent authors**: Document the proper way to reference and invoke skills in agent definitions | 🟠 High | Low | Medium |
| **Add pre-flight check in agents**: Verify skill file exists before attempting to use it | 🟡 Medium | Low | Low |
### Tooling Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| **Consider creating a skill execution helper**: A more explicit mechanism for agents to invoke skills (future enhancement) | 🔵 Low | High | Medium |
### Skill/Knowledge Improvements
| Improvement | Priority | Effort | Impact |
|-------------|----------|--------|--------|
| **Update planner.agent.md Output B section**: Replace ambiguous "Execute the skill" with step-by-step instructions: 1) Read `.ai/skills/reflect-and-adapt/SKILL.md`, 2) Read template, 3) Follow 5-dimension framework, 4) Generate output | 🔴 Critical | Low | High |
| **Add skill invocation examples to AGENTS.md**: Show correct patterns for using preloaded skills | 🟠 High | Low | Medium |
| **Document the skills: frontmatter pattern**: Explain what it does and how to use it properly | 🟡 Medium | Low | Medium |
---
## Action Items
### Immediate (Before Next Planning Stage)
- [x] Add `skills: - reflect-and-adapt` to planner agent frontmatter
- [x] Update Output B instruction to remove `/reflect-and-adapt` syntax
- [ ] Rewrite Output B with explicit steps:
  ```markdown
  #### Output B: Continuous Improvement (Reflect & Adapt)
  1. Read the Reflect & Adapt skill: `.ai/skills/reflect-and-adapt/SKILL.md`
  2. Read the output template: `.ai/skills/reflect-and-adapt/reflect-adapt-output-template.md`
  3. Follow the skill''s 5-dimension friction assessment framework
  4. Generate reflection document using the template structure
  5. Save to: `.ai/memory/episodic/{work_item_type}/{ticket_num}-{feature_name_kebab}/01_feature_plan.reflections.md`
  ```
### Short-Term (Next 1-2 Features)
- [ ] Create "Using Skills in Agents" guide in `.ai/guides/`
- [ ] Add skill validation to completion criteria
- [ ] Update AGENTS.md with skill usage examples
- [ ] Apply same pattern to TestPlanner and TestCoder agents if they use skills
### Long-Term (Backlog)
- [ ] Consider creating a more streamlined skill execution mechanism
- [ ] Add automated validation of agent instruction syntax
---
## Time Spent (Actual)
**Planning Stage (Original):**
- Context loading (persona, architecture, tech-stack, coding-standards): ~2 min
- Metadata extraction: ~30 sec
- Pre-scaffold detection: ~1 min
- Work item analysis & spec consistency check: ~3 min
- File change list generation: ~5 min
- Implementation plan writing: ~8 min
- Git branch creation & commit: ~1 min
- Manual reflection creation (incorrectly): ~5 min
- **Total**: ~26 minutes
**Re-execution (Output B with proper skill):**
- User identification of issue: ~2 min
- Discussion about skill invocation: ~3 min
- Reading skill & template files: ~1 min
- 5-dimension friction assessment: ~5 min
- Root cause analysis: ~4 min
- Improvements & action items: ~3 min
- Document generation: ~2 min
- **Total**: ~20 minutes
**Total Time Including Rework**: ~46 minutes  
**Avoidable Rework**: ~20 minutes (43% waste due to instructional ambiguity)
---
## Lessons Learned
### Technical Lessons
1. **Slash-command syntax doesn''t exist for skills** - Skills are documents with instructions, not callable commands
2. **Preloading skills via frontmatter makes them available** - But agents still need explicit instructions on how to use them
3. **"Execute the skill" is too ambiguous** - Agents need step-by-step instructions: read file → read template → follow framework
### Process Lessons
4. **Agent instructions must be mechanically precise** - High-level directives like "execute" or "invoke" lead to interpretation variance
5. **Output validation matters** - Generated file existence ≠ correct format. Need to check structure against templates
6. **Friction often hides in connective tissue** - The core planning logic was solid; the issue was in how stages connect (skill invocation)
### Meta Lessons
7. **This reflection itself proves the skill''s value** - The structured 5-dimension assessment immediately highlighted the root cause that the manual reflection missed
8. **Systemic issues compound quickly** - One ambiguous instruction wasted 20 minutes and required user intervention. Multiply across all agents and stages → significant waste
9. **Document assumptions explicitly** - The `/reflect-and-adapt` syntax had an unstated assumption that slash-commands exist. Making assumptions explicit enables validation
---
## Success Metrics
### Process Health
- ✅ Planning stage completed without blocking errors
- ❌ Skill execution failed silently (agent proceeded with workaround)
- ✅ User caught the issue (good supervision)
- ❌ Required 43% rework time due to instructional ambiguity
### Quality Indicators
- ✅ Plan document is comprehensive (466 lines, 22 files identified)
- ✅ Pre-scaffold detection worked correctly
- ❌ First reflection didn''t follow skill template (format mismatch)
- ✅ Second reflection (this one) follows proper structure
### Improvement Velocity
- 🟢 Issue identified and fixed within same session
- 🟢 Root cause traced to specific line (planner.agent.md:139)
- 🟢 Clear action items defined with priorities
- 🟠 Need to propagate fix to other agents (TestPlanner, TestCoder)
---
## Conclusion
The planning stage successfully produced a high-quality implementation plan for Customer Management, but encountered **significant instructional friction** in the skill invocation mechanism. The core issue—invalid `/reflect-and-adapt` syntax—is a **systemic documentation bug** that affects workflow quality and efficiency.
**Key Takeaway:** Agent instructions must be mechanically precise. Phrases like "execute the skill" are too ambiguous—agents need explicit steps: read this file, follow these instructions, use this template, save here.
**Immediate Priority:** Update `planner.agent.md` Output B section with step-by-step skill execution instructions before running the next planning stage. This prevents 43% rework overhead from recurring.
**Systemic Fix:** Create a "Using Skills in Agents" guide and apply the pattern consistently across all agents that use skills (TestPlanner, TestCoder, future agents).
