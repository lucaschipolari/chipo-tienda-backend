namespace ChipoBackend.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Registra una entidad nueva en el change tracker como Added.
    /// Necesario cuando la entidad fue creada vía un método de dominio sobre una
    /// navegación que no fue eager-loaded, ya que EF Core la detectaría como Modified.
    /// </summary>
    void Add<T>(T entity) where T : class;

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
