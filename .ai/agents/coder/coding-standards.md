# Formating 
Formating code is important for readability and maintainability. Here are some guidelines to follow:
- Always format the code using the `.editorconfig` that is located on the root repo folder to enforce consistent formatting across the codebase.
- Format all code using JetBrains Rider’s default C# cleanup profile.
- Functions should be small and focused on a single task. If a function is doing too much, consider breaking it down into smaller functions.
- Functions should not exceed 50 lines of code. If a function is longer than that, it may be doing too much and should be refactored.
- Recommended maximum number of code lines per file is 300. If a file exceeds that, consider breaking it down into smaller files or classes.
- DO NOT USE regions to group related functions within a class.
- DO NOT USE Minimal APIs! Use Controllers instead.
- Use the latest features of C# to write clean and concise code.
- Primary constractors should be used for classes that as a body have logic only for the  initialization of properties or fields. This increases the readability.

---
# Records
- Use records for data transfer objects (DTOs) and other simple data structures that do not require behavior.
- Records provide built-in immutability and value-based equality, making them ideal for these use cases.
- Avoid using records for complex objects that require behavior or mutable state. In such cases, consider using classes instead, as they provide more flexibility for defining methods and properties.
- When defining records, always use the standard class-like syntax. This helps to keep the code clean and easy to read.

---
# Classes
- Primary constractors should be used for classes that as a body have logic only for the  initialization of properties or fields. This increases the readability.
