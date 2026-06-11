using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.AlertService.Domain.ValueObjects;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de superfaturamento vs preço de mercado
/// </summary>
public sealed class OverpriceMarketEvaluationStrategy : IAlertEvaluationStrategy
{
    public AlertType SupportedType => AlertType.OverpriceMarket;

    public Alert? Evaluate(AlertRule rule, PriceAnalyzedEvent evt)
    {
        if (evt.MarketAverage <= 0) return null;

        var deviation = (evt.LastPrice - evt.MarketAverage) / evt.MarketAverage * 100m;

        if (!DeviationPercentage.Create(deviation).IsAboveThreshold(rule.Threshold))
            return null;

        var severity = deviation >= 25m ? AlertSeverity.Critical :
                       deviation >= 15m ? AlertSeverity.Medium :
                       AlertSeverity.Low;

        var message = $"Superfaturamento: NF R$ {evt.LastPrice:F2} vs " +
            $"mercado R$ {evt.MarketAverage:F2} ({deviation:F1}%)";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.Category,
            "OverpriceMarket", "Superfaturamento",
            severity, deviation, message,
            evt.LastPrice, evt.MarketAverage, evt.AnalysisDate, evt.ExpenseId);
    }

    public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalyzedEvent evt, CancellationToken ct) =>
        Task.FromResult(Evaluate(rule, evt));
}
