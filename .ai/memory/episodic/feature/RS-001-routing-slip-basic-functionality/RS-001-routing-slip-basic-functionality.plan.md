# Plan: RS-001 — Routing Slip — Basic Functionality

## Metadata
- **Ticket**: RS-001
- **Feature**: Routing Slip — Basic Functionality (`routing-slip-basic-functionality`)
- **Type**: feature
- **Mode**: Ask (plan only, no Git operations)

---

## Story Summary

As a developer, I want to define and execute a routing slip — a sequence of ordered activities that a message passes through — so that I can orchestrate distributed transactions across multiple services.

This is Phase 1 (Basic Functionality): the core forward-execution engine. A routing slip is created via a fluent builder, populated with ordered activities, and executed sequentially in-process. Each activity completion is logged; failures fault the slip and capture the exception.

This work item introduces a **new standalone class library** `Core.RoutingSlip` into the existing `Ai.Api.slnx` solution — it is NOT part of the 4-layer `Ai.Api.*` architecture.

---

## Acceptance Criteria (Given-When-Then)

### AC 1.1 — Routing Slip Contract
- **Given** a routing slip is built
- **When** it is inspected
- **Then** it contains: a unique `TrackingNumber` (Guid), an ordered read-only `Itinerary` of activity entries, an ordered `ActivityLog` of completed entries, and a `State` (one of: `Created`, `Executing`, `Completed`, `Faulted`)

### AC 1.2 — Activity Entry & IActivity<T, TResult> Interface
- **Given** an activity is defined
- **When** it implements `IActivity<T, TResult>`
- **Then** it exposes a `Name` property and an `ExecuteAsync(T payload, CancellationToken)` method returning `Task<TResult>`

### AC 1.3 — Routing Slip Builder (Fluent API)
- **Given** a developer wants to build a routing slip
- **When** they use `RoutingSlipBuilder`
- **Then** they can chain `.AddActivity(activity)` calls and finalize with `.Build()` to obtain a populated `RoutingSlip` contract

### AC 1.4 — Forward Execution
- **Given** a routing slip with N activities in the itinerary
- **When** `IRoutingSlipExecutor.ExecuteAsync(routingSlip, ct)` is called
- **Then** activities execute **in order** (1 → 2 → … → N), asynchronously, and each completed activity is appended to the activity log with a timestamp. If any activity faults, the exception is captured and execution stops.

### AC 1.5 — State Machine
- **Given** a routing slip exists
- **When** it transitions through its lifecycle
- **Then** it follows: `Created → Executing → Completed` on success, or `Created → Executing → Faulted` on failure

---

## Spec Consistency Check

| # | Issue | Severity |
|---|-------|----------|
| 1 | **State diagram ambiguity**: The work item shows `Created → Executing → Completed → Faulted` on one line, which could be misread as `Completed → Faulted`. The most logical interpretation (confirmed by the narrative) is: `Created → Executing → Completed` (success path) and `Created → Executing → Faulted` (failure path). | Medium |
| 2 | **Payload duality**: `IActivity<T, Result>.ExecuteAsync(T payload, ...)` expects the payload at execution time, but the builder example shows payload passed via constructor (`new ReserveInventoryActivity(new ReserveInventoryRequest { ... })`). The plan resolves this by: payload is stored in the activity at construction; the non-generic `IActivity` wraps the internal call, and the generic `ExecuteAsync` receives the stored payload. | Medium |
| 3 | **`ExecutionResult<T>` "can be void"**: The concepts table mentions `ExecutionResult<T>` and says it "can be void." The plan introduces a non-generic `ExecutionResult` (for void) alongside `ExecutionResult<T>` (for typed results). This needs user confirmation. | Low |
| 4 | **`IActivity<T, Result>` naming**: The work item uses `Result` as the generic type parameter name — inconsistent with .NET conventions (`TResult`). The plan uses `TResult` internally but `Result` in the public API if the work item explicitly requires it. Currently assuming `TResult` is acceptable. | Low |

---

## Assumptions

| # | Assumption | Justification |
|---|-----------|---------------|
| A1 | `Core.RoutingSlip` is a standalone class library (not layered into Domain/Application/Infrastructure) since it's a focused, reusable engine with no persistence, API, or infrastructure concerns in Phase 1. | Work item specifies "a new project (Class library)" — singular. The library's responsibilities are purely domain-level (entities, execution, builder). |
| A2 | `IActivity` (non-generic) is needed as a base for storing activities of different `T`/`TResult` types in a single itinerary. | `AddActivity(IActivity)` requires a common type; `IActivity<T, TResult>` alone cannot hold heterogeneous activities. |
| A3 | Payload is stored in the concrete activity via constructor and the non-generic `ExecuteAsync` delegates to the generic one with the stored payload. | Builder example shows `new ReserveInventoryActivity(new ReserveInventoryRequest { ... })` — payload is construction-time, not execution-time. |
| A4 | `Guid.CreateVersion7()` for tracking numbers, consistent with the existing `Application` entity pattern. | Architecture rules mandate `Guid.CreateVersion7()` for IDs. |
| A5 | `ActivityLogEntry` stores: `ActivityName` (string), `Timestamp` (DateTimeOffset), and optionally the `Result` (object?). For Phase 1, minimal fields — name and timestamp. | The work item says "appended to the activity log with a timestamp." No mention of storing results in Phase 1. |
| A6 | Faulted routing slips store the full `Exception` object (not just a message). | AC 1.4: "The faulted routing slip must capture the full Exception object." |
| A7 | `Core.RoutingSlip` has no dependency on `Ai.Api.Domain` or any other existing project. | It's a standalone library with its own domain concepts. |
| A8 | No Wolverine/MediatR usage in this library. It's a plain class library with direct method calls. | Phase 1 is in-process execution with no messaging. Wolverine would add unnecessary complexity for a builder/executor pattern. |

---

## Questions for User Clarification

| # | Question |
|---|----------|
| Q1 | **State diagram**: Confirm that `Completed` and `Faulted` are terminal states (not `Completed → Faulted`). The plan treats them as separate terminal states from `Executing`. |
| Q2 | **`ExecutionResult<T>` "can be void"**: Should there be a non-generic `ExecutionResult` class (like `Unit`), or should `Task` (non-generic) suffice for void activities? |
| Q3 | **Activity log content**: Should the activity log capture the result value of each completed activity, or just name + timestamp? (Phase 1 is minimal.) |
| Q4 | **`AddActivity` signature**: Should it accept `IActivity` (non-generic base), `IActivity<T, TResult>`, or both? The plan uses `IActivity` (non-generic). |
| Q5 | **Exception capture**: Should the faulted slip expose the captured exception as a property (`Exception? FaultException`), or only via internal state? |
| Q6 | **Duplicate activity names**: Should the builder validate that all activities have unique names within an itinerary? |
| Q7 | **Empty itinerary**: Should `Build()` throw if no activities were added, or is an empty slip (instant `Completed`) acceptable? |

---

## Test Strategy

### Unit Tests (`Core.RoutingSlip.Tests` — xUnit + Shouldly)
| Test Area | Tests |
|-----------|-------|
| **RoutingSlipBuilder** | Building with 0, 1, N activities; Build() returns correct itinerary order; activities preserve their names |
| **RoutingSlip entity** | Initial state is `Created`; state transitions are valid; invalid transitions throw; tracking number is unique |
| **RoutingSlipExecutor** | Sequential execution order verified; activity log populated after each success; faulted slip on exception; exception captured; all-activities-success → `Completed`; cancellation respected |
| **Itinerary** | Immutability of the read-only list; order preservation |
| **IActivity (mock)** | Mock activities return expected results; mock activities throw to test fault path |

---

## File Change List

### New Project: `Core.RoutingSlip`

| # | File Path | Layer/Purpose |
|---|-----------|---------------|
| 1 | `src/Core.RoutingSlip/Core.RoutingSlip.csproj` | Project file (net10.0, no external deps) |
| 2 | `src/Core.RoutingSlip/IActivity.cs` | Non-generic activity interface |
| 3 | `src/Core.RoutingSlip/IActivity.Generic.cs` | `IActivity<T, TResult>` (per work item spec) |
| 4 | `src/Core.RoutingSlip/ExecutionResult.cs` | Typed/no-result execution wrapper |
| 5 | `src/Core.RoutingSlip/RoutingSlipState.cs` | Enum: Created, Executing, Completed, Faulted |
| 6 | `src/Core.RoutingSlip/RoutingSlip.cs` | Main entity (tracking number, itinerary, log, state, exception) |
| 7 | `src/Core.RoutingSlip/ActivityEntry.cs` | Itinerary entry wrapping an `IActivity` |
| 8 | `src/Core.RoutingSlip/ActivityLogEntry.cs` | Completed activity record (name, timestamp) |
| 9 | `src/Core.RoutingSlip/RoutingSlipBuilder.cs` | Fluent builder: `AddActivity()` + `Build()` |
| 10 | `src/Core.RoutingSlip/IRoutingSlipExecutor.cs` | Executor interface |
| 11 | `src/Core.RoutingSlip/RoutingSlipExecutor.cs` | Sequential execution implementation |
| 12 | `src/Core.RoutingSlip/RoutingSlipException.cs` | Domain exception for routing-slip-specific errors |

### Modified Files

| # | File Path | Change |
|---|-----------|--------|
| 13 | `Ai.Api.slnx` | Add `<Project Path="src/Core.RoutingSlip/Core.RoutingSlip.csproj" />` |
| 14 | `Directory.Packages.props` | No changes needed (library has no external NuGet dependencies) |

### New Test Project: `Core.RoutingSlip.Tests`

| # | File Path | Purpose |
|---|-----------|---------|
| 15 | `tests/Core.RoutingSlip.Tests/Core.RoutingSlip.Tests.csproj` | Test project (xUnit + Shouldly) |
| 16 | `tests/Core.RoutingSlip.Tests/RoutingSlipBuilderTests.cs` | Builder tests |
| 17 | `tests/Core.RoutingSlip.Tests/RoutingSlipExecutorTests.cs` | Executor tests |

---

## Implementation Details

### 1. `Core.RoutingSlip.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>
```
No NuGet dependencies for Phase 1. Uses `Guid.CreateVersion7()` which is built into .NET 10.

### 2. `IActivity.cs` — Non-generic base interface
```csharp
namespace Core.RoutingSlip;

public interface IActivity
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
```
The itinerary stores `IActivity` instances. This allows heterogeneous activity types.

### 3. `IActivity.Generic.cs` — Typed interface (per work item)
```csharp
namespace Core.RoutingSlip;

public interface IActivity<T, TResult> : IActivity
{
    Task<TResult> ExecuteAsync(T payload, CancellationToken cancellationToken);
}
```
Concrete activities implement this. They store `T payload` from their constructor and the non-generic `IActivity.ExecuteAsync` delegates to the generic one.

### 4. `ExecutionResult.cs`
```csharp
namespace Core.RoutingSlip;

// For activities that return void
public sealed record ExecutionResult
{
    public static readonly ExecutionResult Success = new();
}

// For activities that return a value
public sealed record ExecutionResult<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; } = true;

    public static ExecutionResult<T> FromResult(T value) => new() { Value = value };
}
```

### 5. `RoutingSlipState.cs`
```csharp
namespace Core.RoutingSlip;

public enum RoutingSlipState
{
    Created,
    Executing,
    Completed,
    Faulted
}
```

### 6. `RoutingSlip.cs` — Main entity
- `Guid TrackingNumber { get; }` = `Guid.CreateVersion7()`
- `IReadOnlyList<ActivityEntry> Itinerary { get; }` — set once at build time
- `IReadOnlyList<ActivityLogEntry> ActivityLog { get; }` — grows during execution
- `RoutingSlipState State { get; private set; }` — starts as `Created`
- `Exception? FaultException { get; private set; }` — captured on fault
- Methods: `TransitionToExecuting()`, `RecordActivityCompletion(ActivityLogEntry)`, `TransitionToCompleted()`, `Fault(Exception)`
- Private parameterless constructor + internal constructor for builder

### 7. `ActivityEntry.cs`
- Wraps `IActivity Activity { get; }`
- `int Position { get; }` — 0-based index in itinerary
- Immutable; created by the builder

### 8. `ActivityLogEntry.cs`
```csharp
public sealed record ActivityLogEntry
{
    public string ActivityName { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
}
```
Uses class-like record syntax (per coding standards).

### 9. `RoutingSlipBuilder.cs`
- `AddActivity(IActivity activity)` — appends to internal list
- `Build()` — validates at least one activity (or allows empty per Q7), creates `RoutingSlip` with ordered `ActivityEntry` list
- Returns `RoutingSlip` with state `Created`

### 10. `IRoutingSlipExecutor.cs`
```csharp
public interface IRoutingSlipExecutor
{
    Task ExecuteAsync(RoutingSlip routingSlip, CancellationToken cancellationToken);
}
```

### 11. `RoutingSlipExecutor.cs`
- Validates slip is in `Created` state
- Transitions to `Executing`
- Iterates itinerary in order
- For each entry: calls `activity.ExecuteAsync(ct)`, catches exceptions → fault the slip, records log entry on success
- On completion: transitions to `Completed`

### 12. `RoutingSlipException.cs`
Domain exception for invalid state transitions, empty itineraries, etc.

---

## Implementation Order

| Step | Action | Files |
|------|--------|-------|
| 1 | Create `src/Core.RoutingSlip/` directory and `.csproj` | `Core.RoutingSlip.csproj` |
| 2 | Add project reference to `Ai.Api.slnx` | `Ai.Api.slnx` |
| 3 | Create enums and exceptions | `RoutingSlipState.cs`, `RoutingSlipException.cs` |
| 4 | Create interfaces | `IActivity.cs`, `IActivity.Generic.cs`, `IRoutingSlipExecutor.cs` |
| 5 | Create value/result types | `ExecutionResult.cs` |
| 6 | Create domain entities | `ActivityEntry.cs`, `ActivityLogEntry.cs`, `RoutingSlip.cs` |
| 7 | Create builder | `RoutingSlipBuilder.cs` |
| 8 | Create executor | `RoutingSlipExecutor.cs` |
| 9 | Create test project and tests | `Core.RoutingSlip.Tests.csproj`, test files |
| 10 | Verify build: `dotnet build` | — |
| 11 | Run tests: `dotnet test` | — |

---

## Out of Scope (Explicitly Deferred)
- Compensation / rollback logic → RS-002
- Exception logging (ILogger integration) → RS-003
- Activity subscriptions / saga-based routing slip host
- Message transport — everything is in-process
