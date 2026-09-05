# Developer Installation Guide

This guide describes the steps required to set up a local development environment for the Business Management ERP.

## Prerequisites
- **SDK**: .NET 10 SDK (v10.0.x or newer)
- **Database Engine**: PostgreSQL (v15.0 or newer)
- **Editor**: Visual Studio 2022 (v17.10+) or Visual Studio Code

## Step-by-Step Setup

### 1. Database Configuration
Ensure PostgreSQL is running and you have a superuser role. Open your PostgreSQL terminal and run:
```sql
CREATE DATABASE travel_system_db;
```

### 2. Configure Local Settings
Update [src/BusinessManagement.Web/appsettings.json](file:///d:/omarWeb/src/BusinessManagement.Web/appsettings.json) with your connection credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=travel_system_db;Username=postgres;Password=yourpassword"
}
```

### 3. Restore NuGet Packages
Restore project dependencies from the root directory:
```bash
dotnet restore BusinessManagementSystem.slnx
```

### 4. Run EF Core Migrations
Apply the database migrations to generate tables, indexes, and triggers:
```bash
dotnet ef database update --project src/BusinessManagement.Persistence --startup-project src/BusinessManagement.Web
```

### 5. Start the Application
Run the web application locally:
```bash
dotnet run --project src/BusinessManagement.Web
```
The application will launch on `http://localhost:5000` (or the configured ports in launchSettings.json).
