# FAQ (Frequently Asked Questions)

### Q: Why does the system use a separate database table for `safe_transactions`?
**A**: To allow real-time currency management (EGP and SAR cash registers) without running expensive ledger queries on the general journal lines.

### Q: How can I change the default profit share ratio of a partner?
**A**: Change the ratio in the `partnership_capital` screen. The new ratios will be used for all future profit distributions. Historical profits are locked.

### Q: Does the application support multi-tenancy?
**A**: Yes. Core operational tables contain a `TenantId` column, and database queries are automatically filtered using EF Core Global Query Filters.
