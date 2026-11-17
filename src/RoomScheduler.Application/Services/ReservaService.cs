using RoomScheduler.Application.DTOs.Reservas;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Exceptions;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Application.Services;

public sealed class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepo;
    private readonly ISalaRepository    _salaRepo;

    public ReservaService(IReservaRepository reservaRepo, ISalaRepository salaRepo)
    {
        _reservaRepo = reservaRepo;
        _salaRepo    = salaRepo;
    }

    public async Task<ReservaResponse> CreateAsync(
        Guid professorId, CreateReservaRequest request, CancellationToken ct = default)
    {
        var sala = await _salaRepo.GetByIdAsync(request.SalaId, ct)
            ?? throw new NotFoundException(nameof(Sala), request.SalaId);

        if (!sala.IsActive)
            throw new DomainException("The selected room is not available.");

        var date     = DateOnly.FromDateTime(request.StartsAt.Date);
        var existing = await _reservaRepo.GetByRoomAndDateAsync(request.SalaId, date, ct);

        // Domain entity performs the overlap check (Business Rule 5)
        var reserva = Reserva.Create(
            request.SalaId, professorId, request.Subject,
            request.StartsAt, request.EndsAt, existing);

        await _reservaRepo.AddAsync(reserva, ct);
        await _reservaRepo.SaveChangesAsync(ct);

        return MapToResponse(reserva, sala.Name, professorId.ToString());
    }

    public async Task<ReservaResponse> UpdateAsync(
        Guid professorId, Guid reservaId, UpdateReservaRequest request, CancellationToken ct = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct)
            ?? throw new NotFoundException(nameof(Reserva), reservaId);

        var date     = DateOnly.FromDateTime(request.StartsAt.Date);
        var existing = await _reservaRepo.GetByRoomAndDateAsync(reserva.SalaId, date, ct);

        // Domain method enforces ownership + overlap (Business Rules 5 & 7)
        reserva.Update(professorId, request.Subject, request.StartsAt, request.EndsAt, existing);

        await _reservaRepo.SaveChangesAsync(ct);

        var sala = await _salaRepo.GetByIdAsync(reserva.SalaId, ct);
        return MapToResponse(reserva, sala?.Name ?? "Unknown", professorId.ToString());
    }

    public async Task DeleteAsync(Guid professorId, Guid reservaId, CancellationToken ct = default)
    {
        var reserva = await _reservaRepo.GetByIdAsync(reservaId, ct)
            ?? throw new NotFoundException(nameof(Reserva), reservaId);

        // Domain method enforces ownership (Business Rule 7)
        reserva.ValidateCanDelete(professorId);

        _reservaRepo.Remove(reserva);
        await _reservaRepo.SaveChangesAsync(ct);
    }

    private static ReservaResponse MapToResponse(Reserva r, string salaName, string professorName)
        => new(r.Id, r.SalaId, salaName, r.ProfessorId, professorName,
               r.Subject, r.StartsAt, r.EndsAt, r.CreatedAt);
}
