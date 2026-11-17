# RoomScheduler API

A secure room-booking REST API built with **.NET 10**, **ASP.NET Core**, and **Domain-Driven Design (DDD)**.

Developed as a practical project during an IT technical program, demonstrating clean architecture, OOP encapsulation, JWT authentication, and containerised deployment.

---

## Features

- **JWT Authentication** — login returns a signed token; every protected endpoint validates it
- **Role-based Authorization** — `Admin` creates accounts; `Professor` manages reservations
- **Conflict-free Bookings** — the domain entity rejects overlapping room reservations at the business-logic level
- **Secure Password Storage** — PBKDF2 with random salt via ASP.NET Core `PasswordHasher`
- **RFC 7807 Error Responses** — no stack traces ever reach the client
- **Auto-migration on Startup** — EF Core applies pending migrations before the first request is served
- **Docker-ready** — multi-stage Dockerfile, non-root user, health probe included

---

## Architecture

The project follows a strict four-layer DDD structure:

```
src/
├── RoomScheduler.Domain/          # Entities, domain exceptions, repository interfaces
├── RoomScheduler.Application/     # Use cases, DTOs, FluentValidation validators
├── RoomScheduler.Infrastructure/  # EF Core, JWT service, PasswordHasher, repositories
└── RoomScheduler.Api/             # Controllers, global exception handler, Program.cs
tests/
└── RoomScheduler.Tests/           # xUnit unit tests — pure domain invariant coverage
docs/
├── DEPLOYMENT.md                  # Step-by-step GitHub + Render/Railway deployment guide
└── README.md                      # Presentation materials
```

---

## API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/v1/auth/login` | Public | Authenticate and receive a JWT |
| `POST` | `/api/v1/professores` | Admin | Register a new professor account |
| `GET` | `/api/v1/salas/grid?data=YYYY-MM-DD` | Any | Daily room availability grid |
| `POST` | `/api/v1/reservas` | Professor | Create a reservation |
| `PUT` | `/api/v1/reservas/{id}` | Author | Update your reservation |
| `DELETE` | `/api/v1/reservas/{id}` | Author | Delete your reservation |

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 / C# 14 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 10 |
| Database | SQLite (dev) / PostgreSQL (prod) |
| Auth | JWT Bearer + ASP.NET Core PasswordHasher |
| Validation | FluentValidation 12 |
| Tests | xUnit + FluentAssertions |
| Container | Docker (multi-stage, .NET 10 base images) |

---

## Running Locally

**Prerequisites:** .NET 10 SDK

```bash
# Clone
git clone https://github.com/Valanthar/RoomScheduler.git
cd RoomScheduler

# Run (the database is created automatically on first start)
cd src/RoomScheduler.Api
dotnet run
```

The API starts at `http://localhost:5175`.  
Swagger UI is available at `http://localhost:5175/swagger` in Development mode.

### Running with Docker

```bash
docker build -t roomscheduler .

docker run -d \
  -p 8080:8080 \
  -e Jwt__Secret="$(openssl rand -base64 32)" \
  -v roomscheduler_data:/app/data \
  --name roomscheduler \
  roomscheduler

curl http://localhost:8080/health   # → 200 OK
```

---

## Running the Tests

```bash
dotnet test tests/RoomScheduler.Tests
```

21 unit tests covering all critical domain invariants:
- No past-date reservations (Business Rule 6)
- No overlapping bookings, adjacent slots allowed (Business Rule 5)
- Ownership enforcement on update and delete (Business Rule 7)
- Professor entity validation and email normalisation

---

## Environment Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `Jwt__Secret` | **Yes** | — | JWT signing key (min 32 chars) |
| `Jwt__Issuer` | No | `RoomScheduler` | JWT issuer claim |
| `Jwt__Audience` | No | `RoomScheduler` | JWT audience claim |
| `Jwt__ExpirationMinutes` | No | `60` | Token lifetime in minutes |
| `ConnectionStrings__DefaultConnection` | No | SQLite local file | Database connection string |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | Set to `Development` to enable Swagger |

Generate a secure secret:
```bash
# Linux / macOS
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

---

## Deployment

See [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) for a full step-by-step guide covering GitHub push, Render/Railway setup, and environment variable configuration.

---

## Project Structure — Key Files

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build — restore → test → publish → runtime |
| `.dockerignore` | Keeps `bin/`, `obj/`, secrets out of the build context |
| `requests.http` | Sample HTTP requests for all endpoints (VS Code REST Client) |
| `docs/DEPLOYMENT.md` | Deployment guide |
| `src/RoomScheduler.Domain/Entities/Reserva.cs` | Core domain entity with overlap logic |
| `src/RoomScheduler.Api/Program.cs` | Application entry point, DI wiring, migration runner |

---

## Author

**Carlos Eduardo** — Technical Recruiter transitioning to Software Development  
GitHub: [@Valanthar](https://github.com/Valanthar)  
Email: carloseduardolinhares12@gmail.com
