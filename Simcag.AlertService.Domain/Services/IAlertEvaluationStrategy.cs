using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Domain.Services;

/// <summary>
/// Estratégia para avaliar um tipo específico de regra de alerta
/// </summary>
public interface IAlertEvaluationStrategy
{
    AlertType SupportedType { get; }
    Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalysisCompletedEvent evt, CancellationToken ct);
}
