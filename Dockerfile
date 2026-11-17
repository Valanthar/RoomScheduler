# =============================================================================
# RoomScheduler — Multi-stage Dockerfile (.NET 10)
# =============================================================================
#
# Build:   docker build -t roomscheduler .
#
# Run:     docker run -d \
#            -p 8080:8080 \
#            -e Jwt__Secret="$(openssl rand -base64 32)" \
#            -e ConnectionStrings__DefaultConnection="Data Source=/app/data/roomscheduler.db" \
#            -v roomscheduler_data:/app/data \
#            --name roomscheduler \
#            roomscheduler
#
# Environment variables the container REQUIRES at runtime:
#   Jwt__Secret                          — JWT signing key (min 32 chars)
#
# Environment variables with built-in defaults (override as needed):
#   ConnectionStrings__DefaultConnection — SQLite path or PostgreSQL DSN
#   Jwt__Issuer                          — defaults to "RoomScheduler"
#   Jwt__Audience                        — defaults to "RoomScheduler"
#   Jwt__ExpirationMinutes               — defaults to "60"
#   Cors__AllowedOrigins__0              — first allowed CORS origin
# =============================================================================


# -----------------------------------------------------------------------------
# Stage 1 — restore
# Copies only the project files so this layer is cached as long as no .csproj
# changes — restoring NuGet packages is the slowest part of the build.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /repo

COPY RoomScheduler.slnx                                                           ./
COPY src/RoomScheduler.Domain/RoomScheduler.Domain.csproj                        src/RoomScheduler.Domain/
COPY src/RoomScheduler.Domain/packages.lock.json                                 src/RoomScheduler.Domain/
COPY src/RoomScheduler.Application/RoomScheduler.Application.csproj              src/RoomScheduler.Application/
COPY src/RoomScheduler.Application/packages.lock.json                            src/RoomScheduler.Application/
COPY src/RoomScheduler.Infrastructure/RoomScheduler.Infrastructure.csproj        src/RoomScheduler.Infrastructure/
COPY src/RoomScheduler.Infrastructure/packages.lock.json                         src/RoomScheduler.Infrastructure/
COPY src/RoomScheduler.Api/RoomScheduler.Api.csproj                              src/RoomScheduler.Api/
COPY src/RoomScheduler.Api/packages.lock.json                                    src/RoomScheduler.Api/
COPY tests/RoomScheduler.Tests/RoomScheduler.Tests.csproj                        tests/RoomScheduler.Tests/
COPY tests/RoomScheduler.Tests/packages.lock.json                                tests/RoomScheduler.Tests/

RUN dotnet restore --locked-mode


# -----------------------------------------------------------------------------
# Stage 2 — build & publish
# Copies source code (which changes more often than .csproj files), builds in
# Release mode and publishes a self-contained-ready output to /app/publish.
# -----------------------------------------------------------------------------
FROM restore AS build

COPY src/   src/
COPY tests/ tests/

# Run unit tests as part of the image build — a failing test aborts the build.
RUN dotnet test tests/RoomScheduler.Tests/RoomScheduler.Tests.csproj \
    --no-restore \
    --configuration Release \
    --logger "console;verbosity=minimal"

RUN dotnet publish src/RoomScheduler.Api/RoomScheduler.Api.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish


# -----------------------------------------------------------------------------
# Stage 3 — runtime
# Uses the minimal ASP.NET runtime image (no SDK, no build tools).
# The .NET 10 ASP.NET image is based on Ubuntu 24.04 (Noble).
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a dedicated non-root user and group BEFORE switching to it.
# useradd/groupadd are the portable POSIX tools; adduser --ingroup is Debian-specific.
RUN groupadd --system appgroup \
 && useradd  --system --no-create-home --gid appgroup appuser

# Create the data directory, set ownership while still root, then lock it down.
# This must happen BEFORE "USER appuser" — once we switch user we can't chown.
RUN mkdir -p /app/data \
 && chown appuser:appgroup /app/data \
 && chmod 750 /app/data

# Copy the published output from the build stage.
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root for all subsequent instructions and at runtime.
USER appuser

# ── Runtime environment ───────────────────────────────────────────────────────
# ASPNETCORE_ENVIRONMENT tells ASP.NET Core not to serve Swagger or dev pages.
ENV ASPNETCORE_ENVIRONMENT="Production"

# ASPNETCORE_HTTP_PORTS is the .NET 8+ preferred variable; ASPNETCORE_URLS is
# the legacy fallback. Setting both ensures compatibility across patch versions.
ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_URLS="http://+:8080"

# Default connection string (SQLite). Override via -e at docker run time.
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/roomscheduler.db"

# Jwt__Secret has NO default — the container will refuse to start without it.
# This is intentional: a missing secret is a misconfiguration, not a fallback.

# Declare the volume AFTER chown so the mount point is already owned correctly.
VOLUME ["/app/data"]

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget -qO- http://localhost:8080/health 2>/dev/null || exit 1

ENTRYPOINT ["dotnet", "RoomScheduler.Api.dll"]
