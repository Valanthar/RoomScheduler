using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Application.Services;

namespace RoomScheduler.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Use Case Services ────────────────────────────────────────────────────
        services.AddScoped<IAuthService,      AuthService>();
        services.AddScoped<IProfessorService, ProfessorService>();
        services.AddScoped<IReservaService,   ReservaService>();
        services.AddScoped<ISalaService,      SalaService>();

        // ── FluentValidation — registers all AbstractValidator<T> in this assembly.
        // AddFluentValidationAutoValidation() is intentionally NOT called here because
        // it lives in FluentValidation.AspNetCore, which is an Api-layer dependency.
        // It is registered in Program.cs instead.
        services.AddValidatorsFromAssemblyContaining<AuthService>();

        return services;
    }
}
