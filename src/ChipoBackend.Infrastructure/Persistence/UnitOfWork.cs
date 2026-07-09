using ChipoBackend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChipoBackend.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    // context.Add<T> sets EntityState = Added regardless of key value,
    // ensuring EF Core generates INSERT rather than UPDATE.
    public void Add<T>(T entity) where T : class => context.Add(entity);

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            await _transaction.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            await _transaction.RollbackAsync(ct);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}
