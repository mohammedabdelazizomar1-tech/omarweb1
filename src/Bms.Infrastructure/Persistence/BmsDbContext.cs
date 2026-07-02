using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;
using Bms.Infrastructure.Security;

namespace Bms.Infrastructure.Persistence;

public class BmsDbContext : DbContext
{
    private readonly string _currentUsername; // Normally injected via HTTP Context accessor

    public BmsDbContext(DbContextOptions<BmsDbContext> options) : base(options)
    {
        _currentUsername = "system";
    }

    public BmsDbContext(DbContextOptions<BmsDbContext> options, string currentUsername) : base(options)
    {
        _currentUsername = currentUsername;
    }

    // Identity Module
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();

    // CRM Module
    public DbSet<Passenger> Passengers => Set<Passenger>();

    // Booking Module
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingCost> BookingCosts => Set<BookingCost>();
    public DbSet<BookingSnapshot> BookingSnapshots => Set<BookingSnapshot>();
    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();

    // Finance Module
    public DbSet<Account> ChartOfAccounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Revenue> Revenues => Set<Revenue>();

    // Travel & Suppliers Module
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<SupplierContract> SupplierContracts => Set<SupplierContract>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Visa> Visas => Set<Visa>();

    // Master Data
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<CurrencyMaster> Currencies => Set<CurrencyMaster>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    // System Modules
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Workflow Module
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();

    // Inventory Module
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // Attachments & Outbox
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AttachmentLink> AttachmentLinks => Set<AttachmentLink>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnershipCapital> PartnershipCapitals => Set<PartnershipCapital>();
    public DbSet<SafeTransaction> SafeTransactions => Set<SafeTransaction>();
    public DbSet<GatewayPortal> GatewayPortals => Set<GatewayPortal>();
    public DbSet<ExternalVisa> ExternalVisas => Set<ExternalVisa>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Convert PascalCase properties to snake_case names for PostgreSQL compatibility
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Map table names to snake_case
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
            {
                // Map column names to snake_case
                var columnName = property.GetColumnName();
                if (columnName != null)
                {
                    property.SetColumnName(NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(columnName));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (keyName != null)
                {
                    key.SetName(NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(keyName));
                }
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                var fkName = fk.GetConstraintName();
                if (fkName != null)
                {
                    fk.SetConstraintName(NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(fkName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (indexName != null)
                {
                    index.SetDatabaseName(NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(indexName));
                }
            }
        }

        // Global query filter & xmin Concurrency configuration for BaseEntity derived types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Apply soft delete query filter (IsDeleted == false)
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));

                // Map xmin PostgreSQL system column as Optimistic Concurrency Token
                modelBuilder.Entity(entityType.ClrType)
                    .Property<uint>("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            }
        }

        // Configure UUID defaults for Primary Keys
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null && idProperty.ClrType == typeof(Guid))
            {
                idProperty.SetDefaultValueSql("gen_random_uuid()");
            }
        }

        // Restrict delete behavior on all foreign keys to prevent cascading delete loss
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Specific mappings & Precision rules
        modelBuilder.Entity<BookingCost>(entity =>
        {
            entity.Property(e => e.NetCost).HasPrecision(18, 2);
            entity.Property(e => e.BarcodeCost).HasPrecision(18, 2);
            entity.Property(e => e.CompanyMarkup).HasPrecision(18, 2);
            entity.Property(e => e.AirportCost).HasPrecision(18, 2);
            entity.Property(e => e.HotelCost).HasPrecision(18, 2);
            entity.Property(e => e.TicketCost).HasPrecision(18, 2);
            entity.Property(e => e.BusCost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BookingPayment>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BookingSnapshot>(entity =>
        {
            entity.Property(e => e.FrozenTicketPrice).HasPrecision(18, 2);
            entity.Property(e => e.FrozenSupplierDetails).HasColumnType("jsonb");
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.Property(e => e.TotalBasePrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Visa>(entity =>
        {
            entity.Property(e => e.PricePerSlot).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.Property(e => e.Rate).HasPrecision(18, 6);
        });

        modelBuilder.Entity<PartnershipCapital>(entity =>
        {
            entity.Property(e => e.AmountSar).HasPrecision(18, 2);
            entity.Property(e => e.AmountEgp).HasPrecision(18, 2);
            entity.Property(e => e.HistoricalRate).HasPrecision(18, 4);
            entity.Property(e => e.ShareRatio).HasPrecision(18, 6);
            entity.Property(e => e.ProfitShareRatio).HasPrecision(18, 6);
        });

        // JSONB configurations
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.ChangedColumns).HasColumnType("jsonb");
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Role)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<Bms.Domain.Enums.UserRole>(v))
                  .HasMaxLength(20);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Partner)
                  .WithMany(p => p.Bookings)
                  .HasForeignKey(b => b.PartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SafeTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 4);
            entity.Property(e => e.TransactionType)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<Bms.Domain.Enums.TransactionType>(v))
                  .HasMaxLength(20);
            entity.Property(e => e.Currency)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<Bms.Domain.Enums.Currency>(v))
                  .HasMaxLength(10);
            entity.HasOne(e => e.AssociatedPartner)
                  .WithMany()
                  .HasForeignKey(e => e.AssociatedPartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GatewayPortal>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ServiceType)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<Bms.Domain.Enums.PortalServiceType>(v))
                  .HasMaxLength(30);
            entity.HasOne(e => e.Partner)
                  .WithMany()
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExternalVisa>(entity =>
        {
            entity.Property(e => e.NetCost).HasPrecision(18, 2);
            entity.Property(e => e.BarcodeCost).HasPrecision(18, 2);
            entity.Property(e => e.AgentCommission).HasPrecision(18, 2);
            entity.Property(e => e.AgreementCost).HasPrecision(18, 2);
            entity.Property(e => e.TicketCost).HasPrecision(18, 2);
            entity.Property(e => e.AirportCost).HasPrecision(18, 2);
            entity.Property(e => e.BusCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            entity.Property(e => e.NetProfit).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.RemainingBalance).HasPrecision(18, 2);
        });

        // Configure RBAC joint keys
        modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure Composite & Partial Indexes
        modelBuilder.Entity<Booking>().HasIndex(b => b.BookingNumber).IsUnique();
        modelBuilder.Entity<Booking>().HasIndex(b => new { b.PackageId, b.TravelDate });

        modelBuilder.Entity<Passenger>().HasIndex(p => new { p.PassportNumber, p.Nationality }).IsUnique();

        modelBuilder.Entity<JournalEntry>().HasIndex(j => j.ReferenceNumber).IsUnique();
        modelBuilder.Entity<JournalEntryLine>().HasIndex(l => l.AccountId);

        modelBuilder.Entity<InventoryItem>().HasIndex(i => i.Sku).IsUnique();

        // ----------------- Seeding Enterprise Data -----------------
        var defaultTenantId = new Guid("e1111111-1111-1111-1111-111111111111");
        var defaultCompanyId = new Guid("c1111111-1111-1111-1111-111111111111");
        var defaultBranchId = new Guid("b1111111-1111-1111-1111-111111111111");

        // Seed Tenant
        modelBuilder.Entity<Tenant>().HasData(
            new Tenant { Id = defaultTenantId, Name = "Enterprise Tenant", Subdomain = "agency", IsActive = true, CreatedAt = DateTime.UnixEpoch }
        );

        // Seed Company
        modelBuilder.Entity<Company>().HasData(
            new Company { Id = defaultCompanyId, TenantId = defaultTenantId, Name = "Hajj & Umrah Agency Ltd", CommercialRegister = "10101010", TaxNumber = "999-999-999" }
        );

        // Seed Branch
        modelBuilder.Entity<Branch>().HasData(
            new Branch { Id = defaultBranchId, CompanyId = defaultCompanyId, Name = "Main Branch" }
        );

        // Seed Departments
        var managementDeptId = new Guid("d1111111-1111-1111-1111-111111111111");
        var operationsDeptId = new Guid("d2222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = managementDeptId, Name = "Management", Description = "Agency Executive Directors", CreatedAt = DateTime.UnixEpoch },
            new Department { Id = operationsDeptId, Name = "Operations", Description = "Umrah and Visas Desk staff", CreatedAt = DateTime.UnixEpoch }
        );

        // Seed Partners
        var partnerKhaledId = new Guid("f1111111-1111-1111-1111-111111111111");
        var partnerAlaaId = new Guid("f2222222-2222-2222-2222-222222222222");
        var partnerOmarId = new Guid("f3333333-3333-3333-3333-333333333333");
        var partnerMohamedId = new Guid("f4444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Partner>().HasData(
            new Partner { Id = partnerKhaledId, TenantId = defaultTenantId, Name = "Khaled", Username = "khaled", QuotaLimit = 100, IsActive = true },
            new Partner { Id = partnerAlaaId, TenantId = defaultTenantId, Name = "Alaa", Username = "alaa", QuotaLimit = 80, IsActive = true },
            new Partner { Id = partnerOmarId, TenantId = defaultTenantId, Name = "Omar", Username = "omar", QuotaLimit = 90, IsActive = true },
            new Partner { Id = partnerMohamedId, TenantId = defaultTenantId, Name = "Mohamed", Username = "mohamed", QuotaLimit = 50, IsActive = true }
        );

        // Password hashes for seed users
        var hasher = new Pbkdf2PasswordHasher();
        string adminPasswordHash = hasher.HashPassword("admin123");
        string partnerPasswordHash = hasher.HashPassword("partner123");

        // Seed System Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = new Guid("a1111111-1111-1111-1111-111111111111"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "admin",
                Email = "admin@bms.com",
                PasswordHash = adminPasswordHash,
                FirstName = "System",
                LastName = "Administrator",
                Role = Bms.Domain.Enums.UserRole.Admin,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a2222222-2222-2222-2222-222222222222"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "khaled",
                Email = "khaled@bms.com",
                PasswordHash = partnerPasswordHash,
                FirstName = "Khaled",
                LastName = "Shareholder",
                Role = Bms.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a3333333-3333-3333-3333-333333333333"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "alaa",
                Email = "alaa@bms.com",
                PasswordHash = partnerPasswordHash,
                FirstName = "Alaa",
                LastName = "Shareholder",
                Role = Bms.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a4444444-4444-4444-4444-444444444444"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "omar",
                Email = "omar@bms.com",
                PasswordHash = partnerPasswordHash,
                FirstName = "Omar",
                LastName = "Shareholder",
                Role = Bms.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a5555555-5555-5555-5555-555555555555"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "mohamed",
                Email = "mohamed@bms.com",
                PasswordHash = partnerPasswordHash,
                FirstName = "Mohamed",
                LastName = "Shareholder",
                Role = Bms.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            }
        );

        // Seed Partnership Capital structure
        modelBuilder.Entity<PartnershipCapital>().HasData(
            new PartnershipCapital { Id = Guid.NewGuid(), TenantId = defaultTenantId, ShareholderName = "علاء", AmountSar = 25370.0m, AmountEgp = 329810.0m, HistoricalRate = 13.0m, ShareRatio = 0.219871m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit in EGP bank and Riyals" },
            new PartnershipCapital { Id = Guid.NewGuid(), TenantId = defaultTenantId, ShareholderName = "خالد", AmountSar = 20000.0m, AmountEgp = 260000.0m, HistoricalRate = 13.0m, ShareRatio = 0.173332m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit" },
            new PartnershipCapital { Id = Guid.NewGuid(), TenantId = defaultTenantId, ShareholderName = "عمر", AmountSar = 34600.0m, AmountEgp = 450000.0m, HistoricalRate = 13.0m, ShareRatio = 0.299998m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit" },
            new PartnershipCapital { Id = Guid.NewGuid(), TenantId = defaultTenantId, ShareholderName = "وحيد", AmountSar = 23000.0m, AmountEgp = 299000.0m, HistoricalRate = 13.0m, ShareRatio = 0.199332m, ProfitShareRatio = 0.1500m, Notes = "Star Shine capital" },
            new PartnershipCapital { Id = Guid.NewGuid(), TenantId = defaultTenantId, ShareholderName = "كيلاني", AmountSar = 12400.0m, AmountEgp = 161200.0m, HistoricalRate = 13.0m, ShareRatio = 0.107465m, ProfitShareRatio = 0.2125m, Notes = "Quota shareholder" }
        );

        // Seed Currencies
        var egpId = new Guid("e1111111-1111-1111-1111-111111111111");
        var sarId = new Guid("e2222222-2222-2222-2222-222222222222");
        modelBuilder.Entity<CurrencyMaster>().HasData(
            new CurrencyMaster { Id = egpId, Code = "EGP", Name = "Egyptian Pound", Symbol = "ج.م", IsActive = true },
            new CurrencyMaster { Id = sarId, Code = "SAR", Name = "Saudi Riyal", Symbol = "ر.س", IsActive = true }
        );

        // Seed Initial Gateway visa slots packages
        modelBuilder.Entity<GatewayPortal>().HasData(
            new GatewayPortal { Id = Guid.NewGuid(), TenantId = defaultTenantId, Count = 1000, Amount = 150000.0m, ServiceType = Bms.Domain.Enums.PortalServiceType.QrCode, Notes = "Saudi Gateway initial barcodes package", TransactionDate = new DateOnly(2026, 7, 1) },
            new GatewayPortal { Id = Guid.NewGuid(), TenantId = defaultTenantId, Count = 500, Amount = 95000.0m, ServiceType = Bms.Domain.Enums.PortalServiceType.Vip, Notes = "Saudi Portal initial VIP package", TransactionDate = new DateOnly(2026, 7, 1) }
        );

        // Seed initial safe cash deposits
        modelBuilder.Entity<SafeTransaction>().HasData(
            new SafeTransaction { Id = Guid.NewGuid(), TenantId = defaultTenantId, Amount = 1500010.0m, TransactionType = Bms.Domain.Enums.TransactionType.Deposit, Currency = Bms.Domain.Enums.Currency.EGP, ExchangeRate = 1.0m, DepositorOrWithdrawerName = "Shareholders", Description = "Aggregated EGP Capital Contribution Pool", BankName = "Safe Cash box", TransactionDate = new DateOnly(2026, 7, 1) },
            new SafeTransaction { Id = Guid.NewGuid(), TenantId = defaultTenantId, Amount = 115370.0m, TransactionType = Bms.Domain.Enums.TransactionType.Deposit, Currency = Bms.Domain.Enums.Currency.SAR, ExchangeRate = 13.0m, DepositorOrWithdrawerName = "Shareholders", Description = "Aggregated Riyals Capital Pool", BankName = "Safe Cash box", TransactionDate = new DateOnly(2026, 7, 1) }
        );
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var comparison = System.Linq.Expressions.Expression.Equal(property, falseConstant);
        
        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLogEntry>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUsername;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    // Set audit columns
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUsername;

                    // Generate audit log delta if tracking
                    var auditEntry = new AuditLogEntry(entry);
                    auditEntry.TenantId = entry.Property("TenantId").CurrentValue is Guid tId ? tId : Guid.Empty;
                    auditEntry.UserId = _currentUsername;
                    auditEntries.Add(auditEntry);
                    break;

                case EntityState.Deleted:
                    // Intercept and map to soft delete instead
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = _currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUsername;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Save generated audit logs
        if (auditEntries.Any())
        {
            var logs = auditEntries.Select(e => e.ToAuditLog()).ToList();
            AuditLogs.AddRange(logs);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}

// Temporary audit helper for delta changes
internal class AuditLogEntry
{
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> ChangedColumns { get; } = new();

    public AuditLogEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        EntityName = entry.Entity.GetType().Name;
        EntityId = entry.Property("Id").CurrentValue is Guid id ? id : Guid.Empty;

        foreach (var property in entry.Properties)
        {
            string propertyName = property.Metadata.Name;
            if (propertyName == "Version" || propertyName == "xmin" || propertyName == "UpdatedAt" || propertyName == "UpdatedBy")
                continue;

            if (property.IsModified)
            {
                ChangedColumns.Add(propertyName);
                OldValues[propertyName] = property.OriginalValue;
                NewValues[propertyName] = property.CurrentValue;
            }
        }
    }

    public AuditLog ToAuditLog()
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        return new AuditLog
        {
            TenantId = TenantId,
            EntityName = EntityName,
            EntityId = EntityId,
            ChangedColumns = JsonSerializer.Serialize(ChangedColumns, options),
            OldValues = JsonSerializer.Serialize(OldValues, options),
            NewValues = JsonSerializer.Serialize(NewValues, options),
            UserId = UserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
