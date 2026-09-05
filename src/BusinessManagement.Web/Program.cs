using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using BusinessManagement.Domain.Repositories;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Application.Services;
using BusinessManagement.Persistence;
using BusinessManagement.Shared.Security;
using BusinessManagement.Web.Authorization;
using Serilog;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Register IHttpContextAccessor first
builder.Services.AddHttpContextAccessor();

// 1. Configure Serilog Logging Sink with sensitive data masking
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Destructure.ByTransforming<BusinessManagement.Domain.Entities.User>(u => new { u.Username, u.Email, PasswordHash = "[REDACTED]", TwoFactorSecret = "[REDACTED]" })
    .Destructure.ByTransforming<BusinessManagement.Domain.Entities.Passenger>(p => new { p.FullName, PassportNumber = "[REDACTED]", NationalId = "[REDACTED]" })
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configure PostgreSQL Database Context with Outbox Interceptor
builder.Services.AddScoped<BusinessManagement.Persistence.Interceptors.OutboxInterceptor>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BusinessManagementDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<BusinessManagement.Persistence.Interceptors.OutboxInterceptor>())
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// Register Cloud Sync Background Worker
builder.Services.AddHostedService<BusinessManagement.Web.Services.CloudSyncWorker>();

// Configure Database-Backed Data Protection Keys (Point 17/v2 & Point 4/v3)
builder.Services.AddSingleton<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository, BusinessManagement.Web.Middlewares.CustomDbXmlRepository>();
builder.Services.AddDataProtection()
    .SetApplicationName("BusinessManagementERP");

builder.Services.AddOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>()
    .Configure<Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository>((options, repository) =>
    {
        options.XmlRepository = repository;
    });

// Configure Rate Limiting Policies (Point 19/v2)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 10,
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5; // Max 5 logins per minute
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
});

// 3. Register Core Security & Cryptography Services
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<BusinessManagement.Domain.ICurrentUserProvider, BusinessManagement.Web.Services.CurrentUserProvider>();

// 4. Register Repository and Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 5. Register Core Application Business Logic Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISafeService, SafeService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IPartnershipService, PartnershipService>();
builder.Services.AddScoped<IExternalVisaService, ExternalVisaService>();
builder.Services.AddScoped<ITravelDashboardService, TravelDashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<BusinessManagement.Persistence.Services.AuditIntegrityVerifier>();

// Dynamic Authorization Registration
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();

// 6. Configure Authentication Cookies (OWASP Top 10 Security Rules)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "BusinessManagement.SessionCookie";
        options.Cookie.IsEssential = true;
    });

// 7. Configure Policy-based Authorization (RBAC)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
});

// 8. Register Localization (Arabic-EG default & English-US fallback support)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// 9. Configure Health Checks (Database connectivity check)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BusinessManagementDbContext>("Database");

var app = builder.Build();

// 10. Initialize Database (Run local and cloud migrations and seed lookups)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<BusinessManagementDbContext>();
        
        Log.Information("Initializing local database tables and running migrations...");
        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE \"users\" SET \"email\" = REPLACE(\"email\", '@BusinessManagement.com', '@egypttravelportal.com') WHERE \"email\" ILIKE '%@BusinessManagement.com';");
        Log.Information("Local database migrations completed successfully!");
        
        // Migrate cloud DB if connection is present
        var config = services.GetRequiredService<IConfiguration>();
        var cloudConn = config.GetConnectionString("CloudConnection");
        if (!string.IsNullOrEmpty(cloudConn))
        {
            Log.Information("Initializing cloud database tables and running migrations...");
            var optionsBuilder = new DbContextOptionsBuilder<BusinessManagementDbContext>();
            optionsBuilder.UseNpgsql(cloudConn);
            using var cloudDb = new BusinessManagementDbContext(optionsBuilder.Options);
            await cloudDb.Database.MigrateAsync();
            await cloudDb.Database.ExecuteSqlRawAsync(
                "UPDATE \"users\" SET \"email\" = REPLACE(\"email\", '@BusinessManagement.com', '@egypttravelportal.com') WHERE \"email\" ILIKE '%@BusinessManagement.com';");
            Log.Information("Cloud database migrations completed successfully!");
        }
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Fatal error occurred during database auto-initialization.");
    }
}

// 11. Configure HTTP Request Pipeline & Security Middlewares
app.UseMiddleware<BusinessManagement.Web.Middlewares.ExceptionHandlingMiddleware>();

// Custom CorrelationId and OpenTelemetry TraceId logger context (Point 9/v2, 9/v3, and 5/v4)
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    if (!context.Response.Headers.ContainsKey("X-Correlation-ID"))
    {
        context.Response.Headers.Append("X-Correlation-ID", correlationId);
    }
    
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    using (Serilog.Context.LogContext.PushProperty("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier))
    {
        await next();
    }
});

// Security headers middleware
app.UseMiddleware<BusinessManagement.Web.Middlewares.SecurityHeadersMiddleware>();

// Enable ASP.NET Core Rate Limiting (Point 19/v2)
app.UseRateLimiter();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

// Map static files using optimized MapStaticAssets
app.MapStaticAssets();

// 12. Request Localization Middleware Setup
var supportedCultures = new[] { "ar-EG", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ar-EG")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

// Authentication MUST be verified before Authorization
app.UseAuthentication();
app.UseAuthorization();

// 13. Map Health Checks & Default Routes
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

