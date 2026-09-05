# System Developer Setup & Installation Guide

Complete beginner-friendly walkthrough to run the ERP locally.

## Step 1: Install Dependencies
- Install **.NET 10 SDK** from Microsoft's download center.
- Install **PostgreSQL v15+** server locally.
- (Optional) Install **ClamAV** daemon if testing security filters.

## Step 2: Configure Database
1. Connect to your PostgreSQL instance using pgAdmin or terminal.
2. Update the database connection string details in [src/BusinessManagement.Web/appsettings.json](file:///d:/omarWeb/src/BusinessManagement.Web/appsettings.json):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=travel_db;Username=postgres;Password=yourpassword"
   }
   ```

## Step 3: Run Database Migrations
Run the EF Core database updater tool:
```bash
dotnet ef database update --project src/BusinessManagement.Persistence --startup-project src/BusinessManagement.Web
```

## Step 4: Run the Web App
Run the project using standard CLI:
```bash
dotnet run --project src/BusinessManagement.Web
```
The application will launch on your localhost port `5000`/`5001`.
