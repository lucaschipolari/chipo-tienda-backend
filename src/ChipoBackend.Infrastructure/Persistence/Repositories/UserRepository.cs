using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role).ThenInclude(r => r.Permissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetWithRolesAsync(string email, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role).ThenInclude(r => r.Permissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByRefreshTokenAsync(string tokenHash, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.RefreshTokens)
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash), ct);
}
