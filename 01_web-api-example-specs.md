# Config Service Pre-prompt Planning

This document contains details necessary to create a prompt, which will later be used to create an implementation plan for a REST Web API.
Please review the contents of this file and recommend a PROMPT that can be sent to an AI coding assistant for help with creating a plan for 
this service. 

The prompt should:
- ask the assistant to create a comprehensive plan that includes dependencies, file/folder structure, and architectural patterns.
- recommend strict adherence to ALL of the details in this document.
- strongly encourage the assistant to not add any additional dependencies without approval.
- encourage the assistant to ask for more information if they need it.
- this prompt should be saved to a prompts/ folder structure
## Tech Stack

| Area                 | Choice     | Version |
|----------------------|------------|---------|
| Language             | .net10     | 10.0.202|
| Database engine      | PostgreSQL | v16     |

It is very IMPORTANT the prompt stress the importance of including these SPECIFIC version numbers.

## Data Models

**Application**
DB Table: applications
Columns:
  - id: (primary key) datatype: string/ULID
  - name: unique datatype: string(256)
  - comments: datatype: string(1024)

**Configuations**
DB Table: configurations
Columns:
    - id: (primary key) datatype: string/ULID
    - application_id: (foreign key) datatype: string/ULID
    - name: datatype: string(256) expected to be unique per application
    - comments: datatype: string(1024)
    - config: Dictionary with name/value pairs datatype: JSONB 

## API Endpoints

Should be prefixed with `/api/v1`

**Applications**
  - POST `/applications`
  - PUT `/applications/{id}`
  - GET `/applications/{id}` (includes list of all related configuration.ids)
  - GET `/applications`

**Configurations**
  - POST `/configurations`
  - PUT `/configurations/{id}`
  - GET `/configurations/{id}`

## Data Persistence

This project will  be using EFCore as ORM. All the SQL statements will be in the code.


## Automated Testing

- ALL code files MUST have an associated unit test (that focuses on 80% of the most important scenarios in the file it is testing.
- If we must have a `test/` folder, it should only contain test helpers, widely used mocks, and/or integration tests. Do not create this folder until it is needed.

## Dates and times

Use the most up-to-date .NET documentation for date and time operations to ensure we don't use deprecated APIs.

## Authentication

This is a future feature. We do not have to plan for this now.

## Service Configuration

Use a `.env` file to store environment variables, such as the database configuration string, logging level, etc. 

## Developer Experience

Use a `README.md` file to provide clear instructions on how to set up the development environment, run the application, and execute tests. Include any necessary commands and configurations.
