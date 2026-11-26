namespace ChipoBackend.Domain.Common;

/// <summary>
/// Redondeo comercial de costos y precios en ARS al múltiplo de $1.000 que usa la Calculadora:
/// si los cientos son menores a $300 redondea para abajo, si no para arriba.
/// Ej.: 134.200 → 134.000; 134.300 → 135.000. Un monto positivo nunca queda en 0.
/// </summary>
public static class PriceRounding
{
    public const decimal Step = 1000m;
    public const decimal RoundDownBelow = 300m;

    public static decimal Round(decimal amount)
    {
        if (amount <= 0) return 0;

        var floor = Math.Floor(amount / Step) * Step;
        var remainder = amount - floor;
        if (remainder == 0) return floor;

        var rounded = remainder < RoundDownBelow ? floor : floor + Step;
        return rounded == 0 ? Step : rounded;
    }
}
