# Configuration File Reference

The main configurations are managed in `appsettings.json` and environmental variables.

## Core Environment Variables
- `DefaultConnection`: PostgreSQL database connection string.
- `APP_AUDIT_LEDGER_KEY`: Symmetric signing key for hash chaining (Default: `SecureDefaultAuditLedgerKey123!@#`).
- `CLAMAV_HOST` / `CLAMAV_PORT`: ClamAV scanning daemon configurations.
- `CLAMAV_FAIL_CLOSED`: Set to `true` to reject file uploads if ClamAV socket scanner is offline.

## Database-Backed Data Protection Keys
We register a custom `IXmlRepository` (`CustomDbXmlRepository.cs`) which intercepts key generation and writes the key XML directly into the `Settings` table under `DPKey:...` to share cookie session decryption keys across load-balanced nodes.
