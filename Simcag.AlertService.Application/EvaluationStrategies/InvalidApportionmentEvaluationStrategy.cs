using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.EvaluationStrategies;

/// <summary>
/// Estratégia para avaliação de rateio inválido (requer CostCenterShares — não preenchido no canónico v1).
/// </summary>
public sealed class InvalidApportionmentEvaluationStrategy : IAlertEvaluationStrategy
{
    public AlertType SupportedType => AlertType.InvalidApportionment;

    public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalyzedEvent evt, CancellationToken ct) =>
        Task.FromResult<Alert?>(null);
}
