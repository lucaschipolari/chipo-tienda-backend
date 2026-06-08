using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class RoleRepository(AppDbContext context) : BaseRepository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(r => r.Permissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default) =>
        await context.Permissions.OrderBy(p => p.Resource).ThenBy(p => p.Action).ToListAsync(ct);
}
