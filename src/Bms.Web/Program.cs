using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Bms.Domain.Repositories;
using Bms.Application.Interfaces;
using Bms.Application.Services;
using Bms.Infrastructure.Persistence;
using Bms.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure PostgreSQL Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BmsDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Register Core Security & Cryptography Services
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();

// 3. Register Repository and Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. Register Core Application Business Logic Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISafeService, SafeService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<IPartnershipService, PartnershipService>();
builder.Services.AddScoped<IExternalVisaService, ExternalVisaService>();
builder.Services.AddScoped<ITravelDashboardService, TravelDashboardService>();

// 5. Configure Authentication Cookies (OWASP Top 10 Security Rules)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        
        // Strict cookie security properties
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allows HTTP locally but enforces HTTPS in production
        options.Cookie.Name = "BMS.SessionCookie";
    });

// 6. Register MVC Controllers & Razor Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 7. Initialize Database (Run offline migrations and seed lookups)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<BmsDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogWarning("Dropping existing database schemas to apply travel operations schema...");
        await dbContext.Database.EnsureDeletedAsync();
        
        logger.LogInformation("Initializing database tables and seed lookups...");
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Fatal error occurred during database auto-initialization.");
    }
}

// 8. Configure HTTP Request Pipeline & Security Middlewares
app.UseMiddleware<Bms.Web.Middlewares.ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Map static files using optimized MapStaticAssets
app.MapStaticAssets();

app.UseRouting();

// Authentication MUST be verified before Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Configure Default Route Mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
