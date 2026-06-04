# Plan: RS-001 — Routing Slip — Basic Functionality

## Story Summary

Build a **Core.RoutingSlip** class library that implements the Routing Slip pattern — a distributed transaction pattern where a message carries its own itinerary and passes through ordered activities. Phase 1 delivers the core forward-execution engine: fluent builder API, state machine (`Created → Executing → Completed / Faulted`), activity log, compensation log (empty placeholder), variables, and tracking number.

## Acceptance Criteria (Given-When-Then)

### 1.1 Routing Slip Contract
- **Given** a routing slip is created via the builder
- **When** inspected
- **Then** it contains a unique tracking number (`Guid`), an ordered read-only itinerary, an activity log, a compensation log, and a state property (`Created` / `Executing` / `Completed` / `Faulted` / `Compensated`).

### 1.2 Activity Entry
- **Given** a developer implements `IActivity<TArgument, TResult>`
- **When** the activity is added to a routing slip itinerary
- **Then** it exposes a `Name` property and an `ExecuteAsync` method accepting a typed payload and cancellation token, returning `TResult`.

### 1.3 Forward Execution
- **Given** a routing slip with N activities
- **When** the engine executes
- **Then** activities are invoked in order (1→N). Each completed activity is appended to the activity log with a timestamp and any output variables. On full completion, state → `Completed`.

### 1.4 Building a Routing Slip
- **Given** a developer uses `RoutingSlipBuilder`
- **When** activities are added via `.AddActivity(...)` and `.Build()` is called
- **Then** a fully populated routing slip is returned.

### 1.5 State Machine
- **Given** routing slip execution
- **When** it starts, runs, completes, or fails
- **Then** it transitions: `Created → Executing → Completed` (success) or `Created → Executing → Faulted` (any activity throws).

---

## Spec Consistency Issues (Flagged for Clarification)

| # | Issue | Detail |
|---|-------|--------|
| **S1** | `IActivity<T, Result>` has shadowed generic `T` | The code snippet in §1.2 declares `Task<Result> ExecuteAsync<T>(T payload, ...)`, which introduces a **method-level** generic `T` that shadows the class-level `T`. This is a design bug. The plan assumes a corrected signature (see Implementation Details). |
| **S2** | `Compensated` state listed but out of scope | §1.1 lists `Compensated` as a state, but §1.5's state diagram omits it and "Out of Scope" defers compensation to RS-002. The plan **includes** the enum member but the engine will never transition to it in Phase 1. |
| **S3** | `ExecutionResult<T>` vs plain `Result` | The concepts table mentions "Returned Value: ExecutionResult<T>" but the code shows `Task<Result>` with no wrapper type. The plan uses the plain generic `TResult` (no wrapper) to match the code snippet. |
| **S4** | Variables not formalized | The story and §1.3 mention "variables" and "output variables returned by the activity" but provide no schema, storage mechanism, or accessor API. The plan introduces a `Dictionary<string, object?>` for variables. |
| **S5** | No execution engine entry point defined | The story mentions an "engine" but there is no acceptance criterion defining how to start execution. The plan adds an `IRoutingSlipEngine` with `ExecuteAsync(RoutingSlip, CancellationToken)`. |
| **S6** | Tracking number creation not specified | The builder example never shows how the tracking number is set. The plan auto-generates it via `Guid.NewGuid()` during `Build()`. |

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| **A1** | Corrected `IActivity` signature to `IActivity<TArgument, TResult>` with `Task<TResult> ExecuteAsync(TArgument payload, CancellationToken cancellationToken)` — no shadowed generic. | The code snippet in §1.2 is functionally broken (shadowing). The corrected design matches the intent described in the concepts table. |
| **A2** | Variables stored as `Dictionary<string, object?>` on the routing slip. | Simplest in-process key-value store that satisfies the "output variables" requirement without introducing a type system. |
| **A3** | Activity log entry contains: activity name, timestamp, duration, and optional variables. | Industry-standard logging pattern for activity execution. Matches "with a timestamp" from §1.3. |
| **A4** | Compensation log is an empty `List<CompensationEntry>` placeholder — populated in RS-002. | Explicitly deferred per "Out of Scope". Structure needed now for the contract. |
| **A5** | Tracking number auto-generated as `Guid.NewGuid()` in `Build()`. | No story requirement for external assignment. Keeps the builder API simple as shown in §1.4. |
| **A6** | `Faulted` state captures the exception message and faulted activity index. | Necessary for diagnostics and for RS-002 compensation logic. |
| **A7** | All models are `sealed record` (class-like syntax) for immutability and value semantics. | Aligns with `coding-standards.md` records rule. |
| **A8** | Project targets `net10.0`, uses `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>`. | Consistency with existing solution projects. |
| **A9** | No NuGet dependencies for Phase 1 — pure in-process library. | No persistence, messaging, or validation frameworks needed yet. |
| **A10** | Itinerary is immutable after `Build()` — enforced via `IReadOnlyList<T>`. | Matches §1.1 "ordered, read-only list of activity entries". |

---

## Open Questions

| # | Question |
|---|----------|
| **Q1** | Should `Compensated` be removed from the state enum in Phase 1 since compensation is out of scope? Or keep it for forward compatibility? |
| **Q2** | Should the corrected `IActivity` interface use `TArgument`/`TResult` naming, or keep the original `T`/`Result` naming (minus the shadowing bug)? |
| **Q3** | Should variables be keyed by activity name automatically, or should activities explicitly set variable names? |
| **Q4** | Should the engine be synchronous in Phase 1 or fully async from the start? The `IActivity` interface already returns `Task<TResult>`. |
| **Q5** | Should the faulted routing slip capture the full `Exception` object or just the message string? Full exception could cause serialization issues later. |
| **Q6** | Should `Build()` accept an optional tracking number, or always auto-generate? |

---

## File Change List

All files are **new** under `src/Core.RoutingSlip/`.

| # | File | Layer / Concern |
|---|------|-----------------|
| 1 | `Core.RoutingSlip.csproj` | Project file |
| 2 | `Models/RoutingSlipState.cs` | Enum: state machine states |
| 3 | `Models/RoutingSlip.cs` | Core entity: routing slip contract |
| 4 | `Models/ItineraryEntry.cs` | Value object: a single activity entry in the itinerary |
| 5 | `Models/ActivityLogEntry.cs` | Value object: completed activity record |
| 6 | `Models/CompensationEntry.cs` | Value object: compensation data placeholder |
| 7 | `Activities/IActivity.cs` | Interface: activity contract |
| 8 | `Engine/IRoutingSlipEngine.cs` | Interface: execution engine contract |
| 9 | `Engine/RoutingSlipEngine.cs` | Implementation: forward-execution engine |
| 10 | `Builders/RoutingSlipBuilder.cs` | Fluent builder API |
| 11 | `Ai.Api.slnx` (edit) | Add project reference to solution |

---

## Implementation Details

### 1. `Core.RoutingSlip.csproj`
- SDK: `Microsoft.NET.Sdk`
- `TargetFramework`: `net10.0`
- `ImplicitUsings`: `enable`
- `Nullable`: `enable`
- No package references.

### 2. `RoutingSlipState` (enum)

```csharp
public enum RoutingSlipState
{
    Created,
    Executing,
    Completed,
    Faulted,
    Compensated       // Reserved for RS-002; never set in Phase 1
}
```

### 3. `RoutingSlip` (sealed record, class-like syntax)

```csharp
public sealed record RoutingSlip
{
    public Guid TrackingNumber { get; init; }
    public RoutingSlipState State { get; init; }
    public IReadOnlyList<ItineraryEntry> Itinerary { get; init; } = Array.Empty<ItineraryEntry>();
    public IReadOnlyList<ActivityLogEntry> ActivityLog { get; init; } = Array.Empty<ActivityLogEntry>();
    public IReadOnlyList<CompensationEntry> CompensationLog { get; init; } = Array.Empty<CompensationEntry>();
    public Dictionary<string, object?> Variables { get; init; } = new();
}
```

### 4. `ItineraryEntry` (sealed record)

```csharp
public sealed record ItineraryEntry
{
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public object Activity { get; init; } = null!;  // The IActivity instance (erased generic)
}
```

### 5. `ActivityLogEntry` (sealed record)

```csharp
public sealed record ActivityLogEntry
{
    public string ActivityName { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public TimeSpan Duration { get; init; }
    public Dictionary<string, object?> Variables { get; init; } = new();
}
```

### 6. `CompensationEntry` (sealed record)

```csharp
public sealed record CompensationEntry
{
    public string ActivityName { get; init; } = string.Empty;
    public Dictionary<string, object?> Data { get; init; } = new();
}
```

### 7. `IActivity<TArgument, TResult>` (corrected interface)

```csharp
public interface IActivity<TArgument, TResult>
{
    string Name { get; }
    Task<TResult> ExecuteAsync(TArgument payload, CancellationToken cancellationToken);
}
```

**Note:** This corrects the shadowed generic `T` from the work item code snippet. The class-level `T` is renamed to `TArgument`, and the method no longer introduces its own generic parameter.

### 8. `IRoutingSlipEngine`

```csharp
public interface IRoutingSlipEngine
{
    Task<RoutingSlip> ExecuteAsync(RoutingSlip routingSlip, CancellationToken cancellationToken = default);
}
```

### 9. `RoutingSlipEngine` (implementation)

**Algorithm:**
1. Validate routing slip state is `Created`; throw `InvalidOperationException` otherwise.
2. Transition state to `Executing`.
3. For each `ItineraryEntry` in order (by `Position`):
   - Cast `entry.Activity` to the appropriate `IActivity<,>` via reflection (store a non-generic wrapper in the entry — see note below).
   - Record start timestamp.
   - Await `activity.ExecuteAsync(payload, ct)`.
   - On success: append `ActivityLogEntry` to log, collect any output into `Variables`.
   - On exception: transition state to `Faulted`, set fault metadata, stop execution.
4. If all activities complete: transition state to `Completed`.
5. Return the updated routing slip.

**Non-generic activity wrapper approach:** Since `IActivity<TArgument, TResult>` is generic and the itinerary needs to store heterogeneous activities, introduce an internal non-generic wrapper interface:

```csharp
internal interface IActivityInvoker
{
    string Name { get; }
    Task<object?> InvokeAsync(object? payload, CancellationToken cancellationToken);
}
```

Each `ItineraryEntry` stores an `IActivityInvoker`. The builder wraps each `IActivity<TArgument, TResult>` in an adapter that implements `IActivityInvoker`.

### 10. `RoutingSlipBuilder`

```csharp
public class RoutingSlipBuilder
{
    private readonly List<ItineraryEntry> _entries = new();

    public RoutingSlipBuilder AddActivity<TArgument, TResult>(IActivity<TArgument, TResult> activity, TArgument payload)
    {
        var entry = new ItineraryEntry
        {
            Name = activity.Name,
            Position = _entries.Count,
            Activity = new ActivityInvoker<TArgument, TResult>(activity, payload)
        };
        _entries.Add(entry);
        return this;
    }

    public RoutingSlip Build()
    {
        return new RoutingSlip
        {
            TrackingNumber = Guid.NewGuid(),
            State = RoutingSlipState.Created,
            Itinerary = _entries.AsReadOnly()
        };
    }
}
```

### Naming Convention Checkpoint

| Item | Name | Convention Check |
|------|------|-----------------|
| Enum | `RoutingSlipState` | ✅ PascalCase, singular noun |
| Record | `RoutingSlip` | ✅ PascalCase, singular noun |
| Record | `ItineraryEntry` | ✅ PascalCase, singular noun |
| Record | `ActivityLogEntry` | ✅ PascalCase, singular noun |
| Record | `CompensationEntry` | ✅ PascalCase, singular noun |
| Interface | `IActivity<TArgument, TResult>` | ✅ `I` prefix |
| Interface | `IRoutingSlipEngine` | ✅ `I` prefix |
| Class | `RoutingSlipEngine` | ✅ Interface name without `I` |
| Class | `RoutingSlipBuilder` | ✅ PascalCase, descriptive |
| Async method | `ExecuteAsync` | ✅ Async suffix |
| Async method | `InvokeAsync` | ✅ Async suffix |

---

## Implementation Order

1. **Create project** — `Core.RoutingSlip.csproj` and add to `Ai.Api.slnx`.
2. **Enums** — `RoutingSlipState.cs`
3. **Models (bottom-up)** — `CompensationEntry.cs` → `ActivityLogEntry.cs` → `ItineraryEntry.cs` → `RoutingSlip.cs`
4. **Activity interface** — `IActivity.cs` + internal `IActivityInvoker` + `ActivityInvoker<TArgument, TResult>` adapter
5. **Builder** — `RoutingSlipBuilder.cs`
6. **Engine interface & implementation** — `IRoutingSlipEngine.cs` → `RoutingSlipEngine.cs`
7. **Build & verify** — `dotnet build` the solution

---

## Test Strategy

**Test project:** `tests/Core.RoutingSlip.Tests/` (xUnit + Shouldly, target `net10.0`)

| Test | Type | Covers |
|------|------|--------|
| `RoutingSlipBuilder` builds with correct defaults | Unit | AC 1.1, 1.4 |
| `RoutingSlipBuilder` preserves activity order | Unit | AC 1.4 |
| `RoutingSlipBuilder` auto-generates unique tracking numbers | Unit | AC 1.1 |
| Engine executes activities in order (happy path) | Unit | AC 1.3 |
| Engine transitions `Created → Executing → Completed` | Unit | AC 1.5 |
| Engine populates activity log after each success | Unit | AC 1.3 |
| Engine captures variables from activity output | Unit | AC 1.3 |
| Engine transitions to `Faulted` on activity exception | Unit | AC 1.5 |
| Engine stops execution after first fault | Unit | AC 1.3, 1.5 |
| Engine throws if state is not `Created` at start | Unit | Edge case |
| Cancellation token is honored mid-execution | Unit | Edge case |
| Empty itinerary transitions straight to `Completed` | Unit | Edge case |
