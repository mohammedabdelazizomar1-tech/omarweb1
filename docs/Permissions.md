# System Permissions Matrix

Permissions are represented by unique strings which are assigned to roles and enforced in Controllers or Views.

## Core Permissions List
- `bookings.create` / `bookings.edit` / `bookings.delete`
- `passengers.manage`
- `accounting.post` / `accounting.closing`
- `safe.deposit` / `safe.withdraw`
- `reports.view`
- `db.manage` (Restricted to Admin role only)

## Permissions Matrix
| Feature / Action | Admin | Manager | Operator | Partner | Accountant |
| ---------------- | :---: | :---: | :------: | :-----: | :--------: |
| Edit Bookings    |  Yes  |  Yes  |   Yes    |   Own   |     No     |
| Post Journal JE  |  Yes  |  Yes  |    No    |   No    |    Yes     |
| View Reports     |  Yes  |  Yes  |    No    |   No    |    Yes     |
| Manage DB Stats  |  Yes  |  No   |    No    |   No    |     No     |
| Safe Operations  |  Yes  |  Yes  |    No    |   No    |    Yes     |
