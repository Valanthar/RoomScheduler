namespace RoomScheduler.Application.Interfaces;

/// <summary>
/// Abstracts password hashing so the Application layer is not coupled to ASP.NET Core Identity.
/// </summary>
public interface IPasswordHasherService
{
    string Hash(string plainTextPassword);
    bool   Verify(string plainTextPassword, string hash);
}
