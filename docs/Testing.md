# Testing Framework & Instructions

## Testing Architecture
The tests project [BusinessManagement.Tests](file:///d:/omarWeb/src/BusinessManagement.Tests) contains:
- **Unit Tests**: Asserts correct calculations of partner quotas and ledger validation checks.
- **Integration Tests**: Asserts correct execution of repositories and Entity Framework mappings using InMemory or Postgres containers.
- **Security Tests**: Confirms password hashes are generated using PBKDF2 and checks that PII details are successfully encrypted in database cells.

## Running Tests
Run the test suite using standard dotnet CLI:
```bash
dotnet test
```
