using Simcag.AlertService.Domain.Enums;

namespace Simcag.AlertService.Domain.ValueObjects;

/// <summary>
/// Value object que representa a classificação de um alerta baseado na severidade
/// </summary>
public class AlertClassification
{
    public AlertSeverity Severity { get; init; }
    public decimal DeviationThreshold { get; init; }

    /// <summary>
    /// Construtor interno para uso nos métodos de fábrica
    /// </summary>
    internal AlertClassification(AlertSeverity severity, decimal threshold)
    {
        Severity = severity;
        DeviationThreshold = threshold;
    }

    /// <summary>
    /// Classifica a severidade com base no percentual de desvio absoluto
    /// </summary>
    public static AlertClassification Classify(decimal deviationPercentage)
    {
        var absDeviation = Math.Abs(deviationPercentage);
        return absDeviation switch
        {
            >= 25m => new AlertClassification(AlertSeverity.Critical, 25m),
            >= 15m => new AlertClassification(AlertSeverity.Medium, 15m),
            >= 5m => new AlertClassification(AlertSeverity.Low, 5m),
            _ => new AlertClassification(AlertSeverity.Low, decimal.Zero)
        };
    }

    /// <summary>
    /// Cria uma classificação a partir de uma severidade específica
    /// </summary>
    public static AlertClassification FromSeverity(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Critical => new AlertClassification(AlertSeverity.Critical, 25m),
        AlertSeverity.Medium => new AlertClassification(AlertSeverity.Medium, 15m),
        AlertSeverity.Low => new AlertClassification(AlertSeverity.Low, 5m),
        _ => new AlertClassification(AlertSeverity.Low, decimal.Zero)
    };
}