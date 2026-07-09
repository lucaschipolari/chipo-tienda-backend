using ChipoBackend.Domain.Entities.Users;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Lista paginada de usuarios con búsqueda opcional por nombre o email.
    /// La búsqueda de email usa EF.Property para evitar el problema de HasConversion.
    /// </summary>
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
