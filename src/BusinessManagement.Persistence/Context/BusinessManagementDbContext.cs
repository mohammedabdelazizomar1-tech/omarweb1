using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.NameTranslation;


namespace BusinessManagement.Persistence;

public class BusinessManagementDbContext : DbContext
{
    private readonly ICurrentUserProvider? _currentUserProvider;

    public BusinessManagementDbContext(
        DbContextOptions<BusinessManagementDbContext> options,
        ICurrentUserProvider? currentUserProvider = null) : base(options)
    {
        _currentUserProvider = currentUserProvider;
    }

    private string CurrentUsername => _currentUserProvider?.GetCurrentUsername() ?? "system";

    private Guid CurrentTenantId => _currentUserProvider?.GetCurrentTenantId() ?? Guid.Empty;

    // Identity Module
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<UserMfaRecoveryCode> UserMfaRecoveryCodes => Set<UserMfaRecoveryCode>();

    // CRM Module
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<PassengerNote> PassengerNotes => Set<PassengerNote>();

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
    public DbSet<MonthlyClosing> MonthlyClosings => Set<MonthlyClosing>();

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
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
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
                // Apply soft delete and multi-tenant query filters dynamically
                var method = typeof(BusinessManagementDbContext)
                    .GetMethod(nameof(ConfigureTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);
                
                method?.Invoke(this, new object[] { modelBuilder });

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
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Payload).HasColumnType("jsonb");
        });

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.ToTable("processed_events");
            entity.HasKey(e => e.EventId);
            entity.HasIndex(e => e.EventId).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.Property(e => e.Payload).HasColumnType("jsonb");
            entity.Property(e => e.SequenceId).UseIdentityByDefaultColumn();
            entity.HasIndex(e => new { e.Status, e.NextRetryAt });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Role)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<BusinessManagement.Domain.Enums.UserRole>(v))
                  .HasMaxLength(20);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Partner)
                  .WithMany(p => p.Bookings)
                  .HasForeignKey(b => b.PartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasOne(e => e.Partner)
                  .WithMany()
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SafeTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 4);
            entity.Property(e => e.TransactionType)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<BusinessManagement.Domain.Enums.TransactionType>(v))
                  .HasMaxLength(20);
            entity.Property(e => e.Currency)
                  .HasConversion(v => v.ToString(), v => Enum.Parse<BusinessManagement.Domain.Enums.Currency>(v))
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
                  .HasConversion(v => v.ToString(), v => Enum.Parse<BusinessManagement.Domain.Enums.PortalServiceType>(v))
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

        var encryptionConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
            v => BusinessManagement.Shared.Security.EncryptionHelper.Encrypt(v),
            v => BusinessManagement.Shared.Security.EncryptionHelper.Decrypt(v)
        );

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.Property(p => p.PassportNumber).HasConversion(encryptionConverter);
            entity.Property(p => p.NationalId).HasConversion(encryptionConverter);
            entity.HasIndex(p => new { p.PassportNumberHash, p.Nationality }).IsUnique();
        });

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
        var partnerWaheedId = new Guid("f5555555-5555-5555-5555-555555555555");
        var partnerKelanyId = new Guid("f6666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<Partner>().HasData(
            new Partner { Id = partnerKhaledId, TenantId = defaultTenantId, Name = "Khaled", Username = "khaled", QuotaLimit = 100, IsActive = true },
            new Partner { Id = partnerAlaaId, TenantId = defaultTenantId, Name = "Alaa", Username = "alaa", QuotaLimit = 80, IsActive = true },
            new Partner { Id = partnerOmarId, TenantId = defaultTenantId, Name = "Omar", Username = "omar", QuotaLimit = 90, IsActive = true },
            new Partner { Id = partnerMohamedId, TenantId = defaultTenantId, Name = "Mohamed", Username = "mohamed", QuotaLimit = 50, IsActive = true },
            new Partner { Id = partnerWaheedId, TenantId = defaultTenantId, Name = "Waheed", Username = "waheed", QuotaLimit = 60, IsActive = true },
            new Partner { Id = partnerKelanyId, TenantId = defaultTenantId, Name = "Kelany", Username = "kelany", QuotaLimit = 60, IsActive = true }
        );

        // Static pre-computed password hashes (PBKDF2 / SHA-256 / 100000 iterations)
        // These are FIXED at seed time - never change without re-running HashGen utility
        const string adminPasswordHash   = "100000.YWRtaW5zYWx0MTIzNDU2Nw==.n4oZd6/RB84eYUOC5tNRItWB9SC1qWB4ikTZWrkn8dk=";
        const string partnerPasswordHash = "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=";

        // Seed System Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = new Guid("a1111111-1111-1111-1111-111111111111"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "admin",
                Email = "admin@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = adminPasswordHash,
                FirstName = "System",
                LastName = "Administrator",
                Role = BusinessManagement.Domain.Enums.UserRole.Admin,
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
                Email = "khaled@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Khaled",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
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
                Email = "alaa@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Alaa",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
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
                Email = "omar@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Omar",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
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
                Email = "mohamed@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Mohamed",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a6666666-6666-6666-6666-666666666666"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "waheed",
                Email = "waheed@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Waheed",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            },
            new User
            {
                Id = new Guid("a7777777-7777-7777-7777-777777777777"),
                TenantId = defaultTenantId,
                BranchId = defaultBranchId,
                Username = "kelany",
                Email = "kelany@egypttravelportal.com",
                EmailConfirmed = true,
                PasswordHash = partnerPasswordHash,
                FirstName = "Kelany",
                LastName = "Shareholder",
                Role = BusinessManagement.Domain.Enums.UserRole.Partner,
                DepartmentId = managementDeptId,
                IsActive = true,
                CreatedAt = DateTime.UnixEpoch,
                UpdatedAt = DateTime.UnixEpoch
            }
        );

        // Seed Partnership Capital structure
        var partnerCapitalId1 = new Guid("31111111-1111-1111-1111-111111111111");
        var partnerCapitalId2 = new Guid("32222222-2222-2222-2222-222222222222");
        var partnerCapitalId3 = new Guid("33333333-3333-3333-3333-333333333333");
        var partnerCapitalId4 = new Guid("34444444-4444-4444-4444-444444444444");
        var partnerCapitalId5 = new Guid("35555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<PartnershipCapital>().HasData(
            new PartnershipCapital { Id = partnerCapitalId1, TenantId = defaultTenantId, ShareholderName = "علاء", AmountSar = 25370.0m, AmountEgp = 329810.0m, HistoricalRate = 13.0m, ShareRatio = 0.219871m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit in EGP bank and Riyals" },
            new PartnershipCapital { Id = partnerCapitalId2, TenantId = defaultTenantId, ShareholderName = "خالد", AmountSar = 20000.0m, AmountEgp = 260000.0m, HistoricalRate = 13.0m, ShareRatio = 0.173332m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit" },
            new PartnershipCapital { Id = partnerCapitalId3, TenantId = defaultTenantId, ShareholderName = "عمر", AmountSar = 34600.0m, AmountEgp = 450000.0m, HistoricalRate = 13.0m, ShareRatio = 0.299998m, ProfitShareRatio = 0.2125m, Notes = "Capital deposit" },
            new PartnershipCapital { Id = partnerCapitalId4, TenantId = defaultTenantId, ShareholderName = "وحيد", AmountSar = 23000.0m, AmountEgp = 299000.0m, HistoricalRate = 13.0m, ShareRatio = 0.199332m, ProfitShareRatio = 0.1500m, Notes = "Star Shine capital" },
            new PartnershipCapital { Id = partnerCapitalId5, TenantId = defaultTenantId, ShareholderName = "كيلاني", AmountSar = 12400.0m, AmountEgp = 161200.0m, HistoricalRate = 13.0m, ShareRatio = 0.107465m, ProfitShareRatio = 0.2125m, Notes = "Quota shareholder" }
        );

        // Seed Currencies
        var egpId = new Guid("e1111111-1111-1111-1111-111111111111");
        var sarId = new Guid("e2222222-2222-2222-2222-222222222222");
        // Seed Chart of Accounts
        var accCashEgpId = new Guid("c1111111-1111-1111-1111-111111111111");
        var accCashSarId = new Guid("c2222222-2222-2222-2222-222222222222");
        var accReceivableId = new Guid("c3333333-3333-3333-3333-333333333333");
        var accPayableId = new Guid("c4444444-4444-4444-4444-444444444444");
        var accCapitalId = new Guid("c5555555-5555-5555-5555-555555555555");
        var accRetainedId = new Guid("c6666666-6666-6666-6666-666666666666");
        var accSalesId = new Guid("c7777777-7777-7777-7777-777777777777");
        var accCostVisaId = new Guid("c8888888-8888-8888-8888-888888888888");
        var accCostFlightId = new Guid("c9999999-9999-9999-9999-999999999999");
        var accCostHotelId = new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var accCostBusId = new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var accCostBarcodeId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var accExpenseGenId = new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd");

        modelBuilder.Entity<Account>().HasData(
            new Account { Id = accCashEgpId, TenantId = defaultTenantId, AccountCode = "1101", Name = "صندوق النقدية (ج.م)", Type = "Asset", IsActive = true },
            new Account { Id = accCashSarId, TenantId = defaultTenantId, AccountCode = "1102", Name = "صندوق النقدية (ر.س)", Type = "Asset", IsActive = true },
            new Account { Id = accReceivableId, TenantId = defaultTenantId, AccountCode = "1201", Name = "حسابات ذمم العملاء المدينة", Type = "Asset", IsActive = true },
            new Account { Id = accPayableId, TenantId = defaultTenantId, AccountCode = "2101", Name = "حسابات دائنة للموردين والشركاء", Type = "Liability", IsActive = true },
            new Account { Id = accCapitalId, TenantId = defaultTenantId, AccountCode = "3101", Name = "رأس مال الشركاء", Type = "Equity", IsActive = true },
            new Account { Id = accRetainedId, TenantId = defaultTenantId, AccountCode = "3201", Name = "الأرباح المحتجزة / المجمعة", Type = "Equity", IsActive = true },
            new Account { Id = accSalesId, TenantId = defaultTenantId, AccountCode = "4101", Name = "إيرادات مبيعات الحجوزات", Type = "Revenue", IsActive = true },
            new Account { Id = accCostVisaId, TenantId = defaultTenantId, AccountCode = "5101", Name = "مصروفات تكلفة الفيزا والمنصة", Type = "Expense", IsActive = true },
            new Account { Id = accCostFlightId, TenantId = defaultTenantId, AccountCode = "5102", Name = "مصروفات تذاكر الطيران", Type = "Expense", IsActive = true },
            new Account { Id = accCostHotelId, TenantId = defaultTenantId, AccountCode = "5103", Name = "مصروفات فنادق وتسكين البرامج", Type = "Expense", IsActive = true },
            new Account { Id = accCostBusId, TenantId = defaultTenantId, AccountCode = "5104", Name = "مصروفات انتقالات وباصات", Type = "Expense", IsActive = true },
            new Account { Id = accCostBarcodeId, TenantId = defaultTenantId, AccountCode = "5105", Name = "مصروفات باركود وبوابات", Type = "Expense", IsActive = true },
            new Account { Id = accExpenseGenId, TenantId = defaultTenantId, AccountCode = "5201", Name = "مصروفات إدارية وعامة", Type = "Expense", IsActive = true }
        );

        modelBuilder.Entity<CurrencyMaster>().HasData(
            new CurrencyMaster { Id = egpId, Code = "EGP", Name = "Egyptian Pound", Symbol = "ج.م", IsActive = true },
            new CurrencyMaster { Id = sarId, Code = "SAR", Name = "Saudi Riyal", Symbol = "ر.س", IsActive = true }
        );

        // Seed Initial Gateway visa slots packages
        var gatewayPortalId1 = new Guid("41111111-1111-1111-1111-111111111111");
        var gatewayPortalId2 = new Guid("42222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<GatewayPortal>().HasData(
            new GatewayPortal { Id = gatewayPortalId1, TenantId = defaultTenantId, Count = 1000, Amount = 150000.0m, ServiceType = BusinessManagement.Domain.Enums.PortalServiceType.QrCode, Notes = "Saudi Gateway initial barcodes package", TransactionDate = new DateOnly(2026, 7, 1) },
            new GatewayPortal { Id = gatewayPortalId2, TenantId = defaultTenantId, Count = 500, Amount = 95000.0m, ServiceType = BusinessManagement.Domain.Enums.PortalServiceType.Vip, Notes = "Saudi Portal initial VIP package", TransactionDate = new DateOnly(2026, 7, 1) }
        );

        // Seed initial safe cash deposits
        var safeTransactionId1 = new Guid("51111111-1111-1111-1111-111111111111");
        var safeTransactionId2 = new Guid("52222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<SafeTransaction>().HasData(
            new SafeTransaction { Id = safeTransactionId1, TenantId = defaultTenantId, Amount = 1500010.0m, TransactionType = BusinessManagement.Domain.Enums.TransactionType.Deposit, Currency = BusinessManagement.Domain.Enums.Currency.EGP, ExchangeRate = 1.0m, DepositorOrWithdrawerName = "Shareholders", Description = "Aggregated EGP Capital Contribution Pool", BankName = "Safe Cash box", TransactionDate = new DateOnly(2026, 7, 1) },
            new SafeTransaction { Id = safeTransactionId2, TenantId = defaultTenantId, Amount = 115370.0m, TransactionType = BusinessManagement.Domain.Enums.TransactionType.Deposit, Currency = BusinessManagement.Domain.Enums.Currency.SAR, ExchangeRate = 13.0m, DepositorOrWithdrawerName = "Shareholders", Description = "Aggregated Riyals Capital Pool", BankName = "Safe Cash box", TransactionDate = new DateOnly(2026, 7, 1) }
        );

        // Seed default Package for bookings without an explicit package
        var defaultPackageId = new Guid("99999999-9999-9999-9999-999999999999");
        modelBuilder.Entity<Package>().HasData(
            new Package { Id = defaultPackageId, TenantId = defaultTenantId, Name = "Default Package", NightsCount = 0, TotalBasePrice = 0.0m }
        );

        // Seed default Roles
        var adminRoleId = new Guid("f1111111-1111-1111-1111-111111111111");
        var managerRoleId = new Guid("f2222222-2222-2222-2222-222222222222");
        var employeeRoleId = new Guid("f3333333-3333-3333-3333-333333333333");
        var partnerRoleId = new Guid("f4444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin" },
            new Role { Id = managerRoleId, Name = "Manager" },
            new Role { Id = employeeRoleId, Name = "Employee" },
            new Role { Id = partnerRoleId, Name = "Partner" }
        );

        // Seed default Permissions
        var permBookingsViewId = new Guid("e1111111-1111-1111-1111-111111111111");
        var permBookingsCreateId = new Guid("e2222222-2222-2222-2222-222222222222");
        var permBookingsEditId = new Guid("e3333333-3333-3333-3333-333333333333");
        var permBookingsDeleteId = new Guid("e4444444-4444-4444-4444-444444444444");
        var permSafeViewId = new Guid("e5555555-5555-5555-5555-555555555555");
        var permRolesManageId = new Guid("e6666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = permBookingsViewId, Code = "bookings.view", Description = "عرض دفتر الحجوزات" },
            new Permission { Id = permBookingsCreateId, Code = "bookings.create", Description = "تسجيل حجز جديد" },
            new Permission { Id = permBookingsEditId, Code = "bookings.edit", Description = "تعديل تفاصيل الحجز" },
            new Permission { Id = permBookingsDeleteId, Code = "bookings.delete", Description = "حذف وأرشفة الحجوزات" },
            new Permission { Id = permSafeViewId, Code = "safe.view", Description = "عرض وإدارة الخزينة والشركاء" },
            new Permission { Id = permRolesManageId, Code = "roles.manage", Description = "إدارة الأدوار وصلاحيات النظام" }
        );

        // Seed UserRoles (map admin user to Admin role)
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = new Guid("a1111111-1111-1111-1111-111111111111"), RoleId = adminRoleId }
        );

        // Seed RolePermissions for Admin role (Admin gets all permissions)
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = adminRoleId, PermissionId = permBookingsViewId },
            new RolePermission { RoleId = adminRoleId, PermissionId = permBookingsCreateId },
            new RolePermission { RoleId = adminRoleId, PermissionId = permBookingsEditId },
            new RolePermission { RoleId = adminRoleId, PermissionId = permBookingsDeleteId },
            new RolePermission { RoleId = adminRoleId, PermissionId = permSafeViewId },
            new RolePermission { RoleId = adminRoleId, PermissionId = permRolesManageId }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
        {
            warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
        });
    }

    private void ConfigureTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        var hasTenantId = typeof(TEntity).GetProperty("TenantId") != null;
        if (hasTenantId)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted && EF.Property<Guid>(e, "TenantId") == CurrentTenantId);
        }
        else
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Enforce Audit Logs Immutability (Point 8/v3 & Point 6/v4)
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is ActivityLog)
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    throw new InvalidOperationException("Modification or deletion of security audit logs is strictly prohibited.");
                }
            }
        }

        var auditEntries = new List<AuditLogEntry>();
        var currentUsername = CurrentUsername;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // 2. Automate Passenger Searchable Hashes (Point 1/v2 & Point 1/v3)
            if (entry.Entity is Passenger passenger)
            {
                if (entry.State == EntityState.Added || entry.Property(nameof(Passenger.PassportNumber)).IsModified)
                {
                    passenger.PassportNumberHash = BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField(passenger.PassportNumber);
                }
                if (entry.State == EntityState.Added || entry.Property(nameof(Passenger.NationalId)).IsModified)
                {
                    passenger.NationalIdHash = BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField(passenger.NationalId);
                }
            }

            switch (entry.State)
            {
                case EntityState.Added:
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUsername;
                    entry.Entity.IsDeleted = false;

                    var tenantIdProp = entry.Metadata.FindProperty("TenantId");
                    if (tenantIdProp != null && tenantIdProp.ClrType == typeof(Guid))
                    {
                        var currentValue = entry.Property("TenantId").CurrentValue is Guid tId ? tId : Guid.Empty;
                        if (currentValue == Guid.Empty)
                        {
                            var tenantId = CurrentTenantId;
                            if (tenantId == Guid.Empty)
                            {
                                tenantId = new Guid("e1111111-1111-1111-1111-111111111111");
                            }
                            entry.Property("TenantId").CurrentValue = tenantId;
                        }
                    }
                    break;
                }

                case EntityState.Modified:
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUsername;

                    // Generate audit log delta if tracking
                    var auditEntry = new AuditLogEntry(entry);
                    var tenantIdProp = entry.Metadata.FindProperty("TenantId");
                    auditEntry.TenantId = tenantIdProp != null && entry.Property("TenantId").CurrentValue is Guid tId ? tId : Guid.Empty;
                    auditEntry.UserId = currentUsername;
                    auditEntries.Add(auditEntry);
                    break;
                }

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = currentUsername;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUsername;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Save generated audit logs
        if (auditEntries.Any())
        {
            var logs = auditEntries.Select(e => e.ToAuditLog()).ToList();
            
            // Retrieve previous hash from the database using ordering
            string? previousHash = await AuditLogs
                .IgnoreQueryFilters()
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => a.LogHash)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(previousHash))
            {
                previousHash = "0000000000000000000000000000000000000000000000000000000000000000"; // Genesis hash
            }

            string auditKey = Environment.GetEnvironmentVariable("APP_AUDIT_LEDGER_KEY") ?? "SecureDefaultAuditLedgerKey123!@#";
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(auditKey);

            foreach (var log in logs)
            {
                log.PreviousLogHash = previousHash;
                
                // Payload data to sign: previousHash || timestamp || userId || entityName || entityId || newValues
                string payload = $"{previousHash}|{log.CreatedAt:o}|{log.UserId}|{log.EntityName}|{log.EntityId}|{log.NewValues}";
                
                using (var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                    log.LogHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
                }

                previousHash = log.LogHash; // chain onto the next log entry in this batch!
            }

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





