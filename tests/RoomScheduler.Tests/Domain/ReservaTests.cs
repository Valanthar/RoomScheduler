using FluentAssertions;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Tests.Domain;

/// <summary>
/// Unit tests for the Reserva domain entity.
/// All tests are pure — no I/O, no EF Core, no DI.
/// Each test maps directly to a Business Rule from BUSINESS_RULES.md.
/// </summary>
public class ReservaTests
{
    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static DateTime Future(int minutesFromNow = 60)
        => DateTime.UtcNow.AddMinutes(minutesFromNow);

    private static Reserva BuildReserva(
        Guid?    salaId      = null,
        Guid?    professorId = null,
        DateTime? startsAt   = null,
        DateTime? endsAt     = null,
        IEnumerable<Reserva>? existing = null)
    {
        return Reserva.Create(
            salaId      ?? Guid.NewGuid(),
            professorId ?? Guid.NewGuid(),
            "Test Subject",
            startsAt    ?? Future(60),
            endsAt      ?? Future(120),
            existing    ?? []);
    }

    // ── Business Rule 6: No past-date reservations ───────────────────────────

    [Fact]
    public void Create_ShouldThrow_WhenStartsAtIsInThePast()
    {
        // Arrange
        var pastStart = DateTime.UtcNow.AddMinutes(-10);
        var end       = DateTime.UtcNow.AddMinutes(60);

        // Act
        var act = () => BuildReserva(startsAt: pastStart, endsAt: end);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("*future*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenStartsAtIsNow()
    {
        // The boundary is strictly in the future.
        var act = () => BuildReserva(
            startsAt: DateTime.UtcNow.AddSeconds(-1),
            endsAt:   DateTime.UtcNow.AddHours(1));

        act.Should().Throw<DomainException>();
    }

    // ── End time validation ──────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldThrow_WhenEndsAtIsBeforeStartsAt()
    {
        var act = () => BuildReserva(
            startsAt: Future(60),
            endsAt:   Future(30));  // ends before starts

        act.Should().Throw<DomainException>()
           .WithMessage("*after start*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenDurationIsLessThan15Minutes()
    {
        var act = () => BuildReserva(
            startsAt: Future(60),
            endsAt:   Future(60 + 10)); // only 10 min

        act.Should().Throw<DomainException>()
           .WithMessage("*15 minutes*");
    }

    // ── Business Rule 5: No overlapping reservations ─────────────────────────

    [Fact]
    public void Create_ShouldThrow_WhenNewReservationOverlapsExisting()
    {
        // Arrange
        var salaId = Guid.NewGuid();
        var profId = Guid.NewGuid();

        // Existing: 10:00 → 11:00
        var existing = new List<Reserva>
        {
            BuildReserva(salaId: salaId, startsAt: Future(60), endsAt: Future(120))
        };

        // New: 10:30 → 11:30 — overlaps existing by 30 min
        var act = () => Reserva.Create(
            salaId, profId, "Overlap Subject",
            startsAt: Future(90),
            endsAt:   Future(150),
            existingReservas: existing);

        // Assert
        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenNewReservationIsAdjacentToExisting()
    {
        // Arrange — existing ends exactly when new one starts (no gap, no overlap)
        var salaId = Guid.NewGuid();
        var profId = Guid.NewGuid();

        var existingEnd   = Future(120);
        var existing = new List<Reserva>
        {
            BuildReserva(salaId: salaId, startsAt: Future(60), endsAt: existingEnd)
        };

        // Act — starts exactly when existing ends
        var act = () => Reserva.Create(
            salaId, profId, "Adjacent",
            startsAt: existingEnd,
            endsAt:   existingEnd.AddHours(1),
            existingReservas: existing);

        // Assert — adjacent slots are allowed
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenSameTimeSlotIsForDifferentRoom()
    {
        // Arrange — existing reservation is for a DIFFERENT room
        var salaA  = Guid.NewGuid();
        var salaB  = Guid.NewGuid();
        var profId = Guid.NewGuid();

        var existing = new List<Reserva>
        {
            BuildReserva(salaId: salaA, startsAt: Future(60), endsAt: Future(120))
        };

        // Act — same time, but different room
        var act = () => Reserva.Create(
            salaB, profId, "Different Room",
            startsAt: Future(60),
            endsAt:   Future(120),
            existingReservas: existing);

        act.Should().NotThrow();
    }

    // ── Business Rule 7: Only the owner can mutate ───────────────────────────

    [Fact]
    public void Update_ShouldThrow_WhenCalledByDifferentProfessor()
    {
        // Arrange
        var ownerProfId = Guid.NewGuid();
        var otherProfId = Guid.NewGuid();
        var reserva     = BuildReserva(professorId: ownerProfId);

        // Act — a different professor tries to update
        var act = () => reserva.Update(
            requestingProfessorId: otherProfId,
            newSubject:  "Hijacked Subject",
            newStartsAt: Future(200),
            newEndsAt:   Future(260),
            existingReservas: []);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void ValidateCanDelete_ShouldThrow_WhenCalledByDifferentProfessor()
    {
        var ownerProfId = Guid.NewGuid();
        var otherProfId = Guid.NewGuid();
        var reserva     = BuildReserva(professorId: ownerProfId);

        var act = () => reserva.ValidateCanDelete(otherProfId);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void Update_ShouldSucceed_WhenCalledByOwner()
    {
        var ownerProfId = Guid.NewGuid();
        var reserva     = BuildReserva(professorId: ownerProfId);

        var act = () => reserva.Update(
            requestingProfessorId: ownerProfId,
            newSubject:  "Updated Subject",
            newStartsAt: Future(200),
            newEndsAt:   Future(260),
            existingReservas: []);

        act.Should().NotThrow();
        reserva.Subject.Should().Be("Updated Subject");
    }

    // ── Subject validation ────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("AB")]   // shorter than 3 chars
    public void Create_ShouldThrow_WhenSubjectIsInvalid(string subject)
    {
        var act = () => Reserva.Create(
            Guid.NewGuid(), Guid.NewGuid(), subject,
            Future(60), Future(120), []);

        act.Should().Throw<DomainException>();
    }
}
