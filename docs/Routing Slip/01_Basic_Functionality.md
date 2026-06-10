# Story

As a developer, I want to define and execute a routing slip — a sequence of ordered activities that a message passes through
so that I can orchestrate distributed transactions across multiple services.

## Metadata
work_item_type: feature
ticket_num: RS-001
feature_name: Routing Slip — Basic Functionality

## Context

A **Routing Slip** is a distributed transaction pattern from. It defines a sequence of processing steps (called *activities*) 
that a message must pass through. Each activity can run on a different machine or service.
In our case we run everything in process.
The routing slip itself carries the itinerary, payload, and execution state, so there is no central orchestrator.

This first phase implements the core forward-execution engine: create a routing slip & run activities in order.

This is a new project(Class library) in the solution with name "Core.RoutingSlip". 

## Project Concepts

| Concept                                  | Description                                                                        |
|------------------------------------------|------------------------------------------------------------------------------------|
| Routing Slip                             | The top-level entity. Contains the itinerary & activity logs logs.      |
| Routing Slip Builder                     | A builder for creating routing slip instances.                                     |
| Routing Slip Executor                    | The component responsible for executing routing slips. It processes the itinerary and manages state transitions. |
| Tracking Number                          | A unique identifier (Guid) for each routing slip instance.                         |
| Itinerary                                | An ordered list of activity entries that define what to execute and in what order. |
| Activity                                 | A named processing step. Each activity has a logical name.                         |
| Activity Argument(payload)               | A generic typed input value passed to an activity.                                 |
| Returned Value: ExecutionResult<TResult> | A generic typed value that it is returned by an activity.           |                

## Acceptance Criteria — Phase 1: Basic Functionality

### 1.1 Routing Slip Contract

The routing slip contract (message) must contain:

- A unique **tracking number** (`Guid`).
- An **itinerary** — an ordered, read-only list of activity entries.
- An **activity log** — an ordered list recording completed activities.
- A **state** property indicating the current status: `Created`, `Executing`, `Completed`, `Faulted`.

### 1.2 Activity Entry

Each activity entry in the itinerary implements the interface `IActivity`. 
```csharp
public interface IActivity<T, TResult>
{
    Task<ActivityResult<TResult>> ExecuteAsync(CancellationToken cancellationToken);
}
```
### 1.3 Routing Slip Builder

A developer must be able to programmatically build a routing slip via a fluent builder API using these methods:
- `AddActivity(IActivity<T, TResult>)` — appends an activity to the itinerary.
- `Build()` — returns the populated routing slip contract.
Example:
```csharp
var routingSlipBuilder = new RoutingSlipBuilder();
var routingSlip = routingSlipBuilder
    .AddActivity(_reserveInventoryActivity)
    .AddActivity(_processPaymentActivity))
    .Build();
```
### 1.4 Forward Execution - Routing Slip Executor

Given a routing slip with N activities in the itinerary:

- The executor must execute activities **in order** (1 → 2 → … → N).
- The executor must execute only asynchronously.
- The faulted routing slip must capture the full Exception object.
- After an activity completes successfully:
  - The activity is appended to the **activity log** with a timestamp.
- When all activities complete, the routing slip state transitions to `Completed`.
Example:
```csharp
IRoutingSlipExecutor _executor;
var routingSlipBuilder = new RoutingSlipBuilder();
var routingSlip = routingSlipBuilder
    .AddActivity(_reserveInventoryActivity)
    .AddActivity(_processPaymentActivity))
    .Build();
await _executor.ExecuteAsync(routingSlip, CancellationToken.None);
```
### 1.5 Routing Slip State Machine

The routing slip must transition through the following states:

- Created → Executing → Completed (success path) 
- Created → Executing → Faulted (failure path).


- `Created`: The routing slip has been built but not yet executed.
- `Executing`: The routing slip is actively running through its itinerary.
- `Completed`: All activities executed successfully.
- `Faulted`: An activity failed; execution has stopped (compensation may follow in Phase 2).

### 1.6  ExecutionResult<TResult>
The `ExecutionResult<TResult>` class must encapsulate the result of an activity execution, including:
- A `TResult` property for the activity's return value.
- A `bool IsSuccess` property indicating whether the execution was successful.
- An `Exception` property that captures any exception thrown during execution (null if successful).


## Out of Scope (Phase 1)

- Compensation / rollback logic (deferred to RS-002).
- Exception logging (deferred to RS-003).
- Activity subscriptions / saga-based routing slip host.
- Message transport (the routing slip and activities are modeled as in-process contracts for now).
