# Story

As a developer, I want routing slips to support **compensation** (rollback) — so that when a step fails partway through a multi-step transaction, previously completed steps can be undone in reverse order.

## Metadata

work_item_type: feature
ticket_num: RS-002
feature_name: Routing Slip — Compensations

## Prerequisites

- RS-001 (Basic Functionality) must be complete.

## Context

A key benefit of the Routing Slip pattern is **compensation**. Unlike a traditional two-phase commit, compensation does not use locks. Instead, each activity can optionally have a corresponding *compensate* action. When an activity fails, the routing slip engine walks backward through the activity log and invokes the compensate action for each previously completed activity — in reverse order.

This allows the system to gracefully undo partial work (e.g., cancel a reservation, refund a payment) without a central coordinator.

## Domain Concepts (New)

| Concept              | Description                                                                                         |
| -------------------- | --------------------------------------------------------------------------------------------------- |
| Compensate Address   | The endpoint URI where an activity's *compensate* logic lives (e.g., `queue:reserve-inventory_compensate`). |
| Compensation Log     | Records data needed by compensate actions. Populated by each completed activity's output.           |
| Faulted State        | The routing slip enters `Faulted` when an activity throws; compensation then begins.                |
| Compensated State    | Terminal state after all compensations have run successfully.                                       |

## Acceptance Criteria — Phase 2: Compensations

### 2.1 Compensate Address on Activity Entry

The activity entry in the itinerary must be extended with:

- An optional **compensateAddress** (`Uri?`) — the endpoint URI where the compensation logic lives. If absent, the activity cannot be compensated.

The fluent builder API is extended:

```csharp
builder
    .AddActivity("ReserveInventory", new Uri("queue:reserve-inventory_execute"),
                 compensateAddress: new Uri("queue:reserve-inventory_compensate"))
```

### 2.2 Compensation Log

- When an activity completes successfully, its output variables are stored in the **compensation log** entry (not just the activity log).
- The compensation log entry includes:
  - The activity **name**.
  - A timestamp.
  - The **variables** snapshot that was produced by this activity — these are the values needed by the compensate action.

### 2.3 Compensation Execution

When any activity in the itinerary throws/faults:

1. The routing slip state transitions to `Faulted`.
2. Compensation begins: the engine walks **backwards** through the activity log (last completed activity → first completed activity).
3. For each completed activity:
   - If it has a `compensateAddress`, the engine dispatches a compensate message to that address.
   - The compensate message includes:
     - The routing slip **tracking number**.
     - The **compensation log entry** data for that activity.
   - The compensate action runs and returns success/failure.
   - If compensate succeeds, the activity is marked as compensated.
   - If compensate **fails**, the engine retries compensation for that activity (retry count configurable per routing slip).
4. After all compensable activities have been compensated, the routing slip transitions to `Compensated`.
5. If compensation itself fails irrecoverably, the routing slip stays in `Faulted` and records the exception.

### 2.4 Non-Compensable Activities

- Activities without a `compensateAddress` are **skipped** during the compensation walk.
- The engine logs a warning (see RS-003) that the activity is non-compensable and continues.

### 2.5 State Transitions (Extended)

```
Created → Executing → Completed
                    → Faulted → Compensating → Compensated
                                              → Faulted  (if compensation fails)
```

### 2.6 Builder Extensions

- `AddActivity(name, executeAddress, compensateAddress)` — adds an activity with optional compensation.
- `AddCompensableActivity(name, executeAddress, compensateAddress)` — explicit sugar for activities that support compensation.
- `WithRetryCount(int)` — sets the maximum number of compensation retries per activity (default: 3).

## Out of Scope (Phase 2)

- Logging / observability (deferred to RS-003).
- Partial compensation custom strategies.
- Compensation timeouts.
