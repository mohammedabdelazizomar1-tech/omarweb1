# Performance Optimization Specifications

## 1. Entity Framework Core Query Optimizations
- **AsNoTracking()**: Applied on all read-only queries (dashboard counters, report grids) to prevent memory allocation in the EF tracking cache.
- **Explicit Includes**: Restricts join queries and prevents $N+1$ query issues.
- **Partial/Filtered Indexes**: Indexes are defined as partial (e.g. `CREATE INDEX ... WHERE is_deleted = FALSE`) to reduce index database tree sizes.

## 2. Server Caching
- **MapStaticAssets()**: Leverages the new .NET 10 feature to automatically optimize static files (compress, cache, and serve with fingerprints).
- **Chart.js Optimization**: Dashboard feeds are loaded asynchronously via dynamic JSON endpoints, preventing main page blockages.
