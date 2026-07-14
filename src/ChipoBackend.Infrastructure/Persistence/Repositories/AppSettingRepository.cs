using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class AppSettingRepository(AppDbContext context) : IAppSettingRepository
{
    public async Task<AppSetting?> GetAsync(string key, CancellationToken ct = default) =>
        await context.AppSettings.FirstOrDefaultAsync(s => s.Key == key, ct);

    public void Add(AppSetting setting) => context.AppSettings.Add(setting);
}
