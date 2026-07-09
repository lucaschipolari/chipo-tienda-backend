using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChipoBackend.Infrastructure.Auth;

public class JwtService(IConfiguration configuration) : IJwtService
{
    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", user.FullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Token, string Hash, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);
        var hash = HashToken(token);
        var expiresAt = DateTime.UtcNow.AddDays(int.Parse(configuration["Jwt:RefreshTokenExpiryDays"] ?? "7"));
        return (token, hash, expiresAt);
    }

    public bool ValidateRefreshTokenHash(string token, string hash) =>
        HashToken(token) == hash;

    public string HashRefreshToken(string rawToken) => HashToken(rawToken);

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
