using ChipoBackend.Domain.Entities.Users;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
}
