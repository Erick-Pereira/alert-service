using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de escalada de preços do fornecedor
/// </summary>
public sealed class SupplierEscalationEvaluationStrategy : IAlertEvaluationStrategy
{
    public AlertType SupportedType => AlertType.SupplierEscalation;

    public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalyzedEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.SupplierId) ||
            evt.PriceHistory == null || evt.PriceHistory.Count < 4)
        {
            return Task.FromResult<Alert?>(null);
        }

        var consecutive = 0;
        var maxConsecutive = 0;
        for (var i = 1; i < evt.PriceHistory.Count; i++)
        {
            if (evt.PriceHistory[i] > evt.PriceHistory[i - 1])
            {
                consecutive++;
                if (consecutive > maxConsecutive)
                    maxConsecutive = consecutive;
            }
            else
            {
                consecutive = 0;
            }
        }

        if (maxConsecutive < 3)
            return Task.FromResult<Alert?>(null);

        var first = evt.PriceHistory[0];
        var last = evt.PriceHistory[^1];
        var totalDev = (last - first) / first * 100m;

        if (totalDev < rule.Threshold)
            return Task.FromResult<Alert?>(null);

        var severity = totalDev >= 25m ? AlertSeverity.Critical :
                       totalDev >= 15m ? AlertSeverity.Medium :
                       AlertSeverity.Low;

        var message = $"Escalada: {maxConsecutive} aumentos consecutivos, " +
            $"variação {totalDev:F1}%";

        var alert = Alert.Create(
            evt.ProductId, evt.ProductName, evt.Category,
            "SupplierEscalation", "Escalada de Fornecedor",
            severity, totalDev, message,
            evt.LastPrice, evt.HistoricalAverage, evt.AnalysisDate, evt.ExpenseId);

        return Task.FromResult<Alert?>(alert);
    }
}
