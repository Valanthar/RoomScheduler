using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoomScheduler.Api.Middleware;
using RoomScheduler.Application;
using RoomScheduler.Infrastructure;
using RoomScheduler.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Application & Infrastructure services ─────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Wire up FluentValidation's ASP.NET Core auto-validation here (Api layer)
// because FluentValidation.AspNetCore is an Api dependency, not Application.
builder.Services.AddFluentValidationAutoValidation();

// ── 2. Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── 3. JWT Authentication ─────────────────────────────────────────────────────
//
// Secret resolution order (highest priority wins):
//   1. Environment variable:  Jwt__Secret=<value>          ← production / Docker
//   2. appsettings.Development.json  Jwt.Secret            ← local dev only
//   3. (appsettings.json has NO secret — intentional)
//
// If the secret is absent in the current environment the app refuses to start,
// rather than running with an empty signing key.
var jwtSecret = builder.Configuration["Jwt:Secret"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    // Fail fast with an actionable message so the operator knows exactly what to set.
    throw new InvalidOperationException(
        "JWT signing secret is not configured. " +
        "Set the environment variable 'Jwt__Secret' to a random string of at least 32 characters. " +
        "Example (Linux/Mac):  export Jwt__Secret=$(openssl rand -base64 32)  " +
        "Example (Windows):    $env:Jwt__Secret = [Convert]::ToBase64String((1..32 | % { [byte](Get-Random -Max 256) }))");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? "RoomScheduler",
            ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "RoomScheduler",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew                = TimeSpan.Zero   // tokens expire exactly on time — no grace period
        };
    });

builder.Services.AddAuthorization();

// ── 4. Global Exception Handler (RFC 7807) ────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── 5. CORS ───────────────────────────────────────────────────────────────────
//
// Allowed origins resolution order:
//   1. Environment variable:  Cors__AllowedOrigins__0=https://myapp.com
//   2. appsettings.json       Cors.AllowedOrigins array
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── 6. HSTS ───────────────────────────────────────────────────────────────────
builder.Services.AddHsts(options =>
{
    options.MaxAge            = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload           = true;
});

// ── 7. Swagger / OpenAPI (dev only) ──────────────────────────────────────────
// Note: Swashbuckle 11.x uses Microsoft.OpenApi v3 where all types live
// directly in the Microsoft.OpenApi namespace (no .Models sub-namespace).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "RoomScheduler API",
        Version     = "v1",
        Description = "Secure room-booking API — DDD + .NET 10"
    });
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
//
// MigrateDatabase runs BEFORE the HTTP pipeline so the DB is ready before the
// first request is accepted. It applies any pending migrations or creates the
// database from scratch if it doesn't exist yet.
//
// Production note: for blue/green or rolling deployments where running
// migrations at startup would be unsafe, remove this call and run migrations
// out-of-band:  dotnet ef database update --connection "<prod-conn-string>"
await MigrateDatabase(app);

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();   // RFC 7807 — must be first so it catches pipeline errors too

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RoomScheduler API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

// Security response headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"]        = "DENY";
    ctx.Response.Headers["Referrer-Policy"]        = "no-referrer";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Lightweight health probe used by the Docker HEALTHCHECK and load-balancers.
// Returns 200 OK with no body — intentionally reveals nothing about internals.
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

app.Run();

// ── Startup helper ────────────────────────────────────────────────────────────

static async Task MigrateDatabase(WebApplication app)
{
    // Use a scoped lifetime so the DbContext is disposed after migration completes.
    await using var scope = app.Services.CreateAsyncScope();

    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider
                      .GetRequiredService<ILogger<AppDbContext>>();

    try
    {
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

        if (pending.Count == 0)
        {
            logger.LogInformation("Database is up-to-date. No pending migrations.");
        }
        else
        {
            logger.LogInformation(
                "Applying {Count} pending migration(s): {Migrations}",
                pending.Count,
                string.Join(", ", pending));

            await db.Database.MigrateAsync();

            logger.LogInformation("Database migration completed successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Database migration failed. The application cannot start. " +
            "Check the connection string and database server availability.");

        // Re-throw so the process exits with a non-zero code, which Docker /
        // Kubernetes will treat as a failed startup and restart or alert.
        throw;
    }
}
