# Developer Coding Standards & Conventions

To maintain a clean and unified codebase, developers must adhere to the following standards.

## Language and Syntax Conventions
- **Language**: C# 13 and .NET 10 standards are enforced.
- **Naming Conventions**:
  - **PascalCase**: Classes, namespaces, method names, and public properties.
  - **camelCase**: Local variables, method parameters.
  - **_camelCase**: Private read-only fields.
- **Asynchronous Coding Rules**:
  - Always append the `Async` suffix to asynchronous method signatures (e.g. `SaveAsync()`).
  - Always use `ConfigureAwait(false)` in library or persistence layers.
  - Never use `.Result` or `.Wait()` to prevent thread starvation.

## Clean Architecture Rules
- Controllers must only inject Application services, never EF Core DbContexts or raw repositories.
- Entities must remain thin. Business validations are implemented inside Domain Services or Application validation blocks.
