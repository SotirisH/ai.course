# Plan: API Layer Compliance Refactor

**Work Item Type:** refactor  
**Identifier:** api-layer-compliance  
**Date:** 2026-06-08

---

## Story Summary

Bring the Ai.Api presentation layer into full compliance with `architecture.md` and `coding-standards.md`. The existing controllers and models collectively violate several rules around return types, parameter binding, API conventions, and typed responses.

---

## Acceptance Criteria

### Controllers
- **Given** any controller action returns data, **When** the method signature is inspected, **Then** it uses `ActionResult<T>` instead of `IActionResult`.
- **Given** a route parameter exists (`{id}`), **When** the action method parameter is defined, **Then** `[FromRoute]` is explicitly applied.
- **Given** standard CRUD actions exist (Post, Put, Get, Delete), **When** the action is decorated, **Then** `[ApiConventionMethod]` references the matching `DefaultApiConventions` member.
- **Given** `[ApiConventionMethod]` is applied, **When** the convention covers standard status codes (200, 201, 400, 404), **Then** redundant `[ProducesResponseType]` attributes are removed for those codes.
- **Given** an action returns a non-standard status code (e.g., 409 Conflict), **When** no convention covers it, **Then** `[ProducesResponseType]` for that code is retained.

### Health Check
- **Given** the health endpoint is called, **When** the response is serialized, **Then** it uses a typed `HealthResponse` record rather than an anonymous object.
- **Given** the health endpoint exists, **When** Swagger/OpenAPI is generated, **Then** `[ProducesResponseType]` attributes document the response shape.

### Models
- **Given** a health check response is needed, **When** the `HealthResponse` record is defined, **Then** it uses `sealed record` with class-like syntax and contains `Status` and `Timestamp` properties.

---

## File Change List

| # | File | Action | Layer |
|---|------|--------|-------|
| 1 | `src/Ai.Api/Controllers/ApplicationsController.cs` | **Modify** | API |
| 2 | `src/Ai.Api/Controllers/HealthController.cs` | **Modify** | API |
| 3 | `src/Ai.Api/Models/Responses/HealthResponse.cs` | **Create** | API |

**No changes needed:**
- `src/Ai.Api/Program.cs` — security hardening deferred to a future stage
- `src/Ai.Api/Ai.Api.csproj` — no changes needed
- `Directory.Packages.props` — no changes needed
- `src/Ai.Api/Middleware/ExceptionHandlingMiddleware.cs` — already compliant
- `src/Ai.Api/Models/Requests/CreateApplicationRequest.cs` — already compliant
- `src/Ai.Api/Models/Requests/UpdateApplicationRequest.cs` — already compliant
- `src/Ai.Api/Models/Responses/ApplicationResponse.cs` — already compliant
- `src/Ai.Api/Mappers/ApplicationMappingExtensions.cs` — no issues flagged

---

## Implementation Details

### 1. `src/Ai.Api/Controllers/ApplicationsController.cs` — Architecture Rule Compliance

**1a. Change method return types from `IActionResult` to `ActionResult<T>`:**
- `Create` → `Task<ActionResult<ApplicationResponse>>`
- `GetAll` → `Task<ActionResult<IReadOnlyList<ApplicationResponse>>>`
- `GetById` → `Task<ActionResult<ApplicationResponse>>`
- `Update` → `Task<ActionResult<ApplicationResponse>>`
- `Delete` → `Task<ActionResult>` (no body returned, stays as `ActionResult`)

**1b. Add `[FromRoute]` attribute:**
- On `GetById(Guid id, ...)` → `GetById([FromRoute] Guid id, ...)`
- On `Update(Guid id, ...)` → `Update([FromRoute] Guid id, ...)`
- On `Delete(Guid id, ...)` → `Delete([FromRoute] Guid id, ...)`

**1c. Add `[ApiConventionMethod]` attributes and clean up `[ProducesResponseType]`:**
- **Create**: Add `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]` — remove `[ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]` and `[ProducesResponseType(StatusCodes.Status400BadRequest)]` (covered by convention). **Keep** `[ProducesResponseType(StatusCodes.Status409Conflict)]` (not covered by convention).
- **Update**: Add `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]` — remove `[ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]`, `[ProducesResponseType(StatusCodes.Status400BadRequest)]`, and `[ProducesResponseType(StatusCodes.Status404NotFound)]`. **Keep** `[ProducesResponseType(StatusCodes.Status409Conflict)]`.
- **GetById**: Add `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]` — remove `[ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]` and `[ProducesResponseType(StatusCodes.Status404NotFound)]`.
- **Delete**: Add `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]` — remove `[ProducesResponseType(StatusCodes.Status204NoContent)]` and `[ProducesResponseType(StatusCodes.Status404NotFound)]`.
- **GetAll**: No convention exists for "Get all" — **keep** `[ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]`.

> **Assumption:** `DefaultApiConventions.Post` covers 201 Created and 400 BadRequest. The `[ProducesResponseType]` for 409 is custom business logic and must stay. Confirmed per Microsoft docs.

**1d. Add missing `using` for `DefaultApiConventions`:**
```csharp
using Microsoft.AspNetCore.Mvc;
```
(Already present.)

---

### 2. `src/Ai.Api/Controllers/HealthController.cs` — Typed Response

**2a. Change return type to `ActionResult<HealthResponse>`:**
```csharp
public ActionResult<HealthResponse> Get()
```

**2b. Return a typed `HealthResponse` instead of anonymous object:**
```csharp
return Ok(new HealthResponse
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
});
```

**2c. Add `[ProducesResponseType]` attribute:**
```csharp
[HttpGet]
[ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
```

**2d. Add namespace import:**
```csharp
using Ai.Api.Models.Responses;
```

---

### 3. `src/Ai.Api/Models/Responses/HealthResponse.cs` — New File (Create)

A simple sealed record for the health endpoint response:

```csharp
namespace Ai.Api.Models.Responses;

public sealed record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
```

---

## Implementation Order

The changes should be applied in this order to minimize cascading build breaks:

| Step | File | Rationale |
|------|------|-----------|
| 1 | `src/Ai.Api/Models/Responses/HealthResponse.cs` | Create new file — no dependencies on other changes |
| 2 | `src/Ai.Api/Controllers/HealthController.cs` | Update to use `HealthResponse` — independent of other controllers |
| 3 | `src/Ai.Api/Controllers/ApplicationsController.cs` | Return types, `[FromRoute]`, API conventions — self-contained |

---

## Spec Consistency Check

No mismatches found between the story text and acceptance criteria. All violations have corresponding remediation steps.

---

## Naming Convention Verification

| Item | Convention | Status |
|------|-----------|--------|
| `CreateApplicationCommand` | Verb + Noun + "Command" | ✅ |
| `UpdateApplicationCommand` | Verb + Noun + "Command" | ✅ |
| `DeleteApplicationCommand` | Verb + Noun + "Command" | ✅ |
| `GetApplicationsQuery` | Get + Noun + "Query" | ✅ |
| `GetApplicationByIdQuery` | Get + Noun + "Query" | ✅ |
| `HealthResponse` | Descriptive name (Response suffix) | ✅ |
| `ApplicationResponse` | Descriptive name (Response suffix) | ✅ |
| `CreateApplicationRequest` | Descriptive name (Request suffix) | ✅ |
| `UpdateApplicationRequest` | Descriptive name (Request suffix) | ✅ |
| All records use class-like syntax | `public sealed record Foo { ... }` | ✅ |

---

## Assumptions

1. **`HealthResponse.Timestamp` as `DateTime`**: Used `DateTime` (not `DateTimeOffset`) because `DateTime.UtcNow` is already in use in the existing anonymous object. Justification: minimal change from current behavior.

2. **`DefaultApiConventions` attribute placement**: Applied per-action via `[ApiConventionMethod]` rather than at the controller level via `[ApiConventionType]`. Justification: `GetAll` has no matching convention, and `GetById` differs from standard `Get` naming — per-action provides precise control and avoids broken/swagger warnings from mismatched conventions.

3. **`ActionResult` vs `ActionResult<T>` on Delete**: Delete returns `NoContent()` with no body, so `Task<ActionResult>` is correct. There is no `T` to specify. Justification: `ActionResult<T>` would imply a response body, which 204 No Content does not have.

4. **Security hardening deferred**: `Program.cs` changes (security headers, HSTS, dev-only guards for OpenAPI/Scalar, `UseProblemDetails()`) and the `NetEscapades.AspNetCore.SecurityHeaders` package are intentionally excluded from this stage and will be addressed in a future work item.
