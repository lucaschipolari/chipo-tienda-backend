using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == EmailAddress.Of(email), ct);

    public async Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role).ThenInclude(r => r.Permissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetWithRolesAsync(string email, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role).ThenInclude(r => r.Permissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == EmailAddress.Of(email), ct);

    public async Task<User?> GetByRefreshTokenAsync(string tokenHash, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.RefreshTokens)
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash), ct);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // Construir la query base con las navegaciones necesarias para el listado
        var query = DbSet
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        // Filtro de búsqueda
        // EF.Property<string> permite acceder a la columna "email" mapeada por HasConversion
        // sin tocar EmailAddress.Value, que no es traducible a SQL.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                EF.Property<string>(u, "email").ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
