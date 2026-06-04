# NET/C# Best Practices
Your task is to ensure when you write any .NET/C# code, it meets the best practices. 
This includes:

## Formating 
Formating code is important for readability and maintainability. Here are some guidelines to follow:
- Always format the code using the `.editorconfig` that is located on the root repo folder to enforce consistent formatting across the codebase.
- Format all code using JetBrains Rider’s default C# cleanup profile.
- Functions should be small and focused on a single task. If a function is doing too much, consider breaking it down into smaller functions.
- Functions should not exceed 50 lines of code. If a function is longer than that, it may be doing too much and should be refactored.
- Recommended maximum number of code lines per file is 300. If a file exceeds that, consider breaking it down into smaller files or classes.
- DO NOT USE regions to group related functions within a class.
- DO NOT USE Minimal APIs! Use Controllers instead.
- Use the latest features of C# to write clean and concise code.

---
## Constructors
- Use primary constructor syntax for dependency injection (e.g., `public class MyClass(IDependency dependency)`)
- Also Primary constructors should be used for classes that as a body have logic only for the  initialization of properties or fields. This increases the readability.
---
## Records
- Use records for data transfer objects (DTOs) and other simple data structures that do not require behavior.
- Records provide built-in immutability and value-based equality, making them ideal for these use cases.
- Avoid using records for complex objects that require behavior or mutable state. In such cases, consider using classes instead, as they provide more flexibility for defining methods and properties.
- **IMPORTANT**: When defining records, **always use the standard class-like syntax** (e.g., `public sealed record Foo { public string Bar { get; init; } }`). **Never use positional syntax** (`record Foo(string Bar)`). This is a hard rule.


---
## Async/Await Patterns
- Use async/await for all I/O operations and long-running tasks
- All async methods must have suffix "Async" in their name (e.g., `GetDataAsync`) to clearly indicate that they are asynchronous.
- Return Task,Task<T> or ValueTask,ValueTask<T> from async methods
- Use ConfigureAwait(false) where appropriate
- All async methods should have an Async suffix (e.g., GetDataAsync)
- Avoid async void methods except for event handlers
- Use always cancellation tokens for async methods that may run for an extended period, be cancellable or call other methods in the chain that support cancellation tokens.
- When calling async methods, always await them to ensure proper exception handling and flow control. Avoid using .Result or .Wait() on async methods, as this can lead to deadlocks and other issues.
- When implementing async methods, ensure that all code paths are properly awaited and that exceptions are handled appropriately. This includes using try/catch blocks to catch exceptions and logging them as needed.
- When designing async APIs, consider the potential for cancellation and timeouts. Provide appropriate mechanisms for clients to cancel long-running operations and handle timeouts gracefully.
- Avoid having async methods in loops. Try to batch operations together and await them as a group to improve performance and reduce overhead.

---
## Code Quality
- Ensure SOLID principles compliance
- Avoid code duplication through base classes and utilities
- Use meaningful names that reflect domain concepts
- Keep methods focused and cohesive
- Implement proper disposal patterns for resources

---
## Error Handling & Logging
- Use structured logging with Microsoft.Extensions.Logging
- Include scoped logging with meaningful context
- Throw specific exceptions with descriptive messages
- Use try-catch blocks for expected failure scenarios

---
## Configuration & Settings
- Use strongly-typed configuration classes
- Use IOptions<T> pattern for settings

---
## Magic strings and numbers
- Avoid hardcoding values in code
- Use constants or enums for fixed values
- Consolidate magic strings/numbers in a single location for easy maintenance (eg, static class or configuration)

---
## Solution specs
- Use `.slnx` when you create a solution to ensure that the solution file is lightweight and only includes project references without any build configurations or platform targets. This promotes faster load times and better performance when working with the solution in an IDE.
- If the existing solution file is `.sln` and not `.slnx`, you should convert it to `.slnx` by creating a new solution with the same name but with the `.slnx` extension, and then adding the existing projects to the new solution. After that, you can remove the old `.sln` file from the repository.
