using Simcag.AlertService.Domain.Enums;

namespace Simcag.AlertService.Domain.ValueObjects;

public class DeviationPercentage
{
    public decimal Value { get; }

    private DeviationPercentage(decimal value)
    {
        Value = value;
    }

    public static DeviationPercentage Create(decimal value)
    {
        if (value < -100 || value > 100)
            throw new ArgumentException("Deviação deve estar entre -100% e 100%", nameof(value));
        return new DeviationPercentage(value);
    }

    public bool IsAboveThreshold(decimal threshold) => Math.Abs(Value) > threshold;
    public bool IsPositive() => Value > 0;
    public bool IsNegative() => Value < 0;
}
