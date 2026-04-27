using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de concentração de fornecedor
/// </summary>
public sealed class SupplierConcentrationEvaluationStrategy : IAlertEvaluationStrategy
{
    public AlertType SupportedType => AlertType.SupplierConcentration;

    public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalysisCompletedEvent evt, CancellationToken ct)
    {
        if (evt.SupplierCategoryShare < rule.Threshold) 
            return Task.FromResult<Alert?>(null);

        var severity = evt.SupplierCategoryShare >= 75m ? AlertSeverity.Critical :
                       AlertSeverity.Medium;

        var message = $"Concentração: {evt.SupplierId} detém " +
            $"{evt.SupplierCategoryShare:F1}% da categoria";

        var alert = Alert.Create(
            evt.ProductId, evt.ProductName, evt.Category,
            "SupplierConcentration", "Concentração de Fornecedor",
            severity, evt.SupplierCategoryShare, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);

        return Task.FromResult<Alert?>(alert);
    }
}
