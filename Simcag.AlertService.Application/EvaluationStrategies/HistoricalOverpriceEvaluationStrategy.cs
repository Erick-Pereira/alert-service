using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.AlertService.Domain.ValueObjects;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de desvio histórico (Application)
/// </summary>
public sealed class HistoricalOverpriceEvaluationStrategy : IAlertEvaluationStrategy
{
    private readonly IAlertRuleService _ruleService;

    public HistoricalOverpriceEvaluationStrategy(IAlertRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    public AlertType SupportedType => AlertType.OverpriceHistorical;

    public async Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalysisCompletedEvent evt, CancellationToken ct)
    {
        if (evt.AveragePrice <= 0)
            return null;

        var deviation = (evt.LastPrice - evt.AveragePrice) / evt.AveragePrice * 100m;

        var classification = await _ruleService.ClassifyDeviationAsync(deviation, ct);

        if (classification.Severity == AlertSeverity.Low)
            return null;

        if (!DeviationPercentage.Create(deviation).IsAboveThreshold(rule.Threshold))
            return null;

        // Validação de dispersão (outlier removal)
        if (evt.StandardDeviation > evt.AveragePrice * 0.5m)
            return null;

        var message = $"Desvio histórico: R$ {evt.LastPrice:F2} vs " +
            $"média R$ {evt.AveragePrice:F2} ({deviation:F1}%)";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.Category,
            "OverpriceHistorical", "Desvio Histórico",
            classification.Severity, deviation, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);
    }
}