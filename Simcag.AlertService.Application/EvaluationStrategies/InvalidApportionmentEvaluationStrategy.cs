using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de rateio inválido
/// </summary>
public sealed class InvalidApportionmentEvaluationStrategy : IAlertEvaluationStrategy
{
    public AlertType SupportedType => AlertType.InvalidApportionment;

    public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalysisCompletedEvent evt, CancellationToken ct)
    {
        if (evt.CostCenterShares == null || evt.CostCenterShares.Count == 0)
            return Task.FromResult<Alert?>(null);

        var total = evt.CostCenterShares.Values.Sum();
        var deviation = Math.Abs(total - 100m);

        // Tolerância de 2%
        if (deviation <= 2m) 
            return Task.FromResult<Alert?>(null);

        var message = $"Rateio inválido: soma é {total:F1}%";

        var alert = Alert.Create(
            evt.ProductId, evt.ProductName, evt.Category,
            "InvalidApportionment", "Rateio Inválido",
            AlertSeverity.Critical, deviation, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);

        return Task.FromResult<Alert?>(alert);
    }
}
