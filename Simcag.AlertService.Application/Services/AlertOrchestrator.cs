using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Simcag.Shared.Events;

namespace Simcag.AlertService.Application.Services;

public class AlertOrchestrator
{
    private readonly IAlertRuleService _ruleService;
    private readonly IRedisCacheService _cacheService;
    private readonly IEventPublisher<AlertTriggeredEvent> _eventPublisher;
    private readonly ILogger<AlertOrchestrator> _logger;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromHours(24);

    public AlertOrchestrator(
        IAlertRuleService ruleService,
        IRedisCacheService cacheService,
        IEventPublisher<AlertTriggeredEvent> eventPublisher,
        ILogger<AlertOrchestrator> logger)
    {
        _ruleService = ruleService;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Alert?> OrchestrateAlertEvaluationAsync(
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        using var scope = _logger.BeginScope(
            "Orchestrating alert evaluation for {ProductId}",
            evt.ProductId);

        var rules = await _ruleService.GetApplicableRulesAsync(
            evt.ProductCategory,
            evt.SupplierId,
            ct);

        var applicableRules = rules.Where(r => r.ShouldEvaluate(evt.DataPointsCount)).ToList();

        _logger.LogInformation(
            "Evaluating {RuleCount} rules for product {ProductId}",
            applicableRules.Count,
            evt.ProductId);

        var alerts = new List<Alert>();

        foreach (var rule in applicableRules)
        {
            var alert = await EvaluateRuleAsync(rule, evt, ct);
            if (alert != null)
            {
                alerts.Add(alert);
            }
        }

        if (alerts.Count == 0)
        {
            _logger.LogInformation(
                "No alerts triggered for product {ProductId}",
                evt.ProductId);
            return null;
        }

        // Select most severe alert
        var primaryAlert = alerts.OrderByDescending(a => (int)a.Severity).First();

        // Check for duplication
        var isDuplicate = await _cacheService.IsDuplicateAlertAsync(
            "Product",
            primaryAlert.ProductId,
            AlertCooldown,
            ct);

        if (isDuplicate)
        {
            _logger.LogInformation(
                "Duplicate alert suppressed for {ProductId}",
                primaryAlert.ProductId);
            return primaryAlert;
        }

        await _cacheService.MarkAlertAsSentAsync(
            "Product",
            primaryAlert.ProductId,
            AlertCooldown,
            ct);

        // Publish combined alert event
        var alertEvent = new AlertTriggeredEvent
        {
            AlertId = primaryAlert.Id,
            ProductId = primaryAlert.ProductId,
            ProductName = primaryAlert.ProductName,
            Category = primaryAlert.Category,
            AlertType = primaryAlert.Type,
            AlertCategory = primaryAlert.AlertCategory,
            Severity = primaryAlert.Severity.ToString(),
            DeviationPercentage = primaryAlert.DeviationPercentage,
            Message = primaryAlert.Message,
            CurrentPrice = primaryAlert.CurrentPrice,
            AveragePrice = primaryAlert.AveragePrice,
            OccurredAt = evt.AnalyzedAt,
            GeneratedAt = primaryAlert.CreatedAt,
            Source = "AlertOrchestrator"
        };

        await _eventPublisher.PublishAsync(alertEvent, ct);

        _logger.LogWarning(
            "Alert [{Severity}] {Type} for {ProductId}: {Message}",
            primaryAlert.Severity,
            primaryAlert.Type,
            primaryAlert.ProductId,
            primaryAlert.Message);

        return primaryAlert;
    }

    private async Task<Alert?> EvaluateRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        try
        {
            return rule.Type switch
            {
                AlertType.OverpriceMarket =>
                    EvaluateMarketOverprice(rule, evt),
                AlertType.OverpriceHistorical =>
                    EvaluateHistoricalOverprice(rule, evt, ct),
                AlertType.SupplierEscalation =>
                    EvaluateSupplierEscalation(rule, evt, ct),
                AlertType.SupplierConcentration =>
                    EvaluateSupplierConcentration(rule, evt),
                AlertType.InvalidApportionment =>
                    EvaluateInvalidApportionment(rule, evt),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error evaluating rule {RuleType} for {ProductId}",
                rule.Type,
                evt.ProductId);
            return null;
        }
    }

    private Alert? EvaluateMarketOverprice(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt)
    {
        if (evt.MarketPrice <= 0) return null;

        var deviation = (evt.LastPrice - evt.MarketPrice) / evt.MarketPrice * 100m;

        if (!deviation.IsAboveThreshold(rule.Threshold)) return null;

        var severity = deviation >= 25m ? AlertSeverity.Critical :
                       deviation >= 15m ? AlertSeverity.Warning :
                       AlertSeverity.Info;

        var message = $"Superfaturamento: NF R$ {evt.LastPrice:F2} vs " +
            $"mercado R$ {evt.MarketPrice:F2} ({deviation:F1}%)";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.ProductCategory,
            "OverpriceMarket", "Superfaturamento",
            severity, deviation, message,
            evt.LastPrice, evt.MarketPrice, evt.AnalyzedAt);
    }

    private async Task<Alert?> EvaluateHistoricalOverprice(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        var classification = await _ruleService.ClassifyDeviationAsync(
            (evt.LastPrice - evt.AveragePrice) / evt.AveragePrice * 100m,
            ct);

        if (classification.Severity == AlertSeverity.Info) return null;

        var deviation = (evt.LastPrice - evt.AveragePrice) / evt.AveragePrice * 100m;

        if (!deviation.IsAboveThreshold(rule.Threshold)) return null;

        var message = $"Desvio histórico: R$ {evt.LastPrice:F2} vs " +
            $"média R$ {evt.AveragePrice:F2} ({deviation:F1}%)";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.ProductCategory,
            "OverpriceHistorical", "Desvio Histórico",
            classification.Severity, deviation, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);
    }

    private Alert? EvaluateSupplierEscalation(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.SupplierId) ||
            evt.PriceHistory == null || evt.PriceHistory.Count < 3)
            return null;

        var increases = 0;
        for (int i = 1; i < evt.PriceHistory.Count; i++)
        {
            if (evt.PriceHistory[i] > evt.PriceHistory[i - 1])
                increases++;
        }

        if (increases < 3) return null;

        var first = evt.PriceHistory[0];
        var last = evt.PriceHistory[^1];
        var totalDev = (last - first) / first * 100m;

        if (totalDev < rule.Threshold) return null;

        var severity = totalDev >= 25m ? AlertSeverity.Critical :
                       totalDev >= 15m ? AlertSeverity.Warning :
                       AlertSeverity.Info;

        var message = $"Escalada: {increases} aumentos, " +
            $"variação {totalDev:F1}%";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.ProductCategory,
            "SupplierEscalation", "Escalada de Fornecedor",
            severity, totalDev, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);
    }

    private Alert? EvaluateSupplierConcentration(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt)
    {
        if (evt.SupplierCategoryShare < rule.Threshold) return null;

        var severity = evt.SupplierCategoryShare >= 75m ? AlertSeverity.Critical :
                       AlertSeverity.Warning;

        var message = $"Concentração: {evt.SupplierId} tem " +
            $"{evt.SupplierCategoryShare:F1}% da categoria";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.ProductCategory,
            "SupplierConcentration", "Concentração de Fornecedor",
            severity, evt.SupplierCategoryShare, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);
    }

    private Alert? EvaluateInvalidApportionment(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt)
    {
        if (evt.CostCenterShares == null || evt.CostCenterShares.Count == 0)
            return null;

        var total = evt.CostCenterShares.Values.Sum();
        var deviation = Math.Abs(total - 100m);

        if (deviation <= 2m) return null;

        var message = $"Rateio inválido: soma é {total:F1}%";

        return Alert.Create(
            evt.ProductId, evt.ProductName, evt.ProductCategory,
            "InvalidApportionment", "Rateio Inválido",
            AlertSeverity.Critical, deviation, message,
            evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt);
    }
}
