using ChipoBackend.Domain.Entities.Users;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
}
