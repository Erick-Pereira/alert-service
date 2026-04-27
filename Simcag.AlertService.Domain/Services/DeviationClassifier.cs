using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;

namespace Simcag.AlertService.Domain.Services;

/// <summary>
/// Serviço de domínio responsável por classificar a gravidade de um desvio percentual
/// </summary>
public static class DeviationClassifier
{
    /// <summary>
    /// Classifica a severidade com base no percentual de desvio absoluto
    /// </summary>
    public static AlertClassification Classify(decimal deviationPercentage)
    {
        // Delegate to AlertClassification value object factory
        return AlertClassification.Classify(deviationPercentage);
    }

    /// <summary>
    /// Determina a severidade com base no desvio e no threshold da regra
    /// </summary>
    public static AlertSeverity DetermineSeverity(decimal actualDeviation, decimal threshold)
    {
        var absDeviation = Math.Abs(actualDeviation);
        if (absDeviation >= threshold * 2) return AlertSeverity.Critical;
        if (absDeviation >= threshold) return AlertSeverity.Medium;
        return AlertSeverity.Low;
    }
}
