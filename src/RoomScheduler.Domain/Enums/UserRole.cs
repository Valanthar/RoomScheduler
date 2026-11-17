namespace RoomScheduler.Domain.Enums;

/// <summary>
/// Roles recognized by the authorization layer (Business Rule 4).
/// Stored as an integer column in the database.
/// </summary>
public enum UserRole
{
    Professor = 0,
    Admin     = 1
}
