using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Security;

/// <summary>
/// Generates a signed HS256 JWT containing the professor's Id and Role claims.
/// Configuration is read from appsettings (Jwt:Secret, Jwt:Issuer, Jwt:Audience,
/// Jwt:ExpirationMinutes).
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAt) GenerateToken(Professor professor)
    {
        var secret  = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret is not configured.");
        var issuer   = _config["Jwt:Issuer"]   ?? "RoomScheduler";
        var audience = _config["Jwt:Audience"] ?? "RoomScheduler";
        var minutes  = int.Parse(_config["Jwt:ExpirationMinutes"] ?? "60");

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt   = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   professor.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, professor.Email),
            new Claim(ClaimTypes.Role,               professor.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
