using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Interfaces;
using RoomScheduler.Infrastructure.Persistence;
using RoomScheduler.Infrastructure.Persistence.Repositories;
using RoomScheduler.Infrastructure.Security;

namespace RoomScheduler.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure services.
    /// 
    /// Configuration resolution order (highest wins):
    ///   1. Environment variable  — ConnectionStrings__DefaultConnection
    ///   2. appsettings.{Environment}.json
    ///   3. appsettings.json      — SQLite local fallback
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ────────────────────────────────────────────────────────────
        // The env-var name uses __ (double underscore) as the colon separator,
        // which is the ASP.NET Core convention for all platforms including Linux.
        //   e.g. ConnectionStrings__DefaultConnection=Host=db;Database=rooms;...
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=roomscheduler.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            // Log slow queries in development to catch N+1 problems early.
            options.EnableSensitiveDataLogging(
                sensitiveDataLoggingEnabled: false); // flip to true only for local debugging
        });

        // ── Repositories ────────────────────────────────────────────────────────
        services.AddScoped<IProfessorRepository, ProfessorRepository>();
        services.AddScoped<ISalaRepository,      SalaRepository>();
        services.AddScoped<IReservaRepository,   ReservaRepository>();

        // ── Security ────────────────────────────────────────────────────────────
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton<IJwtService,            JwtService>();

        return services;
    }
}
