using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class PricingCalculatorEntryRepository(AppDbContext context) : IPricingCalculatorEntryRepository
{
    public async Task<PricingCalculatorEntry?> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        await context.PricingCalculatorEntries.FirstOrDefaultAsync(e => e.ProductId == productId, ct);

    public async Task<Dictionary<Guid, PricingCalculatorEntry>> GetAllByProductAsync(CancellationToken ct = default) =>
        await context.PricingCalculatorEntries.AsNoTracking().ToDictionaryAsync(e => e.ProductId, ct);

    public void Add(PricingCalculatorEntry entry) => context.PricingCalculatorEntries.Add(entry);
}
