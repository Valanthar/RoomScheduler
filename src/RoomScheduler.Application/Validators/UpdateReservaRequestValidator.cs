using FluentValidation;
using RoomScheduler.Application.DTOs.Reservas;

namespace RoomScheduler.Application.Validators;

public class UpdateReservaRequestValidator : AbstractValidator<UpdateReservaRequest>
{
    public UpdateReservaRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MinimumLength(3).WithMessage("Subject must have at least 3 characters.");

        RuleFor(x => x.StartsAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).WithMessage("End time must be after start time.");
    }
}
