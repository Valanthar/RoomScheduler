# RoomScheduler — Deployment Guide

This guide covers everything needed to push the project to GitHub and deploy it
to a cloud platform using the Dockerfile. Follow the steps in order.

---

## Prerequisites

| Tool | Minimum version | Install |
|---|---|---|
| Git | 2.x | https://git-scm.com |
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download |
| Docker Desktop | 24.x | https://www.docker.com/products/docker-desktop (optional — only needed for local container testing) |
| GitHub account | — | https://github.com |
| Render **or** Railway account | — | https://render.com **or** https://railway.app |

---

## Part 1 — Push to GitHub

### 1.1 Create the repository on GitHub

1. Log in to GitHub and click **New repository** (the `+` icon → *New repository*).
2. Set the repository name to **RoomScheduler** (or any name you prefer).
3. Choose **Private** or **Public** depending on your submission requirements.
4. Leave *"Initialize this repository with a README"* **unchecked** — the project already has one.
5. Click **Create repository**.
6. Copy the HTTPS URL shown on the next screen, e.g.:
   ```
   https://github.com/YOUR_USERNAME/RoomScheduler.git
   ```

### 1.2 Initialise Git locally and push

Open a terminal in the **project root** (the folder containing `RoomScheduler.slnx`):

```bash
# 1. Initialise a local Git repository (skip if one already exists)
git init

# 2. Stage everything — .gitignore already excludes bin/, obj/, *.db, secrets
git add .

# 3. Review what will be committed (optional sanity check)
git status

# 4. Create the first commit
git commit -m "feat: initial RoomScheduler DDD implementation"

# 5. Rename the default branch to 'main' (matches GitHub's default)
git branch -M main

# 6. Point the local repo at your GitHub remote
git remote add origin https://github.com/YOUR_USERNAME/RoomScheduler.git

# 7. Push
git push -u origin main
```

> **What gets committed:**
> Every file not matched by `.gitignore`. This includes `src/`, `tests/`, `docs/`,
> `Dockerfile`, `.dockerignore`, `requests.http`, all `packages.lock.json` files,
> and all four `*.md` spec files.
>
> **What stays local** (never committed):
> `bin/`, `obj/`, `*.db`, `*.db-shm`, `*.db-wal`, `appsettings.Production.json`,
> and any `*.env` file.

### 1.3 Verify the `/docs` folder is visible on GitHub

After the push, open your repository in a browser and confirm you can see:

```
RoomScheduler/
└── docs/
    ├── README.md
    └── DEPLOYMENT.md   ← this file
```

Add any presentation slides, architecture diagrams, or PDFs inside `docs/` and
commit them with:

```bash
git add docs/
git commit -m "docs: add presentation materials"
git push
```

---

## Part 2 — Deploy with Docker on Render or Railway

Both platforms can read your Dockerfile directly from the GitHub repository.
Choose whichever platform you prefer — instructions for both are below.

---

### Option A — Render

Render's free tier supports a single web service with 512 MB RAM, which is
enough to run RoomScheduler with SQLite.

#### A.1 Create a new Web Service

1. Go to https://render.com and sign in.
2. Click **New +** → **Web Service**.
3. Under *"Connect a repository"*, click **Connect account** and authorise GitHub.
4. Select the **RoomScheduler** repository and click **Connect**.

#### A.2 Configure the build

Render detects the Dockerfile automatically. Confirm these settings:

| Setting | Value |
|---|---|
| **Name** | `roomscheduler` (or any name) |
| **Region** | Closest to your users |
| **Branch** | `main` |
| **Runtime** | **Docker** |
| **Dockerfile Path** | `./Dockerfile` |
| **Instance Type** | Free (or Starter for persistent disk) |

> **Important — persistent storage on Render's free tier:**
> Free instances use ephemeral storage, meaning the SQLite database is wiped on
> every redeploy. For a permanent database either upgrade to a paid instance with
> a Render Disk, or switch to a Render PostgreSQL add-on (see Part 3).

#### A.3 Set environment variables

Scroll down to the **Environment** section and add the following key/value pairs
(see Part 3 for how to generate the values):

| Key | Value |
|---|---|
| `Jwt__Secret` | *(your generated secret — see Part 3)* |
| `Jwt__Issuer` | `RoomScheduler` |
| `Jwt__Audience` | `RoomScheduler` |
| `Jwt__ExpirationMinutes` | `60` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/roomscheduler.db` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Cors__AllowedOrigins__0` | `https://YOUR_RENDER_URL.onrender.com` |

#### A.4 Deploy

Click **Create Web Service**. Render will:

1. Clone your repository.
2. Run `docker build` using your Dockerfile (this runs your unit tests too).
3. Start the container.
4. Apply EF Core migrations automatically on first boot (`MigrateDatabase` in `Program.cs`).

The service URL will be:
```
https://roomscheduler.onrender.com
```
(the exact subdomain depends on the name you chose).

---

### Option B — Railway

Railway's free trial gives $5 of credit, enough for several days of a small service.

#### B.1 Create a new project

1. Go to https://railway.app and sign in with GitHub.
2. Click **New Project** → **Deploy from GitHub repo**.
3. Select the **RoomScheduler** repository.
4. Railway detects the Dockerfile. Click **Deploy Now**.

#### B.2 Expose the port

Railway needs to know which port your container listens on:

1. Click on the deployed service tile.
2. Go to **Settings** → **Networking** → **Generate Domain**.
3. Set **Custom Start Command** to blank (Railway reads `EXPOSE 8080` from the Dockerfile).
4. Under **Variables**, add `PORT=8080` if Railway shows a port mismatch warning.

#### B.3 Set environment variables

Click **Variables** in the service panel and add:

| Key | Value |
|---|---|
| `Jwt__Secret` | *(your generated secret — see Part 3)* |
| `Jwt__Issuer` | `RoomScheduler` |
| `Jwt__Audience` | `RoomScheduler` |
| `Jwt__ExpirationMinutes` | `60` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/roomscheduler.db` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Cors__AllowedOrigins__0` | `https://YOUR_RAILWAY_URL.railway.app` |

Railway automatically redeploys the service after every variable change.

---

## Part 3 — Environment Variables Reference

### Why no secrets are in the codebase

`appsettings.json` deliberately contains **no** `Jwt.Secret` value. The app
refuses to start if `Jwt__Secret` is missing, logging:

```
JWT signing secret is not configured. Set the environment variable 'Jwt__Secret'...
```

This guarantees you can never accidentally deploy with an empty or hardcoded key.

### Generating a strong JWT secret

Pick whichever command matches your local environment:

```bash
# Linux / macOS
openssl rand -base64 32
```

```powershell
# Windows PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

Both produce a 44-character base64 string such as:
```
K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols=
```

Paste this value as-is into the `Jwt__Secret` field in the cloud dashboard.
**Never put it in any file you commit to Git.**

### Connection string options

| Scenario | Value |
|---|---|
| SQLite in container (default) | `Data Source=/app/data/roomscheduler.db` |
| SQLite at a custom path | `Data Source=/mnt/volume/roomscheduler.db` |
| PostgreSQL (Render add-on) | `Host=<host>;Database=roomscheduler;Username=<user>;Password=<pass>` |
| PostgreSQL (Railway add-on) | Same format — Railway provides the full DSN under *Variables* as `DATABASE_URL` |

> **Switching to PostgreSQL:**
> Change `options.UseSqlite(...)` to `options.UseNpgsql(...)` in
> `src/RoomScheduler.Infrastructure/DependencyInjection.cs`.
> The EF Core migrations will run automatically on startup against the new database.
> The `Npgsql.EntityFrameworkCore.PostgreSQL` package is already installed.

### Complete variable reference

| Variable | Required | Default in image | Purpose |
|---|---|---|---|
| `Jwt__Secret` | **Yes** | *(none — startup fails without it)* | JWT signing key |
| `Jwt__Issuer` | No | `RoomScheduler` | JWT `iss` claim |
| `Jwt__Audience` | No | `RoomScheduler` | JWT `aud` claim |
| `Jwt__ExpirationMinutes` | No | `60` | Token lifetime |
| `ConnectionStrings__DefaultConnection` | No | `Data Source=/app/data/roomscheduler.db` | Database location |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | Disables Swagger in prod |
| `ASPNETCORE_HTTP_PORTS` | No | `8080` | Port the Kestrel server listens on |
| `Cors__AllowedOrigins__0` | No | `http://localhost:3000` | First allowed CORS origin |

---

## Part 4 — Verifying the Deployment

Once the service is live, run these checks:

### 4.1 Health probe

```bash
curl https://YOUR_DEPLOYED_URL/health
# Expected: 200 OK
```

### 4.2 Login (should return a JWT)

```bash
curl -X POST https://YOUR_DEPLOYED_URL/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@roomscheduler.com","password":"Admin@123456"}'
# Expected: 200 OK with {"token":"eyJ...","expiresAt":"..."}
```

> The admin account must be seeded first. If you have not added a database seeder,
> register the admin via a direct DB insert or add a seeder to `Program.cs` that
> runs in `Development` only.

### 4.3 Confirm Swagger is off in production

```bash
curl https://YOUR_DEPLOYED_URL/swagger
# Expected: 404 Not Found  (Swagger is disabled when ASPNETCORE_ENVIRONMENT=Production)
```

---

## Part 5 — Submission Link Format

The submission expects two links: the GitHub repository and the live API URL.

### GitHub repository

```
https://github.com/YOUR_USERNAME/RoomScheduler
```

The grader will check that:
- The repository is **public** (or you have shared access with the grader).
- The `/docs` folder exists and contains the presentation materials.
- The `Dockerfile` is present at the repository root.
- The source compiles and tests pass (`dotnet test`).

### Live API (deployed URL)

| Platform | URL format |
|---|---|
| Render | `https://roomscheduler.onrender.com` |
| Railway | `https://roomscheduler-production.up.railway.app` |
| Fly.io | `https://roomscheduler.fly.dev` |

The grader will likely hit the `/health` endpoint and then `POST /api/v1/auth/login`
to confirm the API is reachable and responding correctly.

### Final submission format

```
GitHub:  https://github.com/YOUR_USERNAME/RoomScheduler
API:     https://YOUR_DEPLOYED_URL
```

Replace both placeholders with your actual URLs before submitting.

---

## Quick-Reference Cheatsheet

```bash
# ── Local run ──────────────────────────────────────────────────────────────────
cd src/RoomScheduler.Api
dotnet run
# API: http://localhost:5175
# Swagger: http://localhost:5175/swagger

# ── Local Docker test ──────────────────────────────────────────────────────────
# From the project root:
docker build -t roomscheduler .

docker run -d \
  -p 8080:8080 \
  -e Jwt__Secret="$(openssl rand -base64 32)" \
  -v roomscheduler_data:/app/data \
  --name roomscheduler \
  roomscheduler

curl http://localhost:8080/health        # → 200 OK
docker logs roomscheduler               # → see migration output

# ── Push an update ─────────────────────────────────────────────────────────────
git add .
git commit -m "feat: your change description"
git push
# Render / Railway redeploy automatically on every push to main.

# ── Run tests ──────────────────────────────────────────────────────────────────
dotnet test tests/RoomScheduler.Tests
```
