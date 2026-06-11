# Story

As an administrator, I want to be able to manage customers in the system

## Metadata

work_item_type: feature
ticket_num: 001
feature_name: Customer Management

## Acceptance Criteria

The system should allow administrators to create, update, retrieve, and list Customers. Each Customers should have a unique identifier.
**Customers**

- POST `/customers`
- PUT `/customers/{id}`
- GET `/customers/{id}`
- GET `/customers`

## customers model
- id: (primary key) datatype: guid
- first_name: datatype: string(256)
- last_name: datatype: string(256) Traits: mandatory
- tax_id: datatype: string(16) Traits: mandatory, unique
- comments: datatype: string(1024)
