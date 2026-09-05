# Project Folder Structure Reference

Detailed workspace visual directory tree:

```text
d:/omarWeb/
├── docs/                                  # Project Documentation files
├── src/
│   ├── BusinessManagement.Domain/        # Domain Models & Core Exceptions
│   │   ├── Entities/                      # Database entities (Booking, Passenger)
│   │   └── Repositories/                  # Interface Repository & UnitOfWork contracts
│   ├── BusinessManagement.Application/   # Service logic, DTOs, and Interfaces
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   ├── Interfaces/                    # Service Interfaces
│   │   └── Services/                      # Service Implementations (BookingService)
│   ├── BusinessManagement.Persistence/   # Database configuration & seeding
│   │   ├── Context/                       # BusinessManagementDbContext code
│   │   ├── Migrations/                    # EF Core Migrations
│   │   └── Repositories/                  # Implementation of Repository & UoW
│   ├── BusinessManagement.Infrastructure/ # Mail adapters & external service clients
│   ├── BusinessManagement.Shared/        # Password Hashers, TOTP & Security helpers
│   │   └── Security/                      # AES-GCM and ClamAV stream scanner socket
│   └── BusinessManagement.Web/           # MVC Controllers & Razor Views
│       ├── Controllers/                   # Controllers (DatabaseManagementController)
│       └── Views/                         # Razor Views (.cshtml)
```
