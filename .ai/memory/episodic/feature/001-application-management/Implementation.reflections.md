# Implementation Reflections: Application Management (Feature #001)

**Ticket:** #001 | **Feature:** 001-application-management | **Work Item Type:** feature

---

## 1. Violations & Showstoppers

### Namespace/Type Collision: `Application`
**Severity:** High (blocked compilation)
**Description:** The domain entity `Application` lives in namespace `Ai.Api.Domain.Entities`, but ALL Application-layer classes live under `Ai.Api.Application`. C# resolves `Application` as the namespace rather than the type, causing `CS0118` across 9 files.

**Root Cause:** The entity name `Application` is too generic and collides with the layer namespace `Ai.Api.Application`.

**Resolution:** Added `global using DomainApp = Ai.Api.Domain.Entities.Application;` to both `Ai.Api.Application/GlobalUsings.cs` and `Ai.Api.Infrastructure/GlobalUsings.cs`. All references to the domain entity use the `DomainApp` alias.

**Recommendation:** Consider renaming the domain entity to something more specific (e.g., `ManagedApplication`, `AppRegistration`, `AppInstance`) to avoid this collision. Alternatively, rename the Application project to `Ai.Api.ApplicationLayer` or similar. This is a systemic naming issue that will affect every feature touching this entity.

### Missing `Wolverine.FluentValidation` Using
**Severity:** Low (resolved quickly)
**Description:** `opts.UseFluentValidation()` failed without `using Wolverine.FluentValidation;` in `DependencyInjection.cs`.

**Root Cause:** The tech-stack.md documents this requirement but the plan didn't explicitly call out the needed using directive.

**Recommendation:** Add a "Required Usings" section to implementation plans for Wolverine-related files.

---

## 2. Process Friction / Workflow Gaps

### Plan Document Had Mixed Content
**Description:** The plan contained embedded code-block change descriptions from a previous review process interspersed with the actual plan content (lines 264-335). This made it harder to parse the canonical plan structure.

**Recommendation:** Finalize plans before handing to the coder agent. The plan should be a clean document without review artifacts embedded inline.

### Compilation Feedback Loop
**Description:** The edit-insert tool has limited preview, so namespace conflicts were discovered only at build time. It took 3 build iterations to resolve all `CS0118` errors.

**Recommendation:** Consider a pre-build validation step or linter that detects namespace/type name collisions before attempting compilation.

---

## 3. Tooling Friction / Missing Capabilities

### File Editing Requires Post-Edit Fixes
**Description:** The `insert_edit_into_file` tool only shows the edited content, not the full file. Stale `using` directives (e.g., `using Ai.Api.Domain.Entities;` that were no longer needed) were left in files. This was caught during manual review post-build.

**Recommendation:** A "clean file" or "remove unused usings" capability would be helpful after bulk edits.

### PowerShell `&&` Not Supported
**Description:** Using `&&` for command chaining fails in PowerShell. Had to switch to `;`.

**Recommendation:** Document in the .ai/rules that PowerShell `;` is the chaining operator, not `&&`.

---

## 4. Design Decisions Made During Implementation

| # | Decision | Rationale |
|---|----------|-----------|
| DD1 | Used `DomainApp` alias instead of renaming entity | Minimal scope change; entity rename would affect the plan, acceptance criteria, and all references |
| DD2 | `ExceptionHandlingMiddleware` created instead of exception filter | Middleware catches Wolverine-dispatched exceptions before they reach MVC pipeline; more robust |
| DD3 | `InvalidOperationException` for "not found" and "already exists" scenarios | Wolverine handlers can throw these naturally; middleware maps them to 404/409 via message inspection |
| DD4 | `AsNoTracking()` in all repository reads | Since domain entities are mapped back to persistence entities on write, tracking provides no benefit |

---

## 5. Summary
- **Files created:** 22
- **Files modified:** 4 (csproj files + Program.cs)
- **Build iterations to success:** 4
- **Key takeaway:** The `Application` entity name collision with the `Ai.Api.Application` namespace is a structural issue that will recur. A long-term fix (entity rename or project rename) should be considered.
