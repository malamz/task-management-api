![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/ORM-EF%20Core-512BD4?style=flat-square)
![JWT](https://img.shields.io/badge/Auth-JWT-black?style=flat-square&logo=json-web-tokens)
![OpenAPI](https://img.shields.io/badge/OpenAPI-Swagger-6BA53F?style=flat-square&logo=openapi-initiative)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

# Task Management API

A production-ready REST API built with **.NET**, demonstrating JWT authentication, role-based authorization, repository pattern, CRUD operations, paginated queries, global error handling, rate limiting, and Swagger documentation.

---

## Features

- **JWT-based authentication** – stateless token flow with configurable expiry
- **Role-based authorization** – `Admin` and `User` roles; delete is admin-only
- **RESTful CRUD** – full task lifecycle with partial updates
- **Pagination & filtering** – query tasks by status, priority, or assigned user
- **Repository pattern** – clean separation between business logic and data access
- **Entity Framework Core** – Code-First with migrations; supports both **PostgreSQL** and **SQL Server**
- **Rate limiting** – per-endpoint fixed-window policies (ASP.NET Core built-in)
- **Global error handling** – every exception maps to a consistent JSON envelope
- **Serilog logging** – structured logs to console and rolling daily files
- **Swagger / OpenAPI** – interactive docs ship with the API out of the box

---

## Tech Stack

| Layer            | Technology                                              |
| ---------------- | ------------------------------------------------------- |
| Runtime          | .NET 8                                                  |
| Framework        | ASP.NET Core Web API                                    |
| ORM              | Entity Framework Core 8                                 |
| Database         | SQL Server _or_ PostgreSQL (toggle in config)           |
| Auth             | JWT via `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Password hashing | BCrypt.Net                                              |
| Logging          | Serilog (Console + File sinks)                          |
| Docs             | Swashbuckle / Swagger UI                                |
| Rate limiting    | `Microsoft.AspNetCore.RateLimiting`                     |

---

## Project Structure

```
TaskManagementAPI/
├── Controllers/
│   ├── AuthController.cs          # POST /api/auth/register & /login
│   └── TasksController.cs         # Full CRUD /api/tasks
├── Services/
│   ├── AuthService.cs             # Registration & login logic
│   ├── TaskService.cs             # Task business logic + DTO mapping
│   └── TokenService.cs            # JWT generation
├── Data/
│   ├── AppDbContext.cs            # EF Core context + Fluent API config
│   └── DbSeeder.cs                # Seeds a default admin user
├── Repositories/
│   ├── Repository.cs              # Generic repo base class
│   ├── UserRepository.cs          # FindByEmail
│   └── TaskRepository.cs          # Paginated, filtered queries
├── Interfaces/
│   ├── IRepository.cs             # Generic repo interface
│   ├── IUserRepository.cs         # User repo interface
│   ├── ITaskRepository.cs         # Task repo interface
│   ├── IAuthService.cs            # Auth service interface
│   ├── ITaskService.cs            # Task service interface
│   └── ITokenService.cs           # Token service interface
├── Models/
│   ├── User.cs                    # User Entity
│   ├── Task.cs                    # Task Entity
│   └── Enums.cs                   # Enums
├── Contracts/
│   ├── AuthDtos.cs                # Auth Request / Response objects (DTOs)
│   ├── TaskDtos.cs                # Task Request / Response objects (DTOs)
│   └── SharedDtos.cs              # Common DTOs
├── Middleware/
│   └── GlobalExceptionHandler.cs  # Centralized error handling logic
├── Migrations/                    # EF Core database migration history
├── Configuration/
│   └── AppSettings.cs             # JwtSettings, DatabaseSettings
├── Properties/
│   └── launchSettings.json
├── appsettings.json               # Production config template
├── appsettings.Development.json   # Dev overrides
├── Program.cs                     # DI, middleware pipeline, startup
└── TaskManagementAPI.csproj       # Project file + package refs
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or remote) **or** PostgreSQL

### 1. Clone & Navigate

```bash
# Clone the repository
git clone https://github.com/malamz/task-management-api.git

# Navigate to the project directory
cd task-management-api/src/TaskManagementAPI
```

### 2. Configure the database

Open **`appsettings.json`** (or `appsettings.Development.json` for local work):

```json
"Database": {
  "UsePostgres": true,  // true → Npgsql
  "ConnectionString": "Server=localhost;Database=TaskManagementDB;Trusted_Connection=True;"
}
```

> **PostgreSQL example connection string:**
> `"Host=localhost;Port=5432;Database=TaskManagementDB;Username=postgres;Password=yourpassword;"`

### 3. Set a JWT secret

In the same config file, replace the placeholder key with something strong (≥ 32 characters):

```json
"Jwt": {
  "SecretKey": "YourSuperSecretKeyHere_MustBeAtLeast32CharactersLong!"
}
```

### 4. Run migrations & start

```bash
# Apply migrations manually (auto-runs on startup too, but running manually is safer)
dotnet ef database update
```

> Note: Make sure EF Core Tools are installed on your machine.  
> `dotnet tool install --global dotnet-ef`

```bash
# Start the API
dotnet run
```

### 5. Open Swagger

```
http://localhost:5032/swagger
```

> Note: Your port might differ based on launchSettings.json; check the terminal output for the exact URL.

The seeder creates a default **admin** account on first launch:

| Field    | Value               |
| -------- | ------------------- |
| Email    | `admin@example.com` |
| Password | `Admin123!`         |

---

## API Endpoints

### Authentication

| Method | Endpoint             | Auth | Description                  |
| ------ | -------------------- | ---- | ---------------------------- |
| POST   | `/api/auth/register` | ❌   | Create account → returns JWT |
| POST   | `/api/auth/login`    | ❌   | Authenticate → returns JWT   |

### Tasks

| Method | Endpoint          | Auth | Role      | Description                        |
| ------ | ----------------- | ---- | --------- | ---------------------------------- |
| GET    | `/api/tasks`      | ✅   | Any       | Paginated list (see filters below) |
| GET    | `/api/tasks/{id}` | ✅   | Any       | Single task                        |
| POST   | `/api/tasks`      | ✅   | Any       | Create task                        |
| PUT    | `/api/tasks/{id}` | ✅   | Any       | Partial update                     |
| DELETE | `/api/tasks/{id}` | ✅   | **Admin** | Delete task                        |

#### GET /api/tasks query parameters

| Parameter        | Default | Description                      |
| ---------------- | ------- | -------------------------------- |
| `page`           | 1       | Page number                      |
| `pageSize`       | 10      | Items per page (max 50)          |
| `status`         | –       | `Todo` \| `InProgress` \| `Done` |
| `priority`       | –       | `Low` \| `Medium` \| `High`      |
| `assignedUserId` | –       | Filter by user GUID              |

---

## Architecture Decisions

| Decision                        | Rationale                                                                                                |
| ------------------------------- | -------------------------------------------------------------------------------------------------------- |
| **JWT (stateless)**             | No server-side session store needed; scales horizontally; standard for API-first architectures           |
| **Repository pattern**          | Decouples controllers/services from EF Core; makes unit testing straightforward                          |
| **Code-First migrations**       | Schema lives in version control; team-friendly; works identically across SQL Server and PostgreSQL       |
| **BCrypt for passwords**        | Adaptive cost factor protects against brute-force even if the DB is compromised                          |
| **Global exception middleware** | Single place to map exceptions → HTTP codes; controllers stay focused on happy-path logic                |
| **Fixed-window rate limiter**   | Built into ASP.NET Core 8; no extra dependency; protects auth endpoints from credential-stuffing         |
| **DTO layer**                   | Prevents accidental exposure of navigation properties / internal fields; makes the API contract explicit |

---

## What I Would Add in a Production Deployment

- **Secrets Management** – connection strings, JWT keys, and API credentials moved to Azure Key Vault / AWS Secrets Manager. Environment Variables for local development to keep appsettings.json clean.
- **Refresh tokens** – short-lived access tokens + long-lived refresh tokens stored securely
- **Email verification** – SMTP integration with token-based confirmation links
- **Audit logging** – who changed what, when (append-only audit table)
- **Redis caching** – cache hot queries; distributed cache for multi-instance deployments
- **Health-check endpoints** – `/health` and `/ready` for load balancer heartbeats and container orchestration probes
- **API versioning** – `/api/v1/…` route prefix
- **Integration tests** – `Microsoft.AspNetCore.Mvc.Testing` with an in-memory database
- **CI/CD** – GitHub Actions pipeline: lint → test → build → deploy to cloud

---

## License

This project is MIT licensed – use it as a portfolio piece or starting point for your own work.
