# Formating 
Formating code is important for readability and maintainability. Here are some guidelines to follow:
- Always format the code using the `.editorconfig` that is located on the root repo folder to enforce consistent formatting across the codebase.
- Functions should be small and focused on a single task. If a function is doing too much, consider breaking it down into smaller functions.
- Functions should not exceed 50 lines of code. If a function is longer than that, it may be doing too much and should be refactored.
- Recommended maximum number of code lines per file is 300. If a file exceeds that, consider breaking it down into smaller files or classes.
- DO NOT USE regions to group related functions within a class.
- DO NOT USE Minimal APIs! Use Controllers instead.
