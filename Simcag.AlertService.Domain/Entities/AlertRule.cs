using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;

namespace Simcag.AlertService.Domain.Entities;

/// <summary>
/// Regra de detecção de anomalias financeiras
/// </summary>
public class AlertRule
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string? SupplierId { get; private set; }
    public AlertType Type { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public decimal Threshold { get; private set; }
    public int MinDataPoints { get; private set; } = 5;
    public TimeSpan EvaluationWindow { get; private set; } = TimeSpan.FromDays(30);
    public AlertSeverity DefaultSeverity { get; private set; } = AlertSeverity.Medium;

    private AlertRule() { }

    public static AlertRule Create(
        string name,
        string description,
        string category,
        string? supplierId,
        AlertType type,
        decimal threshold,
        AlertSeverity severity = AlertSeverity.Medium)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da regra é obrigatório", nameof(name));
        if (threshold <= 0)
            throw new ArgumentException("Threshold deve ser positivo", nameof(threshold));

        return new AlertRule
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            SupplierId = supplierId,
            Type = type,
            Threshold = threshold,
            DefaultSeverity = severity
        };
    }

    public void SetCategory(string category) => Category = category;
    public void SetSupplierId(string? supplierId) => SupplierId = supplierId;

    public void Disable() => IsEnabled = false;
    public void Enable() => IsEnabled = true;
    public void UpdateThreshold(decimal newThreshold)
    {
        if (newThreshold <= 0)
            throw new ArgumentException("Threshold deve ser positivo", nameof(newThreshold));
        Threshold = newThreshold;
    }

    public bool ShouldEvaluate(int availableDataPoints) =>
        IsEnabled && availableDataPoints >= MinDataPoints;

    public AlertSeverity DetermineSeverity(decimal actualDeviation) =>
        Math.Abs(actualDeviation) >= Threshold * 2 ? AlertSeverity.Critical :
        Math.Abs(actualDeviation) >= Threshold ? DefaultSeverity :
        AlertSeverity.Low;
}
