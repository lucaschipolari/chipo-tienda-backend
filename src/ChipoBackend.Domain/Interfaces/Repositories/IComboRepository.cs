using ChipoBackend.Domain.Entities.Combos;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IComboRepository : IRepository<Combo>
{
    Task<Combo?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Combo>> GetAllWithItemsAsync(bool onlyActive, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken ct = default);
}
