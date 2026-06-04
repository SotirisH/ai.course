# Overview

This document outlines the technology stack used in our projects, including programming languages, frameworks, libraries, and tools.
The chosen tech stack is designed to ensure scalability, maintainability, and efficiency in our development processes.

# Packages
- Use [central package management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) to maintain consistent versions across projects and simplify updates.
- Use the latest stable versions of all packages to ensure access to the latest features and security updates.
- If a package is no longer maintained or has known security vulnerabilities, consider alternatives that are actively maintained and secure.
- If a package license is not free anymore, ask the user what to do. Options:
  * Find an alternative package that is free and provides similar functionality.
  * Use the latest free version.
  * Use the latest version(lincesed will be required).

# Frontend
- Blazor WebAssembly: A framework for building interactive web applications using C# and .NET. It allows for a rich user experience while leveraging the power of .NET on the client side.
- HTML/CSS: Standard technologies for structuring and styling web pages.
- JavaScript: Used for enhancing interactivity and integrating third-party libraries when necessary.
- Bootstrap: A popular CSS framework for responsive design and pre-built UI components.

# Backend
## General
- .NET: A free, open-source development platform for building a wide range of applications.
- Entity Framework Core: A modern object-relational mapping (ORM) framework for .NET.
- WolverineFx: A microservices framework for .NET that simplifies the development of distributed applications.
- WolverineFx.FluentValidation: An extension for WolverineFx that integrates FluentValidation for robust validation of models and requests.

### ⚠️ WolverineFx Namespace Quirks
- **Package vs Namespace**: NuGet packages use `WolverineFx.*` naming (e.g., `WolverineFx`, `WolverineFx.FluentValidation`) but the C# namespaces drop the `Fx` suffix (e.g., `using Wolverine;`, `using Wolverine.FluentValidation;`). Always use the non-`Fx` namespace in code.
- **`UseWolverine()` vs `ConfigureWolverine()`**: `UseWolverine()` is an `IHostBuilder` extension (use in `AddApplication()` via `host.UseWolverine()`). `ConfigureWolverine()` is an `IServiceCollection` extension (use in `Program.cs` for `AlwaysUseServiceLocationFor<T>()`). Do NOT call `UseWolverine()` on `IServiceCollection`.
- **FluentValidation Exception**: Wolverine's `.UseFluentValidation()` middleware throws standard `FluentValidation.ValidationException` (not a Wolverine-specific type). Catch `FluentValidation.ValidationException` when handling validation errors.

## API
- ASP.NET Core Web API: A framework for building RESTful APIs using .NET.
- Native OpenAPI + Scalar (Recommended Modern Alternative): A modern approach to API design and documentation, providing a more efficient and flexible way to define and consume APIs.

## Database
- PostgreSQL is the default database provider. Use `Npgsql.EntityFrameworkCore.PostgreSQL` for EF Core integration. Only use a different provider if explicitly specified in the story requirements.

## Testing
- Shouldly: A popular validation library for .NET that provides a fluent API for defining validation rules.
