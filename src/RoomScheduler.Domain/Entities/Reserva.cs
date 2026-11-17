using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Domain.Entities;

/// <summary>
/// Aggregate root representing a room reservation.
///
/// Invariants enforced (all via constructor / business methods):
///   - Start time must be in the future              (Business Rule 6).
///   - End time must be after start time.
///   - No time overlap with existing reservations    (Business Rule 5).
///   - Only the owning professor can mutate the reservation (Business Rule 7).
///
/// The overlap-check method is PURE (no I/O, no DI) so it is fully unit-testable.
/// </summary>
public sealed class Reserva : Entity<Guid>
{
    public Guid      SalaId       { get; private set; }
    public Guid      ProfessorId  { get; private set; }
    public string    Subject      { get; private set; } = default!;
    public DateTime  StartsAt     { get; private set; }
    public DateTime  EndsAt       { get; private set; }
    public DateTime  CreatedAt    { get; private init; }

    // Navigation properties — populated by EF Core, never set directly.
    public Sala?      Sala      { get; private set; }
    public Professor? Professor { get; private set; }

    // ── EF Core ─────────────────────────────────────────────────────────────────
    private Reserva() { }

    // ── Factory ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a validated reservation.
    /// <paramref name="existingReservas"/> must contain ALL reservations for the
    /// target room on the target day — provided by the Application layer after
    /// querying the repository.
    /// </summary>
    public static Reserva Create(
        Guid                       salaId,
        Guid                       professorId,
        string                     subject,
        DateTime                   startsAt,
        DateTime                   endsAt,
        IEnumerable<Reserva>       existingReservas)
    {
        ValidateSubject(subject);
        ValidateTimes(startsAt, endsAt);
        CheckOverlap(salaId, startsAt, endsAt, existingReservas, excludedId: null);

        return new Reserva
        {
            Id          = Guid.NewGuid(),
            SalaId      = salaId,
            ProfessorId = professorId,
            Subject     = subject.Trim(),
            StartsAt    = startsAt,
            EndsAt      = endsAt,
            CreatedAt   = DateTime.UtcNow
        };
    }

    // ── Business Methods ────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the reservation's subject and/or time window.
    /// Enforces ownership (Business Rule 7) and re-validates all invariants.
    /// <paramref name="existingReservas"/> must exclude this reservation itself.
    /// </summary>
    public void Update(
        Guid                 requestingProfessorId,
        string               newSubject,
        DateTime             newStartsAt,
        DateTime             newEndsAt,
        IEnumerable<Reserva> existingReservas)
    {
        EnforceOwnership(requestingProfessorId);
        ValidateSubject(newSubject);
        ValidateTimes(newStartsAt, newEndsAt);
        CheckOverlap(SalaId, newStartsAt, newEndsAt, existingReservas, excludedId: Id);

        Subject  = newSubject.Trim();
        StartsAt = newStartsAt;
        EndsAt   = newEndsAt;
    }

    /// <summary>
    /// Validates the requesting professor owns this reservation before deletion.
    /// The actual delete is performed by the repository; this method only guards.
    /// </summary>
    public void ValidateCanDelete(Guid requestingProfessorId)
        => EnforceOwnership(requestingProfessorId);

    // ── Private domain logic ────────────────────────────────────────────────────

    /// <summary>
    /// Pure overlap check — no I/O, fully testable.
    /// Two intervals [A, B) and [C, D) overlap when A &lt; D AND C &lt; B.
    /// </summary>
    private static void CheckOverlap(
        Guid                 salaId,
        DateTime             startsAt,
        DateTime             endsAt,
        IEnumerable<Reserva> existingReservas,
        Guid?                excludedId)
    {
        var conflict = existingReservas.FirstOrDefault(r =>
            r.SalaId   == salaId          &&
            r.Id       != (excludedId ?? Guid.Empty) &&
            r.StartsAt <  endsAt          &&
            startsAt   <  r.EndsAt);

        if (conflict is not null)
            throw new ConflictException(
                $"The room already has a reservation from {conflict.StartsAt:HH:mm} " +
                $"to {conflict.EndsAt:HH:mm} on {conflict.StartsAt:yyyy-MM-dd}.");
    }

    private void EnforceOwnership(Guid requestingProfessorId)
    {
        if (ProfessorId != requestingProfessorId)
            throw new ForbiddenException(
                "Only the professor who created this reservation can modify or delete it.");
    }

    private static void ValidateSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Reservation subject cannot be empty.");
        if (subject.Trim().Length < 3)
            throw new DomainException("Reservation subject must have at least 3 characters.");
    }

    private static void ValidateTimes(DateTime startsAt, DateTime endsAt)
    {
        if (startsAt <= DateTime.UtcNow)
            throw new DomainException("Reservation start time must be in the future.");
        if (endsAt <= startsAt)
            throw new DomainException("Reservation end time must be after start time.");
        if ((endsAt - startsAt).TotalMinutes < 15)
            throw new DomainException("Reservation must be at least 15 minutes long.");
    }
}
