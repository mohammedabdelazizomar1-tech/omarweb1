# Exception and Error Handling Pipeline

## ExceptionHandlingMiddleware
The C# global exception middleware catches unhandled exceptions:
1. Logs trace events using Serilog, including `CorrelationId` and `TraceId` logs context.
2. Identifies the HTTP status code based on exception type:
   - `InvalidOperationException` / `ArgumentException` -> 400 Bad Request
   - `KeyNotFoundException` -> 404 Not Found
   - `UnauthorizedAccessException` -> 401 Unauthorized
   - General Exception -> 500 Internal Server Error
3. Detects AJAX calls by checking if `X-Requested-With` header is `"XMLHttpRequest"` or `Accept` contains `"application/json"`.
   - **AJAX Response**: Returns JSON `{ success = false, message = exception.Message, details = ... }`.
   - **Browser Response**: Redirects user to `/Home/Error?message={message}`.
