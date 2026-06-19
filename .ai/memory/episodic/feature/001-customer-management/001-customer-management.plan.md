# Implementation Plan: Customer Management

## Metadata

- **Ticket**: 001
- **Feature Name**: Customer Management
- **Work Item Type**: feature

---

## Story Summary

As an administrator, I want to be able to manage customers in the system through a complete set of CRUD operations. The system must support creating, updating, retrieving (single and list), and deleting customers. Each customer must have a unique identifier and a unique tax ID.

---

## Acceptance Criteria (Given-When-Then)

1. **Create Customer**
   - **Given** I am an administrator
   - **When** I POST to `/customers` with valid customer data (last_name, tax_id required)
   - **Then** a new customer is created with a unique GUID and the customer details are returned

2. **Update Customer**
   - **Given** I am an administrator
   - **When** I PUT to `/customers/{id}` with valid customer data
   - **Then** the existing customer is updated with the new information

3. **Retrieve Single Customer**
   - **Given** I am an administrator
   - **When** I GET `/customers/{id}` with a valid customer ID
   - **Then** the customer details are returned

4. **Retrieve All Customers**
   - **Given** I am an administrator
   - **When** I GET `/customers`
   - **Then** a list of all customers in the system is returned

5. **Delete Customer**
   - **Given** I am an administrator
   - **When** I DELETE `/customers/{id}` with a valid customer ID
   - **Then** the customer is permanently removed from the system

---

## Spec Consistency Check

### ✅ Consistent
- Story mentions managing customers with CRUD operations
- Acceptance criteria lists all 5 endpoints (Create, Update, Get by ID, List, Delete)
- Model is defined with all required fields
- Each Customer has a unique identifier (id as GUID)
- tax_id is marked as unique and mandatory (appropriate for tax identifiers)

### ⚠️ Potential Issues / Ambiguities
1. **first_name field**: Not marked as mandatory in the model, but last_name is. Need clarification if this is intentional (some cultures may not use first names or may have single names).
2. **Pagination**: No mention of pagination for GET `/customers` - need clarification for production scenarios with large datasets.
3. **Delete type**: Not specified if this should be soft delete (retain for audit) or hard delete (permanent removal).
4. **Partial updates**: Only PUT is mentioned - should we also support PATCH for partial updates?

---

## File Change List

### 🔵 Domain Layer
*No files required* - No custom exceptions or domain enums specified in the story.

### 🔵 Application Layer

#### Features/Customers
- 🆕 **CREATE** `Features/Customers/Commands/CreateCustomerCommandHandler.cs`
  - Contains: `CreateCustomerCommand` record, `CreateCustomerCommandHandler` class
  - Validates input, calls repository, returns CustomerDto

- 🆕 **CREATE** `Features/Customers/Commands/UpdateCustomerCommandHandler.cs`
  - Contains: `UpdateCustomerCommand` record, `UpdateCustomerCommandHandler` class
  - Validates input, checks existence, calls repository, returns CustomerDto

- 🆕 **CREATE** `Features/Customers/Commands/DeleteCustomerCommandHandler.cs`
  - Contains: `DeleteCustomerCommand` record, `DeleteCustomerCommandHandler` class
  - Checks existence, calls repository delete

- 🆕 **CREATE** `Features/Customers/Queries/GetCustomerQueryHandler.cs`
  - Contains: `GetCustomerQuery` record, `GetCustomerQueryHandler` class
  - Retrieves single customer by ID, returns CustomerDto or null

- 🆕 **CREATE** `Features/Customers/Queries/GetCustomersQueryHandler.cs`
  - Contains: `GetCustomersQuery` record, `GetCustomersQueryHandler` class
  - Retrieves all customers, returns List<CustomerDto>

#### DTOs
- 🆕 **CREATE** `Features/Customers/DTOs/CustomerDto.cs`
  - Full customer representation with all fields
  - Used for responses from queries and commands

- 🆕 **CREATE** `Features/Customers/DTOs/CreateCustomerDto.cs`
  - Input DTO for creating customers
  - Fields: FirstName, LastName, TaxId, Comments

- 🆕 **CREATE** `Features/Customers/DTOs/UpdateCustomerDto.cs`
  - Input DTO for updating customers
  - Fields: Id, FirstName, LastName, TaxId, Comments

#### Validators
- 🆕 **CREATE** `Validators/CreateCustomerValidator.cs`
  - FluentValidation validator for CreateCustomerDto
  - Rules: LastName required + max 256 chars, TaxId required + max 16 chars, FirstName max 256 chars (optional), Comments max 1024 chars

- 🆕 **CREATE** `Validators/UpdateCustomerValidator.cs`
  - FluentValidation validator for UpdateCustomerDto
  - Rules: Same as create + Id validation

#### Interfaces
- 🆕 **CREATE** `Interfaces/Repositories/ICustomerRepository.cs`
  - Methods: AddAsync(CreateCustomerDto), UpdateAsync(UpdateCustomerDto), GetByIdAsync(Guid), GetAllAsync(), DeleteAsync(Guid), ExistsByTaxIdAsync(string, Guid?)

### 🔵 Infrastructure Layer

#### Persistence/Entities
- 🆕 **CREATE** `Persistence/Entities/CustomerEntity.cs`
  - EF Core entity representing the Customers table
  - Properties: Id (Guid, PK), FirstName (string?), LastName (string), TaxId (string), Comments (string?)

#### Persistence/Configurations
- 🆕 **CREATE** `Persistence/Configurations/CustomerEntityConfiguration.cs`
  - Fluent API configuration for CustomerEntity
  - Configures: Table name, primary key, column types/lengths, unique constraint on TaxId, required fields

#### Persistence/Repositories
- 🆕 **CREATE** `Persistence/Repositories/CustomerRepository.cs`
  - Implements ICustomerRepository
  - Methods use extension mappers to convert between DTOs and entities
  - Handles uniqueness check for TaxId

#### Mappers
- 🆕 **CREATE** `Mappers/CustomerMapperExtensions.cs`
  - Extension methods for mapping between DTOs and entities
  - ToEntity(), ToDto() methods

#### Migrations
- 🆕 **CREATE** `Migrations/{timestamp}_CreateCustomersTable.cs` (EF Core generated)
  - Creates Customers table with proper schema
  - Adds unique index on TaxId

### 🔵 API Layer

#### Controllers
- 🆕 **CREATE** `Controllers/CustomersController.cs`
  - REST controller with 5 endpoints:
    - POST /customers → CreateCustomer
    - PUT /customers/{id} → UpdateCustomer
    - GET /customers/{id} → GetCustomer
    - GET /customers → GetCustomers
    - DELETE /customers/{id} → DeleteCustomer
  - Uses Wolverine mediator to send commands/queries
  - Uses ActionResult<T> for all responses
  - Implements proper HTTP status codes (200, 201, 204, 400, 404)

#### Models/Requests
- 🆕 **CREATE** `Models/Requests/CreateCustomerRequest.cs`
  - API request model for creating customers
  - Fields: FirstName, LastName, TaxId, Comments

- 🆕 **CREATE** `Models/Requests/UpdateCustomerRequest.cs`
  - API request model for updating customers
  - Fields: FirstName, LastName, TaxId, Comments (Id comes from route)

#### Models/Responses
- 🆕 **CREATE** `Models/Responses/CustomerResponse.cs`
  - API response model for customer data
  - Fields: Id, FirstName, LastName, TaxId, Comments

#### Mappers
- 🆕 **CREATE** `Mappers/CustomerMapperExtensions.cs`
  - Extension methods for mapping between API models and DTOs
  - ToCommand(), ToDto(), ToResponse() methods

---

## Implementation Details

### Technology Stack
- **Framework**: .NET 10+ / C# 14+
- **Architecture**: Clean Architecture with 4 layers
- **CQRS**: Wolverine mediator for commands and queries
- **Validation**: FluentValidation integrated with Wolverine middleware
- **ORM**: Entity Framework Core with PostgreSQL (Npgsql provider)
- **Repository Pattern**: Interfaces in Application, implementations in Infrastructure
- **API**: ASP.NET Core Web API with Controllers (no Minimal APIs)

### Key Design Decisions

1. **DTO-Based Repository Pattern**
   - Repositories accept and return DTOs, never domain entities
   - Infrastructure layer maps between persistence entities and DTOs internally
   - Application layer remains decoupled from persistence concerns

2. **CQRS with Wolverine**
   - Separate command and query handlers for clear separation of concerns
   - Commands modify state (Create, Update, Delete)
   - Queries retrieve data (Get, GetAll)
   - Each handler focuses on a single responsibility

3. **Validation Strategy**
   - FluentValidation validators in Application layer
   - Integrated with Wolverine middleware for automatic validation
   - Validation occurs before handler execution
   - Returns 400 Bad Request with validation details on failure

4. **API Contract Separation**
   - API layer defines its own request/response models
   - Not exposing Application DTOs directly to clients
   - Provides flexibility to evolve internal and external contracts independently

5. **Uniqueness Constraint**
   - TaxId must be unique across all customers
   - Enforced at database level via unique index
   - Additional check in repository before insert/update to provide friendly error messages
   - Update operations exclude current customer ID from uniqueness check

6. **Error Handling**
   - 404 Not Found when customer doesn't exist (Get, Update, Delete)
   - 400 Bad Request for validation failures
   - 409 Conflict for duplicate TaxId attempts
   - Proper use of ActionResult<T> for type-safe responses

7. **ID Generation**
   - Use `Guid.CreateVersion7()` for customer IDs
   - Version 7 GUIDs are time-ordered, improving database index performance

### Database Schema

**Table: Customers**

| Column      | Type          | Constraints                  |
|-------------|---------------|------------------------------|
| Id          | UUID (GUID)   | PRIMARY KEY                  |
| FirstName   | VARCHAR(256)  | NULLABLE                     |
| LastName    | VARCHAR(256)  | NOT NULL                     |
| TaxId       | VARCHAR(16)   | NOT NULL, UNIQUE             |
| Comments    | VARCHAR(1024) | NULLABLE                     |

**Indexes:**
- Primary Key on Id (automatic)
- Unique Index on TaxId

### Validation Rules

**CreateCustomerValidator / UpdateCustomerValidator:**
- `LastName`: Required, MaxLength(256)
- `TaxId`: Required, MaxLength(16), Must be unique (checked via repository)
- `FirstName`: Optional, MaxLength(256) when provided
- `Comments`: Optional, MaxLength(1024) when provided

### API Endpoints Specification

#### 1. Create Customer
- **Endpoint**: `POST /customers`
- **Request Body**: CreateCustomerRequest (JSON)
- **Success Response**: 201 Created with CustomerResponse and Location header
- **Error Responses**: 400 Bad Request (validation), 409 Conflict (duplicate TaxId)

#### 2. Update Customer
- **Endpoint**: `PUT /customers/{id:guid}`
- **Request Body**: UpdateCustomerRequest (JSON)
- **Success Response**: 200 OK with CustomerResponse
- **Error Responses**: 400 Bad Request (validation), 404 Not Found, 409 Conflict (duplicate TaxId)

#### 3. Get Customer
- **Endpoint**: `GET /customers/{id:guid}`
- **Success Response**: 200 OK with CustomerResponse
- **Error Responses**: 404 Not Found

#### 4. Get All Customers
- **Endpoint**: `GET /customers`
- **Success Response**: 200 OK with List<CustomerResponse>
- **Notes**: Returns empty array if no customers exist

#### 5. Delete Customer
- **Endpoint**: `DELETE /customers/{id:guid}`
- **Success Response**: 204 No Content
- **Error Responses**: 404 Not Found

---

## Implementation Order

### Phase 1: Application Layer Foundation
1. Create `Features/Customers/DTOs/CustomerDto.cs`
2. Create `Features/Customers/DTOs/CreateCustomerDto.cs`
3. Create `Features/Customers/DTOs/UpdateCustomerDto.cs`
4. Create `Interfaces/Repositories/ICustomerRepository.cs`
5. Create `Validators/CreateCustomerValidator.cs`
6. Create `Validators/UpdateCustomerValidator.cs`

### Phase 2: Application Layer Handlers
7. Create `Features/Customers/Commands/CreateCustomerCommandHandler.cs`
8. Create `Features/Customers/Commands/UpdateCustomerCommandHandler.cs`
9. Create `Features/Customers/Commands/DeleteCustomerCommandHandler.cs`
10. Create `Features/Customers/Queries/GetCustomerQueryHandler.cs`
11. Create `Features/Customers/Queries/GetCustomersQueryHandler.cs`

### Phase 3: Infrastructure Layer
12. Create `Persistence/Entities/CustomerEntity.cs`
13. Create `Persistence/Configurations/CustomerEntityConfiguration.cs`
14. Create `Mappers/CustomerMapperExtensions.cs`
15. Create `Persistence/Repositories/CustomerRepository.cs`
16. Register `ICustomerRepository` in Infrastructure DI
17. Generate and apply EF Core migration for Customers table

### Phase 4: API Layer
18. Create `Models/Requests/CreateCustomerRequest.cs`
19. Create `Models/Requests/UpdateCustomerRequest.cs`
20. Create `Models/Responses/CustomerResponse.cs`
21. Create `Mappers/CustomerMapperExtensions.cs`
22. Create `Controllers/CustomersController.cs`

### Phase 5: Testing & Validation
23. Manual testing of all 5 endpoints
24. Verify validation rules work correctly
25. Verify uniqueness constraint on TaxId
26. Verify proper HTTP status codes
27. Verify error handling (404, 400, 409)

---

## Assumptions

### 1. **first_name is Optional**
**Justification**: The model specification marks `last_name` as mandatory but does not mark `first_name` as mandatory. This suggests intentional optionality. Some cultures use single names, or customers might be organizations. However, this should be confirmed with the product owner.

### 2. **Administrator Authorization is Handled Externally**
**Justification**: The story mentions "As an administrator" but provides no authentication/authorization details. Assuming this is handled by existing middleware or is out of scope for this feature. The API will focus on functionality, not auth.

### 3. **No Pagination for GET /customers Initially**
**Justification**: The acceptance criteria do not mention pagination, filtering, or sorting. Assuming a simple list endpoint for the initial implementation. Pagination can be added as a follow-up enhancement if needed.

### 4. **Hard Delete (Permanent Removal)**
**Justification**: The story says "delete" without specifying soft delete. Assuming hard delete (permanent removal) unless otherwise specified. No audit trail requirement is mentioned.

### 5. **Full Update with PUT (No PATCH)**
**Justification**: Only PUT is mentioned in the acceptance criteria. Assuming full resource replacement. Partial updates (PATCH) can be added later if needed.

### 6. **Standard REST Conventions**
**Justification**: Following standard REST practices for endpoint design, HTTP methods, and status codes unless specifically contradicted by requirements.

### 7. **No Search/Filter Capabilities Initially**
**Justification**: The GET `/customers` endpoint is described as returning a list without any query parameters. Assuming no filtering, searching, or sorting in the first iteration.

### 8. **TaxId Format Not Validated Beyond Length**
**Justification**: The model specifies max length 16 but doesn't specify format (e.g., SSN, EIN, international formats). Assuming any alphanumeric string up to 16 characters is acceptable unless specific format rules are provided.

### 9. **No Concurrent Update Protection**
**Justification**: No mention of optimistic concurrency control (e.g., ETags, version numbers). Assuming last-write-wins for updates.

### 10. **English Language for Names/Comments**
**Justification**: No internationalization requirements specified. Assuming standard UTF-8 string storage without specific locale handling.

---

## Questions for Clarification

### 1. **Is first_name Optional or Mandatory?**
**Context**: The model marks `last_name` as mandatory but `first_name` has no such trait. Is this intentional to support single-name customers or organizations?
**Impact**: Affects validation rules and API documentation.

### 2. **Should GET /customers Support Pagination?**
**Context**: For production scenarios with potentially thousands of customers, returning all at once may cause performance issues.
**Impact**: Affects API design, repository interface, and query handler. Would add optional query parameters like `?page=1&pageSize=20`.

### 3. **Should We Implement Soft Delete or Hard Delete?**
**Context**: Soft delete retains data with a "deleted" flag for audit purposes. Hard delete permanently removes records.
**Impact**: Affects database schema (needs DeletedAt column for soft delete), repository implementation, and query filters.

### 4. **Are There Specific Error Messages or Validation Rules?**
**Context**: Beyond the basic constraints (required fields, max lengths, uniqueness), are there any specific validation rules for TaxId format, name characters, etc.?
**Impact**: Affects validator implementation and user experience.

### 5. **Should We Support Partial Updates (PATCH)?**
**Context**: PUT typically replaces the entire resource. PATCH allows updating only specific fields.
**Impact**: Requires additional endpoint, command, and handling logic. More flexible for clients.

### 6. **Is There a Preferred TaxId Format or Pattern?**
**Context**: Tax IDs vary by country (SSN, EIN, VAT, etc.). Should we validate format or accept any string?
**Impact**: Affects validation rules and potentially error messages.

### 7. **Should the System Prevent Updating a Customer's TaxId?**
**Context**: Tax IDs are typically immutable once assigned. Should updates be allowed to change the TaxId field?
**Impact**: Affects update logic and validation. May require separate endpoint for TaxId correction with audit trail.

### 8. **Are There Any Audit Requirements?**
**Context**: Should we track who created/modified records and when?
**Impact**: Requires additional fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) in entity and database schema.

---

## Risk Assessment

### Low Risk
- Standard CRUD operations with well-established patterns
- Clean Architecture structure provides clear boundaries
- Wolverine and FluentValidation are mature, well-documented libraries

### Medium Risk
- TaxId uniqueness enforcement across concurrent requests (mitigated by database unique constraint + pre-check)
- Performance of GET /customers without pagination (mitigated by assuming low initial volume)

### High Risk
- None identified at this stage

---

## Dependencies

- **External**: None (self-contained feature)
- **Internal**: 
  - Requires AppDbContext to be properly configured in Infrastructure layer
  - Requires Wolverine to be configured with FluentValidation middleware
  - Requires proper DI registration in Infrastructure and API layers

---

## Testing Strategy

### Unit Tests (Application Layer)
- Test each command/query handler with mocked repositories
- Test validators with various valid/invalid inputs
- Test edge cases (empty strings, max lengths, null values)

### Integration Tests (Infrastructure Layer)
- Test repository methods with real database (Testcontainers)
- Verify TaxId uniqueness constraint enforcement
- Test entity-to-DTO mappings

### E2E Tests (API Layer)
- Test all 5 endpoints with WebApplicationFactory
- Verify proper HTTP status codes
- Test validation error responses
- Test 404 scenarios
- Test conflict scenarios (duplicate TaxId)

---

## Future Enhancements (Out of Scope)

1. Pagination, filtering, and sorting for GET /customers
2. Search functionality (by name, TaxId)
3. Soft delete with audit trail
4. PATCH endpoint for partial updates
5. Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
6. Optimistic concurrency control
7. Bulk operations (create/update/delete multiple)
8. Export functionality (CSV, Excel)
9. Advanced TaxId format validation by country
10. Customer activity history/audit log

---

## Completion Checklist

- [ ] All Application layer files created
- [ ] All Infrastructure layer files created
- [ ] All API layer files created
- [ ] EF Core migration generated and applied
- [ ] Repository registered in DI
- [ ] All 5 endpoints tested manually
- [ ] Validation rules verified
- [ ] TaxId uniqueness constraint tested
- [ ] Error handling verified (404, 400, 409)
- [ ] Code formatted and follows coding standards
- [ ] Documentation updated (if applicable)
