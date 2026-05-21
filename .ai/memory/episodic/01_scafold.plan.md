## Plan: Scaffold Clean Architecture Projects

Create 4 empty .NET 10 projects following clean architecture layers (Domain, Application, Infrastructure, API) under `src/`, wire up project references in correct dependency order, and register all projects in the solution file.

### Steps
1. Create `src/Ai.Api.Domain` class library targeting .NET 10 with no dependencies for domain entities and business logic
2. Create `src/Ai.Api.Application` class library targeting .NET 10, adding project reference to [Ai.Api.Domain](I:\GitRepo\ai.course\src\Ai.Api.Domain\Ai.Api.Domain.csproj) for application use cases
3. Create `src/Ai.Api.Infrastructure` class library targeting .NET 10, adding project references to Application and Domain for data access and external services
4. Create `src/Ai.Api` Web API project targeting .NET 10 with controllers (per [coding-style.md](I:\GitRepo\ai.course\.ai\rules\coding-style.md)), adding project references to Application and Infrastructure
5. Update [Ai.Api.slnx](I:\GitRepo\ai.course\Ai.Api.slnx) to include all four `.csproj` paths using `<Project Path="..." />` elements within the `<Solution>` root

### Further Considerations
1. Verify slnx XML schema — the `<Project>` element may require a `Type` attribute or specific path format (relative vs absolute)
2. The API project template may generate Minimal API code by default — ensure controller-based scaffolding or remove Minimal API artifacts
3. Consider running `dotnet format` after creation to align with the root [.editorconfig](I:\GitRepo\ai.course\.editorconfig)
4. Project naming: API project as `Ai.Api` (matches solution) while others use `Ai.Api.*` prefix — confirm this convention is acceptable
