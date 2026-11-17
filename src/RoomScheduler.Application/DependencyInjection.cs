using FluentValidation;
using FluentValidation.AspNetCore;
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

        // ── FluentValidation — scans this assembly for all AbstractValidator<T> ─
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<DependencyInjection>();

        return services;
    }
}
