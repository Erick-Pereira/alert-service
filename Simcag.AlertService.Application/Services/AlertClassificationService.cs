using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Simcag.AlertService.Application.Services;

/// <summary>
/// Serviço de classificação de desvios e busca de regras
/// </summary>
public sealed class AlertClassificationService : IAlertRuleService
{
    private readonly IAlertRuleRepository _ruleRepository;
    private readonly ILogger<AlertClassificationService> _logger;

    public AlertClassificationService(
        IAlertRuleRepository ruleRepository,
        ILogger<AlertClassificationService> logger)
    {
        _ruleRepository = ruleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AlertRule>> GetApplicableRulesAsync(
        string category,
        string? supplierId,
        CancellationToken ct)
    {
        var rules = await _ruleRepository.GetActiveRulesAsync(ct);

        // Filtrar por categoria se especificada
        if (!string.IsNullOrEmpty(category))
        {
            rules = rules.Where(r => 
                string.IsNullOrEmpty(r.Category) || 
                r.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(supplierId))
        {
            rules = rules.Where(r =>
                string.IsNullOrEmpty(r.SupplierId) ||
                r.SupplierId.Equals(supplierId, System.StringComparison.OrdinalIgnoreCase));
        }

        return rules;
    }

    public async Task<DeviationPercentage> CalculateDeviationAsync(
        decimal currentValue,
        decimal baseline,
        CancellationToken ct)
    {
        if (baseline == 0)
        {
            _logger.LogWarning("Baseline is zero, cannot calculate deviation");
            return DeviationPercentage.Create(0);
        }

        var deviation = (currentValue - baseline) / baseline * 100m;
        return DeviationPercentage.Create(deviation);
    }

    public async Task<AlertClassification> ClassifyDeviationAsync(
        decimal deviationPercentage,
        CancellationToken ct)
    {
        var classification = AlertClassification.Classify(deviationPercentage);
        await Task.CompletedTask;
        return classification;
    }
}
