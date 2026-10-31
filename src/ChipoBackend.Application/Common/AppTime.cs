namespace ChipoBackend.Application.Common;

/// <summary>
/// Hora local del negocio (Argentina, UTC-3 fijo, sin horario de verano).
/// Los timestamps se guardan en UTC, pero el agrupado por día/semana/mes del
/// dashboard debe hacerse en hora local; si no, las ventas de la noche
/// (después de las 21:00) caen en el día UTC siguiente.
/// </summary>
public static class AppTime
{
    /// <summary>Desfase fijo de Argentina respecto de UTC.</summary>
    public static readonly TimeSpan ArgentinaOffset = TimeSpan.FromHours(-3);

    /// <summary>Un instante UTC convertido a hora local Argentina.</summary>
    public static DateTime ToLocal(DateTime utc) => utc + ArgentinaOffset;

    /// <summary>Una hora local Argentina convertida al UTC equivalente.</summary>
    public static DateTime ToUtc(DateTime local) => local - ArgentinaOffset;

    /// <summary>"Ahora" en hora local Argentina.</summary>
    public static DateTime LocalNow => DateTime.UtcNow + ArgentinaOffset;

    /// <summary>La fecha de hoy en hora local Argentina.</summary>
    public static DateTime LocalToday => LocalNow.Date;
}
