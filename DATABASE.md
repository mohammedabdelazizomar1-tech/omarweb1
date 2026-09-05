# Database Architecture & Table Reference

The system runs on **PostgreSQL** with Entity Framework Core. This document defines the actual schema, columns, indexes, and value converters implemented in the codebase.

## Value Converters & Encryption
- **PII Encryption**: Encrypted in memory using `AES-GCM` via EF ValueConverter.
  - `Passenger.PassportNumber`
  - `Passenger.NationalId`
- **Searchable Hashes**: Columns `PassportNumberHash` and `NationalIdHash` are generated using SHA-256 for index lookups.
- **Enums**: Stored as string representations (`VARCHAR`) with a length limit of 20-30 characters:
  - `User.Role` (`UserRole` Enum: `Admin`, `Manager`, `Employee`, `Partner`)
  - `SafeTransaction.TransactionType` (`TransactionType` Enum: `Deposit`, `Withdrawal`)
  - `SafeTransaction.Currency` (`Currency` Enum: `EGP`, `SAR`)

---

## Complete Table Reference

### 1. tenants
- **Columns**: `id` (UUID, PK), `name` (VARCHAR), `subdomain` (VARCHAR), `is_active` (BOOLEAN).

### 2. companies
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK -> tenants.id), `name` (VARCHAR), `commercial_register` (VARCHAR), `tax_number` (VARCHAR), `address` (VARCHAR).

### 3. branches
- **Columns**: `id` (UUID, PK), `company_id` (UUID, FK -> companies.id), `name` (VARCHAR).

### 4. departments
- **Columns**: `id` (UUID, PK), `name` (VARCHAR), `description` (VARCHAR).

### 5. users
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK -> tenants.id), `branch_id` (UUID, FK -> branches.id), `username` (VARCHAR, UNIQUE), `email` (VARCHAR), `password_hash` (VARCHAR), `first_name` (VARCHAR), `last_name` (VARCHAR), `role` (VARCHAR), `is_active` (BOOLEAN), `two_factor_enabled` (BOOLEAN), `two_factor_secret` (VARCHAR), `department_id` (UUID, FK -> departments.id, Nullable).

### 6. roles
- **Columns**: `id` (UUID, PK), `name` (VARCHAR).

### 7. user_roles
- **Columns**: `user_id` (UUID, PK/FK -> users.id), `role_id` (UUID, PK/FK -> roles.id).

### 8. permissions
- **Columns**: `id` (UUID, PK), `code` (VARCHAR), `description` (VARCHAR).

### 9. role_permissions
- **Columns**: `role_id` (UUID, PK/FK -> roles.id), `permission_id` (UUID, PK/FK -> permissions.id).

### 10. passengers
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK -> tenants.id), `full_name` (VARCHAR), `passport_number` (VARCHAR - Encrypted), `passport_number_hash` (VARCHAR), `national_id` (VARCHAR - Encrypted), `national_id_hash` (VARCHAR), `nationality` (VARCHAR), `gender` (VARCHAR), `phone_number` (VARCHAR).

### 11. bookings
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK), `branch_id` (UUID, FK), `booking_number` (VARCHAR, UNIQUE), `passenger_id` (UUID, FK -> passengers.id), `partner_id` (UUID, FK -> partners.id), `package_id` (UUID, FK -> packages.id), `travel_date` (DATE), `current_status` (VARCHAR), `selling_price` (NUMERIC), `payments_collected` (NUMERIC), `remaining_balance` (NUMERIC), `has_qr_code` (BOOLEAN).

### 12. booking_costs
- **Columns**: `id` (UUID, PK), `booking_id` (UUID, FK -> bookings.id), `net_cost` (NUMERIC), `barcode_cost` (NUMERIC), `company_markup` (NUMERIC), `airport_cost` (NUMERIC), `hotel_cost` (NUMERIC), `ticket_cost` (NUMERIC), `bus_cost` (NUMERIC).

### 13. safe_transactions
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK), `amount` (NUMERIC), `transaction_type` (VARCHAR), `currency` (VARCHAR), `exchange_rate` (NUMERIC), `description` (VARCHAR), `associated_partner_id` (UUID, FK -> partners.id, Nullable), `bank_name` (VARCHAR), `depositor_or_withdrawer_name` (VARCHAR), `transaction_date` (DATE).

### 14. external_visas
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK), `passenger_name` (VARCHAR), `affiliation` (VARCHAR), `net_cost` (NUMERIC), `barcode_cost` (NUMERIC), `agent_commission` (NUMERIC), `agreement_cost` (NUMERIC), `ticket_cost` (NUMERIC), `airport_cost` (NUMERIC), `bus_cost` (NUMERIC), `total_cost` (NUMERIC), `selling_price` (NUMERIC), `net_profit` (NUMERIC), `amount_paid` (NUMERIC), `remaining_balance` (NUMERIC).

### 15. partnership_capitals
- **Columns**: `id` (UUID, PK), `shareholder_name` (VARCHAR), `amount_sar` (NUMERIC), `amount_egp` (NUMERIC), `historical_rate` (NUMERIC), `share_ratio` (NUMERIC), `profit_share_ratio` (NUMERIC), `notes` (VARCHAR).

### 16. monthly_closings
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID, FK), `year` (INTEGER), `month` (INTEGER), `is_closed` (BOOLEAN), `closed_at` (TIMESTAMP), `closed_by` (VARCHAR).

### 17. audit_logs
- **Columns**: `id` (UUID, PK), `tenant_id` (UUID), `entity_name` (VARCHAR), `entity_id` (UUID), `changed_columns` (JSONB), `old_values` (JSONB), `new_values` (JSONB), `user_id` (VARCHAR), `created_at` (TIMESTAMP), `log_hash` (VARCHAR), `previous_log_hash` (VARCHAR).
