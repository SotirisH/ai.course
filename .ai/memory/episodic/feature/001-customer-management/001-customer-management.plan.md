## Metadata

- Ticket: 001
- Feature Name: Customer Management
- Work Item Type: feature

## Story summary

Administrators must be able to create, update, retrieve, delete and list customers. Each customer has a unique identifier (GUID) and a set of fields (first name, last name, tax id, comments).

## Acceptance criteria (Given / When / Then)

- Given an authenticated administrator
  - When they POST /customers with valid payload
  - Then a new Customer is created and returned (201) with its id

- Given an existing Customer id
  - When they PUT /customers/{id} with valid payload
  - Then the customer is updated (200) and returned

- Given an existing Customer id
  - When they GET /customers/{id}
  - Then the customer is returned (200)

- Given no filters
  - When they GET /customers
  - Then a list of customers is returned (200)

- Given an existing Customer id
  - When they DELETE /customers/{id}
  - Then the customer is removed (204)

## Spec issues / Consistency checks

1. Field-level ambiguity:
   - first_name has no "mandatory" trait while last_name is mandatory. Is first_name optional? (Assume: optional unless user clarifies.)
2. Missing auditing fields:
   - Story/model does not mention created_at/updated_at. Recommend adding audit fields or confirm omission.
3. List endpoint behavior unspecified:
   - No paging, sorting or filtering defined. For production-safe APIs we should add pagination and at least basic sorting; otherwise list may return large result sets.
4. Validation rules incomplete:
   - No explicit constraints for string encoding, allowed characters, or tax_id format beyond length and uniqueness.
5. Uniqueness enforcement:
   - tax_id must be unique — must be enforced at DB (unique index) and handled at repository/service level for clear errors.
6. Route parameter types:
   - Endpoints use {id} but do not specify type; controllers should constrain to GUID in route templates.
7. Authorization:
   - Story says "administrator" but no auth/roles implementation details are provided. Assume role-based authorization is already available and will be required on controller actions.
8. Plural/singular naming:
   - Model section uses plural 'customers'; domain and DTOs should use singular 'Customer'.
9. Typo / formatting:
   - DELETE line contains an extra backtick on the story file. (Minor)

These items are included as open questions below.

## Questions for clarification

1. Is first_name optional or required?
2. Should we include auditing fields (created_at, updated_at)?
3. Should GET /customers support pagination, sorting or filtering? If yes, preferred defaults (page size, sort fields)?
4. Is tax_id format constrained beyond length (e.g., numeric only)?
5. Are there soft-delete requirements or hard delete only?
6. Should we expose any additional fields (email, phone) in this feature or defer to future stories?
7. Confirm authorization: require [Authorize(Roles = "Administrator")] on controller or handled elsewhere?
8. Any internationalization concerns for name fields (normalization)?

## File change list

(Pre-scaffold detection: no matching files found — all files listed below are CREATE)

Domain
- CREATE: (no domain entities per architecture) — may add Domain exceptions if needed (e.g., CustomerNotFoundException).

Application
- CREATE: src/Ai.Api.Application/Features/Customer/DTOs/CustomerDto.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/DTOs/CreateCustomerDto.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/DTOs/UpdateCustomerDto.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/Commands/CreateCustomerCommandHandler.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/Commands/UpdateCustomerCommandHandler.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/Commands/DeleteCustomerCommandHandler.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/Queries/GetCustomerByIdQueryHandler.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Features/Customer/Queries/ListCustomersQueryHandler.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Interfaces/Repositories/ICustomerRepository.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Validators/CreateCustomerValidator.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Validators/UpdateCustomerValidator.cs  🟢 CREATE
- CREATE: src/Ai.Api.Application/Mappers/CustomerMappingExtensions.cs  🟢 CREATE

Infrastructure
- CREATE: src/Ai.Api.Infrastructure/Persistence/Entities/CustomerEntity.cs  🟢 CREATE
- CREATE: src/Ai.Api.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs (Fluent API, unique index on tax_id)  🟢 CREATE
- UPDATE: src/Ai.Api.Infrastructure/Persistence/Context/AppDbContext.cs — add DbSet<CustomerEntity> and apply configuration  🟡 Already exists — review before use
- CREATE: src/Ai.Api.Infrastructure/Repositories/CustomerRepository.cs implements ICustomerRepository (maps between DTOs and entities)  🟢 CREATE
- CREATE: src/Ai.Api.Infrastructure/Mappers/CustomerEntityMappings.cs  🟢 CREATE

API / Presentation
- CREATE: src/Ai.Api.Api/Controllers/CustomersController.cs  🟢 CREATE
- CREATE: src/Ai.Api.Api/Models/Requests/CreateCustomerRequest.cs  🟢 CREATE
- CREATE: src/Ai.Api.Api/Models/Requests/UpdateCustomerRequest.cs  🟢 CREATE
- CREATE: src/Ai.Api.Api/Models/Responses/CustomerResponse.cs  🟢 CREATE
- CREATE: src/Ai.Api.Api/Mappers/CustomerApiMappings.cs  🟢 CREATE

Database / Migrations
- CREATE: Migration to add Customers table with unique index on tax_id and appropriate columns (id guid PK, first_name, last_name, tax_id unique, comments)

Tests (recommended)
- CREATE: tests/Unit/Ai.Api.Application/CustomerTests/CreateCustomerTests.cs
- CREATE: tests/Integration/Ai.Api.Api/CustomerControllerTests.cs (using WebApplicationFactory + Testcontainers Postgres)

## Implementation details

Data model (persistence)
- CustomerEntity (Infrastructure/Persistence/Entities):
  - Id : Guid (PK)
  - FirstName : string (max 256) NULLABLE
  - LastName : string (max 256) NOT NULL
  - TaxId : string (max 16) NOT NULL, UNIQUE
  - Comments : string (max 1024) NULLABLE
  - Optionally: CreatedAt, UpdatedAt if confirmed

Fluent API (CustomerConfiguration):
- Configure property lengths, required for LastName, unique index on TaxId, table name "Customers".

Application DTOs
- CustomerDto: Id, FirstName, LastName, TaxId, Comments
- CreateCustomerDto: FirstName?, LastName, TaxId, Comments?
- UpdateCustomerDto: Id, FirstName?, LastName, TaxId, Comments?

Repository interface (ICustomerRepository)
- Task<CustomerDto> AddAsync(CreateCustomerDto dto, CancellationToken ct)
- Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct)
- Task<IEnumerable<CustomerDto>> ListAsync(CancellationToken ct) -- consider paging in future
- Task<bool> UpdateAsync(UpdateCustomerDto dto, CancellationToken ct)
- Task<bool> DeleteAsync(Guid id, CancellationToken ct)

Handlers
- Use Wolverine mediator handlers for commands / queries per architecture. Handlers validate input (FluentValidation middleware) and call repository.

API
- CustomersController : ControllerBase
  - [HttpPost] Create -> maps CreateCustomerRequest -> CreateCustomerDto -> send command -> returns 201 with Location header
  - [HttpPut("{id:guid}")] Update -> id route constraint; map and call handler
  - [HttpGet("{id:guid}")] GetById
  - [HttpGet] List -> consider optional query params for paging
  - [HttpDelete("{id:guid}")] Delete -> returns 204 on success
  - Apply [Authorize(Roles = "Administrator")] on controller (assumption)

Validation
- CreateCustomerValidator enforces LastName required, TaxId required and length <=16. Additional format rules to be clarified.

Error handling
- Repository should translate unique constraint violations into a domain/application-level validation exception (e.g., TaxIdAlreadyExists) which maps to 409 Conflict on API layer.

Migration
- Add EF Core migration to create Customers table and unique index on TaxId.

## Implementation order (recommended)

1. Application: Define DTOs, repository interface ICustomerRepository, validators (Create/Update)
2. Infrastructure: Create CustomerEntity, CustomerConfiguration
3. Infrastructure: Update AppDbContext to add DbSet and apply configuration
4. Infrastructure: Implement CustomerRepository
5. Application: Implement handlers (Create/Update/Get/List/Delete) using repository
6. API: Add Request/Response models and CustomersController
7. Migrations: Add and run migration locally (integration tests rely on DB)
8. Tests: Unit tests for handlers and integration tests for controller
9. Docs: Update API docs /OpenAPI definitions

## Assumptions (with justification)

1. first_name is optional: The model explicitly marks last_name as mandatory and first_name is not marked. Default to optional to avoid breaking clients. (If wrong, user must confirm.)
2. Auditing fields are not required: Story omitted created_at/updated_at; to keep scope minimal we will not add them unless requested. (Adding later is backward-compatible if nullable.)
3. Soft-delete not required: Story mentions delete without specifying soft-delete. Implement hard delete unless instructed otherwise.
4. Authorization: Assume role-based auth exists; controller will require Administrator role. This aligns with story text mentioning administrators.
5. No pagination by default: Story did not request it; implement list returning all items but add TODO to introduce pagination when needed. (Simple for MVP.)
6. TaxId uniqueness enforced at DB + application: Implement unique index and map DB constraint violations to 409 Conflict for clear client feedback.
7. API route id is GUID: Use route constraints {id:guid} to avoid unnecessary 404s; this follows architecture guidance.

## Next steps / Acceptance to implement

- Answer open questions above.
- Confirm assumptions or provide corrections.
- Upon confirmation, implement files per File change list and open a PR from feature/001-customer-management.

---

Generated by Planner agent.
