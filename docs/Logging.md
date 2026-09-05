# System Logging Specification

Logging is powered by Serilog and structured to ensure compliance and audit-readiness.

## Log Sensitive Data Masking
To prevent writing sensitive Personally Identifiable Information (PII) to log files:
- Destructuring transforms are applied in `Program.cs` for entities such as `User` and `Passenger`.
- Passwords, hashes, Totp secrets, passport numbers, and national IDs are redacted and replaced with `[REDACTED]` prior to file output.

## Log Structure
```json
{
  "Timestamp": "2026-07-05T12:00:00Z",
  "Level": "Information",
  "MessageTemplate": "User logged in.",
  "Properties": {
    "CorrelationId": "4bf92f3577b34da6a3ce929d0e0e4736",
    "TraceId": "0af7651916cd43dd8448eb211c80319c",
    "UserId": "a1111111-1111-1111-1111-111111111111"
  }
}
```
