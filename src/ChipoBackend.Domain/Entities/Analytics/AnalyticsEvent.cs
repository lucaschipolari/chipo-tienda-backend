using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Analytics;

public enum AnalyticsEventType
{
    ProductView = 0,
    AddToCart = 1,
    Favorite = 2,
    Search = 3,
}

/// <summary>
/// Evento anónimo de interacción en la tienda (vista de producto, agregado al
/// carrito, favorito, búsqueda). Sin datos personales: solo un sessionId
/// aleatorio para estimar visitantes únicos.
/// </summary>
public class AnalyticsEvent : BaseEntity
{
    public AnalyticsEventType Type { get; private set; }
    public Guid? ProductId { get; private set; }
    public string? SearchTerm { get; private set; }
    public int? ResultCount { get; private set; }
    public string? SessionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AnalyticsEvent() { }

    public static AnalyticsEvent ForProduct(AnalyticsEventType type, Guid productId, string? sessionId) => new()
    {
        Type = type,
        ProductId = productId,
        SessionId = Trim(sessionId, 64),
        CreatedAt = DateTime.UtcNow,
    };

    public static AnalyticsEvent ForSearch(string term, int resultCount, string? sessionId) => new()
    {
        Type = AnalyticsEventType.Search,
        SearchTerm = Trim(term, 120),
        ResultCount = resultCount,
        SessionId = Trim(sessionId, 64),
        CreatedAt = DateTime.UtcNow,
    };

    private static string? Trim(string? s, int max)
        => string.IsNullOrWhiteSpace(s) ? null : (s.Length > max ? s[..max] : s);
}
