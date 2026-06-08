# Routing Slip

Routing Slip is a pattern that allows you to define a sequence of processing steps (activities) that a message should go through. 
Each step can be executed on a different machine or service, and the routing slip keeps track of the progress and the next steps to be executed.

A routing slip contains an itinerary, variables, and activity/compensation logs. It is defined by a message contract, which the underlying routing slip components use to execute and compensate the transaction. The routing slip contract includes:

## Acceptance Criteria

- A unique tracking number for each routing slip
- An itinerary that contains an ordered list of activities
- An activity log that contains an ordered list of completed activities
- A compensation log that contains data related to completed activities that can be compensated if the routing slip faults
- A collection of exceptions that may have occurred during routing slip execution

## Detailed User Stories

| Ticket  | Feature                 | Description                                                     |
| ------- | ----------------------- | --------------------------------------------------------------- |
| RS-001  | Basic Functionality     | Forward execution engine, itinerary, variables, activity log.   |
| RS-002  | Compensations           | Reverse-order rollback of completed activities via compensate actions. |
| RS-003  | Logging Support         | Structured lifecycle and per-activity log events with correlation. |

- [RS-001 — Basic Functionality](Routing%20Slip/01_Basic_Functionality.md)
- [RS-002 — Compensations](Routing%20Slip/02_Compensations.md)
- [RS-003 — Logging Support](Routing%20Slip/03_Logging_Support.md)
