using ChipoBackend.Domain.Entities.Users;

namespace ChipoBackend.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    (string Token, string Hash, DateTime ExpiresAt) GenerateRefreshToken();
    bool ValidateRefreshTokenHash(string token, string hash);
}
