using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Simcag.AlertService.Application.Services;

public class AlertClassificationService : IAlertRuleService
{
    private readonly ILogger<AlertClassificationService> _logger;

    public AlertClassificationService(ILogger<AlertClassificationService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<AlertRule>> GetApplicableRulesAsync(
        string category,
        string? supplierId,
        CancellationToken ct)
    {
        // In production, this would fetch from database
        // For now, return default rules
        var rules = new List<AlertRule>
        {
            AlertRule.Create(
                "Market Overprice Detection",
                "Detects when purchase price exceeds market price by threshold",
                AlertType.OverpriceMarket,
                15m, // 15% threshold
                AlertSeverity.Warning),

            AlertRule.Create(
                "Historical Deviation Detection",
                "Detects significant deviation from historical average",
                AlertType.OverpriceHistorical,
                15m,
                AlertSeverity.Warning),

            AlertRule.Create(
                "Supplier Price Escalation",
                "Detects consecutive price increases from supplier",
                AlertType.SupplierEscalation,
                15m,
                AlertSeverity.Warning),

            AlertRule.Create(
                "Supplier Concentration Risk",
                "Detects over-dependence on single supplier",
                AlertType.SupplierConcentration,
                60m, // 60% of category purchases
                AlertSeverity.Warning),

            AlertRule.Create(
                "Invalid Apportionment Detection",
                "Detects invalid cost center apportionment",
                AlertType.InvalidApportionment,
                2m, // 2% tolerance
                AlertSeverity.Critical)
        };

        // Filter by category if specified
        if (!string.IsNullOrEmpty(category))
        {
            // Could apply category-specific rules here
        }

        await Task.CompletedTask;
        return rules.Where(r => r.IsEnabled);
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
