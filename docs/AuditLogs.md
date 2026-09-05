# Cryptographic Immutability & Verifier

## DbContext Interceptor Auditing
Auditing is handled inside `BusinessManagementDbContext.SaveChangesAsync()`:
1. **Delta Extraction**: Compares originals and current property values for modified `BaseEntity` instances, ignoring `xmin`, `Version`, `UpdatedAt`, and `UpdatedBy`.
2. **Genesis Hash**: If no logs exist, starts chain with genesis hash: `0000000000000000000000000000000000000000000000000000000000000000`.
3. **Log Signing**: Computes HMAC-SHA256 hash using environmental key `APP_AUDIT_LEDGER_KEY` over string format:
   `previousHash|createdAt|userId|entityName|entityId|newValues`
4. **PG Immutability**: Implemented Postgres system triggers blocking `UPDATE` or `DELETE` on `audit_logs` and `activity_logs`.

## AuditIntegrityVerifier
An administrative service verifying integrity:
- Checks if the calculated hash matches the `LogHash`.
- Uses a checkpoint record in `Settings` (`AuditVerificationCheckpoint`) to avoid scanning the entire database from scratch.
