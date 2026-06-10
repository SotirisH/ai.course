# Story

As an administrator, I want to be able to manage applications in the system

## Metadata

work_item_type: feature
ticket_num: 001
feature_name: Application Management

## Acceptance Criteria

The system should allow administrators to create, update, retrieve, and list applications. Each application should have a unique identifier and be associated with related configuration IDs.
**Applications**

- POST `/applications`
- PUT `/applications/{id}`
- GET `/applications/{id}`
- GET `/applications`

## Applications model

- id: (primary key) datatype: guid
- name: unique datatype: string(256)
- comments: datatype: string(1024)

## Testing Requirements

### Unit Tests (tests/UnitTests/)
- **Validators**: Test CreateApplicationRequest and UpdateApplicationRequest validation rules
  - Valid input scenarios
  - Invalid input scenarios (empty name, name too long, comments too long)
  - Edge cases (boundary values for string lengths)
- **Handlers**: Test command and query handlers in isolation
  - CreateApplicationCommandHandler with mocked repository
  - UpdateApplicationCommandHandler with mocked repository
  - GetApplicationQueryHandler with mocked repository
  - GetAllApplicationsQueryHandler with mocked repository
- **Mappers**: Test request/response/DTO mapping extensions
  - Request → Command/Query mappings
  - DTO → Response mappings
  - Null handling and edge cases

### Integration Tests (tests/IntegrationTests/)
- **Repository Tests**: Test with real PostgreSQL (Testcontainers)
  - Create application and verify persistence
  - Update application and verify changes
  - Get application by ID
  - Get all applications with pagination
  - Unique constraint validation on name field
  - Database constraint violations (e.g., name length exceeds limit)
- **DbContext Tests**: Verify entity configurations and migrations
  - Entity mapping correctness
  - Index creation
  - Constraint enforcement

### E2E Tests (tests/E2ETests/)
- **API Integration Tests**: Full request/response cycle using WebApplicationFactory
  - POST /applications - Create new application (happy path)
  - GET /applications/{id} - Retrieve application by ID (happy path)
  - GET /applications - List all applications (happy path)
  - PUT /applications/{id} - Update application (happy path)
  - Verify HTTP status codes
  - Verify response body structure and content
  - Verify content-type headers
