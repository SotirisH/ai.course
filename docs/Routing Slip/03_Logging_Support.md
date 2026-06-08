# Story

As a developer or operator, I want the routing slip engine to emit structured logs at key execution points — so that I can observe, debug, and audit the progress of routing slips across distributed activities.

## Metadata

work_item_type: feature
ticket_num: RS-003
feature_name: Routing Slip — Logging Support

## Prerequisites

- RS-001 (Basic Functionality) must be complete.
- RS-002 (Compensations) should be complete so logging covers compensation paths.

## Context

Routing slips span multiple activities, each potentially running on different machines. Without structured logs, it is difficult to trace the end-to-end flow, diagnose failures, or audit completed transactions. This feature adds first-class logging events emitted by the routing slip engine at every lifecycle transition and activity boundary.

## Log Contract

All log events follow a consistent shape:

- **TrackingNumber** (`Guid`) — ties the event to a specific routing slip.
- **Timestamp** (`DateTimeOffset`) — UTC time of the event.
- **EventType** (`string`) — one of the predefined event types below.
- **Payload** (`Dictionary<string, object?>`) — event-specific data.

## Acceptance Criteria — Phase 3: Logging Support

### 3.1 Routing Slip Lifecycle Events

The engine must emit log events for every state transition:

| EventType              | Emitted When                                          | Payload                                       |
| ---------------------- | ----------------------------------------------------- | --------------------------------------------- |
| `RoutingSlipCreated`   | A routing slip is built and execution begins.          | Itinerary (activity names), initial variables |
| `RoutingSlipExecuting` | The first activity starts executing.                   | —                                             |
| `RoutingSlipCompleted` | All activities finished successfully.                 | Total duration, activity log summary          |
| `RoutingSlipFaulted`   | An activity faulted, triggering compensation.          | Faulted activity name, exception message      |
| `RoutingSlipCompensating` | Compensation walk has started.                       | Number of activities to compensate            |
| `RoutingSlipCompensated`  | All compensations completed successfully.            | Total compensation duration                   |

### 3.2 Per-Activity Events

The engine must emit events for every activity execution and compensation:

| EventType                  | Emitted When                               | Payload                                                   |
| -------------------------- | ------------------------------------------ | --------------------------------------------------------- |
| `ActivityExecuting`        | An activity is about to execute.           | Activity name, arguments                                  |
| `ActivityCompleted`        | An activity executed successfully.         | Activity name, output variables, duration                 |
| `ActivityFaulted`          | An activity threw an exception.            | Activity name, exception type, exception message, stack   |
| `ActivityCompensating`     | Compensate action is about to run.         | Activity name, compensation-log data                      |
| `ActivityCompensated`      | Compensate action completed successfully.  | Activity name, duration                                   |
| `ActivityCompensationFaulted` | Compensate action failed.               | Activity name, exception type, exception message, retry count |

### 3.3 Warning Events

The engine must emit warnings for non-exceptional but noteworthy situations:

| EventType                        | Emitted When                                                   |
| -------------------------------- | -------------------------------------------------------------- |
| `ActivityNotCompensable`         | A completed activity has no compensateAddress and is skipped.  |
| `CompensationRetry`              | A compensate action is being retried after a failure.          |
| `CompensationRetryExhausted`     | All retries for a compensate action have been exhausted.       |

### 3.4 Log Output Configuration

- Logs must be emitted through standard `ILogger<RoutingSlipEngine>` so they work with any ASP.NET logging provider (console, Serilog, Application Insights, etc.).
- Each log event must use a structured logging approach (log message templates with named parameters) so that providers can index individual fields.
- Log level mapping:
  - Lifecycle events → `Information`
  - Activity execution events → `Debug`
  - Warnings → `Warning`
  - Fault/exception events → `Error`

### 3.5 Correlation

- Every log event must include the `TrackingNumber` as both a structured property and in the message template.
- The `TrackingNumber` must propagate to all activity and compensate messages so that each service can correlate its own logs back to the routing slip.

### 3.6 Audit Trail

The `ActivityLog` and `CompensationLog` collections already on the routing slip contract serve as the persistent audit trail. After RS-003, these logs are enhanced to include timing data (start/end timestamps per entry) so they can be used for duration analysis.

## Out of Scope (Phase 3)

- Metrics / OpenTelemetry tracing (separate feature).
- Log storage and retention policies.
