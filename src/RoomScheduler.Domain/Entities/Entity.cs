namespace RoomScheduler.Domain.Entities;

/// <summary>
/// Abstract base for all domain entities.
/// Equality is identity-based (same Id == same entity), not reference-based.
/// TId is kept generic so we can use Guid for all aggregates without boxing.
/// </summary>
public abstract class Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; protected init; }

    protected Entity() { }  // Required by EF Core

    protected Entity(TId id)
    {
        if (id.Equals(default))
            throw new ArgumentException("Entity Id cannot be the default value.", nameof(id));

        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other))  return true;
        if (GetType() != other.GetType())  return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
