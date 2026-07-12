# 🐄 HerdSmart — Smart Farm Management System

A multi-tenant farm management API built with **Clean Architecture** and **.NET 10**, designed to help farm owners manage cattle, health records, milk production, vaccinations, and IoT alerts in real time.

---

## ✨ Features

- 🔐 **JWT Authentication** with Refresh Token (Redis)
- 🏢 **Multi-tenancy** — full data isolation per farm
- 🐄 **Cattle Management** — full CRUD with soft delete
- 💉 **Vaccination Scheduling** — with overdue tracking
- 🏥 **Health Logs** — diagnosis and treatment records
- 🥛 **Milk Production** — shift-based logging and reporting
- 📡 **IoT Alerts** — real-time sensor alerts via SignalR
- 👥 **Role-based Access** — Owner, Vet, Worker
- 📊 **Dashboard** — farm summary and top producers

---

## 🏗️ Architecture

```
HerdSmart/
├── HerdSmart.Domain/         → Entities, Enums
├── HerdSmart.Application/    → CQRS, Handlers, Interfaces
├── HerdSmart.Infrastructure/ → EF Core, JWT, Redis, Services
├── HerdSmart.Api/            → Controllers, Middlewares, SignalR
└── HerdSmart.Tests/          → Unit Tests
```

### Layers

| Layer | Responsibility |
|-------|---------------|
| Domain | Entities, Enums, Business Rules |
| Application | CQRS Commands/Queries, Validation, Mapping |
| Infrastructure | Database, JWT, Redis, External Services |
| API | Controllers, Middlewares, SignalR Hub |

---

## 🛠️ Tech Stack

| Technology | Purpose |
|------------|---------|
| .NET 10 | Framework |
| ASP.NET Core | Web API |
| Entity Framework Core | ORM |
| SQL Server | Database |
| Redis | Refresh Token Cache |
| MediatR | CQRS Pattern |
| FluentValidation | Input Validation |
| AutoMapper | Object Mapping |
| Serilog | Structured Logging |
| SignalR | Real-time IoT Alerts |
| Hangfire | Vaccination Scheduling |
| xUnit + Moq | Unit Testing |
| ULID | Sortable Unique IDs |

---

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server
- Redis

### Installation

```bash
# Update appsettings.json
# Set your ConnectionStrings, JWT settings, and Redis connection
```

### Configuration

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=HerdSmart;Trusted_Connection=True;TrustServerCertificate=True",
    "RedisConnection": "localhost:6379"
  },
  "JWT": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "HerdSmart",
    "Audience": "HerdSmart-Client",
    "ExpiryInMinutes": 60
  }
}
```

### Run Migrations

```bash
dotnet ef database update --project HerdSmart.Infrastructure --startup-project HerdSmart.Api
```

### Run the API

```bash
dotnet run --project HerdSmart.Api
```

API will be available at `https://localhost:7062`

---

## 📡 API Endpoints

### 🔐 Auth
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new farm | Public |
| POST | `/api/auth/login` | Login | Public |
| POST | `/api/auth/refresh-token` | Refresh access token | Public |

### 👥 Users
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users` | Get all users | Owner |
| GET | `/api/users/{id}` | Get user by ID | Owner |
| POST | `/api/users` | Create Vet or Worker | Owner |
| PUT | `/api/users/{id}` | Update user | Owner |
| DELETE | `/api/users/{id}` | Delete user | Owner |

### 🐄 Cattle
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/cattle` | Get all cattle | All |
| GET | `/api/cattle/{id}` | Get cattle details | All |
| POST | `/api/cattle` | Add cattle | Owner/Vet |
| PUT | `/api/cattle/{id}` | Update cattle | Owner/Vet |
| DELETE | `/api/cattle/{id}` | Soft delete cattle | Owner |

### 🥛 Milk Production
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/milk` | Get all logs | All |
| GET | `/api/milk/summary` | Production summary | All |
| GET | `/api/milk/top-producers` | Top producing cattle | All |
| POST | `/api/milk` | Log milk session | Worker/Vet |

### 💉 Vaccinations
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/vaccinations` | Get all schedules | All |
| GET | `/api/vaccinations/pending` | Pending vaccinations | All |
| GET | `/api/vaccinations/overdue` | Overdue vaccinations | All |
| POST | `/api/vaccinations` | Schedule vaccination | Vet |
| PUT | `/api/vaccinations/{id}/complete` | Mark as completed | Vet |

### 🏥 Health Logs
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/health-logs` | Get all logs | All |
| POST | `/api/health-logs` | Add health log | Vet |
| PUT | `/api/health-logs/{id}` | Update log | Vet |

### 📡 IoT Alerts
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/alerts` | Get all alerts | All |
| GET | `/api/alerts/active` | Active alerts | All |
| POST | `/api/alerts` | Send alert (IoT sensor) | System |
| PUT | `/api/alerts/{id}/resolve` | Resolve alert | Owner/Vet |

---

## 👥 Roles

| Role | Permissions |
|------|------------|
| **Owner** | Full access + manage users |
| **Vet** | Health logs, vaccinations, cattle |
| **Worker** | Milk production, read-only cattle |

---

## 🧪 Running Tests

```bash
dotnet test HerdSmart.Tests
```

---

## 📁 Project Structure

```
Application/
├── Common/
│   ├── Behaviors/      → Logging, Validation, Performance
│   ├── Exceptions/     → Custom exceptions
│   ├── Extensions/     → Pagination helper
│   ├── Interfaces/     → IApplicationDbContext, IJwtService...
│   └── Models/         → PaginatedResult
└── Features/
    ├── Auth/           → Register, Login, RefreshToken
    ├── Users/          → CRUD
    ├── Cattle/         → CRUD
    ├── Milk/           → Production logs
    ├── Vaccinations/   → Schedule management
    ├── HealthLogs/     → Medical records
    └── Alerts/         → IoT telemetry
```

---

## 🔄 Git Flow

```
main
└── develop
    ├── feature/domain
    ├── feature/persistence
    ├── feature/auth
    ├── feature/cattle
    └── ...
```

---

## 👨‍💻 Author

Built with ❤️ as a portfolio project demonstrating Clean Architecture, CQRS, Multi-tenancy, and IoT integration in .NET 10.
