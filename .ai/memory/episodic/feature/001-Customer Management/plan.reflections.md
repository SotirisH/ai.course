# Reflect & Adapt: Customer Management — Planning Stage

## 1. Assess Friction Encountered

### Violations & Showstoppers

- **None identified.** All required context files loaded successfully. Metadata extracted cleanly from the work item. No process steps were skipped.

### Process Friction / Workflow Gaps

- **Git branch name with spaces**: The feature name "Customer Management" contains a space, but git rejects branch names with spaces. Had to convert to kebab-case (`feature/001-customer-management`) while the directory uses the original spaced name. This is a cosmetic inconsistency between the filesystem path and the branch name. The planning instructions say `feature/{ticket_num}-{feature_name}` — spaces in `{feature_name}` break git branch creation.
- **No DELETE in story but present in reference**: The Applications feature (reference pattern) includes a DELETE endpoint, but the Customer story omits it. This creates ambiguity — is it intentional or an oversight? The planner must flag this rather than silently omit it.

### Tooling Friction / Missing Capabilities

- **`file_search` tool failures**: Two consecutive `file_search` calls failed with `TypeError: Cannot read properties of undefined (reading 'length')`. The cause is unclear — possibly related to input format. Had to fall back to `grep_search` and `list_dir` to find the Application MappingExtensions file.
- **No directory-level read for Mappings**: `read_file` on a directory path (the `ApplicationManagement/Mappings` folder) returned "not found" — expected behavior but worth noting that directory listing should be used instead.

### Delays, Confusion, or Inefficiencies

- **Minor**: Needed 3 separate tool calls to locate the `Application\Mappings\ApplicationMappingExtensions.cs` file (one failed `file_search`, one `grep_search` that returned no results due to global usings masking the import, and finally `list_dir` + `read_file`). This is because `ToDto`/`ToEntity`/`ApplyTo` were in namespaces resolved via global usings, making grep on the raw strings miss them.
- **Minor**: Had to infer the Application-layer mapping file location. The architecture doc says mappings go in `Application/Mappers/` but the actual codebase places them in `Application/Mappings/`.

## 2. Identify Root Causes

| Issue | Root Cause | Classification |
|-------|-----------|---------------|
| Branch name with spaces | The planning instructions use `{feature_name}` directly in branch name without sanitization. Feature names can contain spaces. | Systemic |
| Missing DELETE endpoint ambiguity | The story format doesn't require explicit listing of "not implemented" endpoints. The planner must always cross-reference with reference patterns. | Systemic |
| `file_search` tool failure | Unknown tool layer issue — possibly a malformed parameter or a transient runtime error. | One-time |
| Architecture doc vs actual codebase (Mappers vs Mappings) | The architecture doc says `Mappers/` but the actual codebase uses `Mappings/`. The codebase is the source of truth. | Systemic |
| Grep missed mapping extensions | Global usings mask direct namespace references. Searching for method names (`ToDto`, `ToEntity`) in code that uses global usings is unreliable. | Systemic |

## 3. Propose Actionable Improvements

### Workflow / Process

1. **Branch name sanitization**: Update the planning instructions to specify that `{feature_name}` should be converted to kebab-case for the git branch: `feature/{ticket_num}-{kebab-feature-name}`. This prevents the space-in-branch-name error.
2. **Architecture doc alignment**: Update `architecture.md` line 94 to use `Mappers/` → `Mappings/` to match the actual codebase folder structure. The `Application/Mappers/` reference is incorrect; the real folder is `Application/Mappings/`.
3. **Cross-reference checklist**: Add a step to the planning workflow that explicitly compares the work item's endpoint list against the reference feature's endpoint list to catch omissions.

### Tooling

4. **Prefer `list_dir` over `file_search` for known locations**: When the folder structure is well-known (e.g., `Application/Mappings/`), `list_dir` followed by targeted `read_file` is more reliable than `file_search` with glob patterns.

### Skill / Knowledge

5. **Global usings awareness**: When searching for code references, be aware that `global using` declarations may cause `grep_search` to miss actual usages. Search for the class/method names directly in file content rather than relying on `using` statement patterns.

## 4. Prioritize Improvements

| # | Improvement | Priority |
|---|------------|----------|
| 1 | Branch name sanitization in planning instructions | 🟠 High — causes immediate failure on every feature with spaces |
| 2 | Architecture doc folder name correction (`Mappers/` → `Mappings/`) | 🟡 Medium — causes confusion but doesn't block progress |
| 3 | Cross-reference checklist for endpoint parity | 🟡 Medium — prevents missed requirements |
| 4 | Prefer `list_dir` for known folder locations | 🔵 Low — minor efficiency improvement |
| 5 | Global usings grep awareness | 🔵 Low — edge case, easily worked around |
