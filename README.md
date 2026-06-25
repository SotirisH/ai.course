# Overview

An ai tranning project. It implements a REST Web API

## Reference Implementation

When implementing new CRUD features, use **ApplicationManagement** as the canonical reference implementation. It demonstrates the established patterns across all layers:

| Layer | Reference Files |
|-------|----------------|
| **Application** | `src/Ai.Api.Application/Features/ApplicationManagement/` — Commands, Queries, DTOs |
| **Infrastructure** | `src/Ai.Api.Infrastructure/Persistence/Repositories/ApplicationRepository.cs` — Repository pattern |
| **API** | `src/Ai.Api/Controllers/ApplicationsController.cs` — Controller + Wolverine `IMessageBus` pattern |
| **Mapping** | `src/Ai.Api/Mappers/ApplicationMappingExtensions.cs` — Request ↔ Command ↔ DTO mapping |

Key patterns to follow:
- **Wolverine `IMessageBus`** mediator pattern (commands + handlers in same file)
- **FluentValidation** with `CreateXxxCommandValidator` / `UpdateXxxCommandValidator` naming
- **EF Core Fluent API** configuration with snake_case DB columns, unique indexes
- **`record` types** with standard class-like syntax (no positional records)
- **`Guid.CreateVersion7()`** for primary keys
- **`ExceptionHandlingMiddleware`** — throw `InvalidOperationException` with "was not found" / "already exists" messages for 404/409
