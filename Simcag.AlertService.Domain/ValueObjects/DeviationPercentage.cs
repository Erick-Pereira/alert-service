using Simcag.AlertService.Domain.Enums;

namespace Simcag.AlertService.Domain.ValueObjects;

public class DeviationPercentage
{
    /// <summary>Alinhado com price-analysis <c>numeric(10,2)</c> e auditoria de superfaturamento extremo.</summary>
    public const decimal MaxStoredPercent = 9999.99m;

    public decimal Value { get; }

    private DeviationPercentage(decimal value)
    {
        Value = value;
    }

    public static DeviationPercentage Create(decimal value)
    {
        var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        if (rounded > MaxStoredPercent)
            rounded = MaxStoredPercent;
        if (rounded < -MaxStoredPercent)
            rounded = -MaxStoredPercent;
        return new DeviationPercentage(rounded);
    }

    public bool IsAboveThreshold(decimal threshold) => Math.Abs(Value) > threshold;
    public bool IsPositive() => Value > 0;
    public bool IsNegative() => Value < 0;
}
