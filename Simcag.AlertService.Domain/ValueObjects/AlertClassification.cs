using Simcag.AlertService.Domain.Enums;

namespace Simcag.AlertService.Domain.ValueObjects;

using Simcag.AlertService.Domain.Enums;

public class AlertClassification
{
    public AlertSeverity Severity { get; }
    public decimal DeviationThreshold { get; }

    private AlertClassification(AlertSeverity severity, decimal threshold)
    {
        Severity = severity;
        DeviationThreshold = threshold;
    }

    public static AlertClassification Classify(decimal deviationPercentage)
    {
        var absDeviation = Math.Abs(deviationPercentage);
        return absDeviation switch
        {
            >= 25m => new AlertClassification(AlertSeverity.Critical, 25m),
            >= 15m => new AlertClassification(AlertSeverity.Medium, 15m),
            >= 5m => new AlertClassification(AlertSeverity.Info, 5m),
            _ => new AlertClassification(AlertSeverity.Info, 0m)
        };
    }

    public static AlertClassification FromSeverity(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Critical => new AlertClassification(AlertSeverity.Critical, 25m),
        AlertSeverity.Warning => new AlertClassification(AlertSeverity.Medium, 15m),
        AlertSeverity.Info => new AlertClassification(AlertSeverity.Info, 5m),
        _ => new AlertClassification(AlertSeverity.Info, 0m)
    };
}
