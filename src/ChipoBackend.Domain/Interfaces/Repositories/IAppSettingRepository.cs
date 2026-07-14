using ChipoBackend.Domain.Entities.Config;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IAppSettingRepository
{
    Task<AppSetting?> GetAsync(string key, CancellationToken ct = default);
    void Add(AppSetting setting);
}
