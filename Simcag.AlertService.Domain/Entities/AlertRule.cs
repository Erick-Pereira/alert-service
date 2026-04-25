using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;

namespace Simcag.AlertService.Domain.Entities;

using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;

/// <summary>
/// Regra de detecção de anomalias financeiras
/// </summary>
public class AlertRule
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AlertType Type { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public decimal Threshold { get; private set; }
    public int MinDataPoints { get; private set; } = 5;
    public TimeSpan EvaluationWindow { get; private set; } = TimeSpan.FromDays(30);
    public AlertSeverity DefaultSeverity { get; private set; } = AlertSeverity.Warning;

    private AlertRule() { }

    public static AlertRule Create(
        string name,
        string description,
        AlertType type,
        decimal threshold,
        AlertSeverity severity = AlertSeverity.Warning)
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
            Type = type,
            Threshold = threshold,
            DefaultSeverity = severity
        };
    }

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
        AlertSeverity.Info;
}
