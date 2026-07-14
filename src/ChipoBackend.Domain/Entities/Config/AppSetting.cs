namespace ChipoBackend.Domain.Entities.Config;

/// <summary>Configuración global simple clave-valor (reutilizable).</summary>
public class AppSetting
{
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public DateTime UpdatedAt { get; private set; }

    private AppSetting() { }

    public static AppSetting Create(string key, string value) =>
        new() { Key = key, Value = value, UpdatedAt = DateTime.UtcNow };

    public void SetValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
