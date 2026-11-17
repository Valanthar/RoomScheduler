using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Domain.Entities;

/// <summary>
/// Aggregate root representing a bookable room.
///
/// Invariants enforced:
///   - Name cannot be null/whitespace.
///   - Capacity must be at least 1.
///   - Reservations are owned by this aggregate and exposed read-only
///     so the conflict-check logic can remain inside the domain (Business Rule 5).
/// </summary>
public sealed class Sala : Entity<Guid>
{
    private readonly List<Reserva> _reservas = [];

    public string Name       { get; private set; } = default!;
    public int    Capacity   { get; private set; }
    public bool   IsActive   { get; private set; }

    /// <summary>
    /// Read-only view of this room's reservations.
    /// Loaded by the repository when conflict checking is needed.
    /// </summary>
    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

    // ── EF Core ─────────────────────────────────────────────────────────────────
    private Sala() { }

    public static Sala Create(string name, int capacity)
    {
        ValidateName(name);
        ValidateCapacity(capacity);

        return new Sala
        {
            Id       = Guid.NewGuid(),
            Name     = name.Trim(),
            Capacity = capacity,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate()   => IsActive = true;

    // ── Private guards ──────────────────────────────────────────────────────────

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Room name cannot be empty.");
    }

    private static void ValidateCapacity(int capacity)
    {
        if (capacity < 1)
            throw new DomainException("Room capacity must be at least 1.");
    }
}
