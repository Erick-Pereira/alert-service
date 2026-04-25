using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Simcag.Shared.Events;

namespace Simcag.AlertService.Application.UseCases;

public class EvaluateAlertUseCase : IAlertService
{
    private readonly IAlertRuleService _ruleService;
    private readonly IRedisCacheService _cacheService;
    private readonly IEventPublisher<AlertTriggeredEvent> _eventPublisher;
    private readonly ILogger<EvaluateAlertUseCase> _logger;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromHours(24);

    public EvaluateAlertUseCase(
        IAlertRuleService ruleService,
        IRedisCacheService cacheService,
        IEventPublisher<AlertTriggeredEvent> eventPublisher,
        ILogger<EvaluateAlertUseCase> logger)
    {
        _ruleService = ruleService;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Alert?> EvaluateAlertAsync(
        PriceAnalysisCompletedEvent analysisEvent,
        CancellationToken ct)
    {
        using var scope = _logger.BeginScope(
            "Evaluating alerts for product {ProductId} (event {EventId})",
            analysisEvent.ProductId,
            analysisEvent.EventId);

        try
        {
            // 1. Fetch applicable rules
            var rules = await _ruleService.GetApplicableRulesAsync(
                analysisEvent.ProductCategory,
                analysisEvent.SupplierId,
                ct);

            _logger.LogInformation(
                "Found {RuleCount} applicable rules for product {ProductId}",
                rules.Count(),
                analysisEvent.ProductId);

            Alert? mostSevereAlert = null;

            // 2. Evaluate each rule
            foreach (var rule in rules)
            {
                var alert = await EvaluateRuleAsync(rule, analysisEvent, ct);
                if (alert != null)
                {
                    if (mostSevereAlert == null ||
                        alert.Severity > mostSevereAlert.Severity)
                    {
                        mostSevereAlert = alert;
                    }
                }
            }

            // 3. If alert found, check for duplicates and publish
            if (mostSevereAlert != null)
            {
                var isDuplicate = await _cacheService.IsDuplicateAlertAsync(
                    "Product",
                    mostSevereAlert.ProductId,
                    AlertCooldown,
                    ct);

                if (isDuplicate)
                {
                    _logger.LogInformation(
                        "Duplicate alert suppressed for product {ProductId} (cooldown active)",
                        mostSevereAlert.ProductId);
                    return mostSevereAlert; // Return but don't publish
                }

                await _cacheService.MarkAlertAsSentAsync(
                    "Product",
                    mostSevereAlert.ProductId,
                    AlertCooldown,
                    ct);

                // 4. Publish event
                var alertEvent = CreateAlertEvent(mostSevereAlert, analysisEvent);
                await _eventPublisher.PublishAsync(alertEvent, ct);

                _logger.LogWarning(
                    "ALERT [{Severity}] {Type} for product {ProductId}: {Message}",
                    mostSevereAlert.Severity,
                    mostSevereAlert.Type,
                    mostSevereAlert.ProductId,
                    mostSevereAlert.Message);

                return mostSevereAlert;
            }

            _logger.LogInformation(
                "No alert conditions met for product {ProductId}",
                analysisEvent.ProductId);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to evaluate alerts for product {ProductId}",
                analysisEvent.ProductId);
            throw;
        }
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
                    await EvaluateMarketOverpriceRuleAsync(rule, evt, ct),
                AlertType.OverpriceHistorical =>
                    await EvaluateHistoricalOverpriceRuleAsync(rule, evt, ct),
                AlertType.SupplierEscalation =>
                    await EvaluateSupplierEscalationRuleAsync(rule, evt, ct),
                AlertType.SupplierConcentration =>
                    await EvaluateSupplierConcentrationRuleAsync(rule, evt, ct),
                AlertType.InvalidApportionment =>
                    await EvaluateInvalidApportionmentRuleAsync(rule, evt, ct),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error evaluating rule {RuleType} for product {ProductId}",
                rule.Type,
                evt.ProductId);
            return null;
        }
    }

    private async Task<Alert?> EvaluateMarketOverpriceRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        if (evt.MarketPrice <= 0) return null;

        var deviation = await _ruleService.CalculateDeviationAsync(
            evt.LastPrice,
            evt.MarketPrice,
            ct);

        if (!rule.ShouldEvaluate(1) ||
            !deviation.IsAboveThreshold(rule.Threshold))
        {
            return null;
        }

        var classification = await _ruleService.ClassifyDeviationAsync(
            deviation.Value,
            ct);

        var message = $"Superfaturamento detectado: " +
            $"preço NF R$ {evt.LastPrice:F2} vs mercado R$ {evt.MarketPrice:F2} " +
            $"({deviation.Value:F1}% acima)";

        return Alert.Create(
            evt.ProductId,
            evt.ProductName,
            evt.ProductCategory,
            "OverpriceMarket",
            "Superfaturamento",
            classification.Severity,
            deviation.Value,
            message,
            evt.LastPrice,
            evt.MarketPrice,
            evt.AnalyzedAt);
    }

    private async Task<Alert?> EvaluateHistoricalOverpriceRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        var deviation = await _ruleService.CalculateDeviationAsync(
            evt.LastPrice,
            evt.AveragePrice,
            ct);

        if (!rule.ShouldEvaluate(evt.DataPointsCount) ||
            !deviation.IsAboveThreshold(rule.Threshold) ||
            evt.StandardDeviation > evt.AveragePrice * 0.5m)
        {
            return null;
        }

        var classification = await _ruleService.ClassifyDeviationAsync(
            deviation.Value,
            ct);

        var message = $"Desvio histórico: " +
            $"preço atual R$ {evt.LastPrice:F2} vs média R$ {evt.AveragePrice:F2} " +
            $"({deviation.Value:F1}% acima)";

        return Alert.Create(
            evt.ProductId,
            evt.ProductName,
            evt.ProductCategory,
            "OverpriceHistorical",
            "Desvio Histórico",
            classification.Severity,
            deviation.Value,
            message,
            evt.LastPrice,
            evt.AveragePrice,
            evt.AnalyzedAt);
    }

    private async Task<Alert?> EvaluateSupplierEscalationRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.SupplierId) ||
            evt.PriceHistory == null ||
            evt.PriceHistory.Count < 3)
        {
            return null;
        }

        // Check for 3+ consecutive increases
        var increases = 0;
        for (int i = 1; i < evt.PriceHistory.Count; i++)
        {
            if (evt.PriceHistory[i] > evt.PriceHistory[i - 1])
                increases++;
        }

        if (increases < 3) return null;

        // Calculate trend magnitude
        var firstPrice = evt.PriceHistory[0];
        var lastPrice = evt.PriceHistory[^1];
        var totalDeviation = (lastPrice - firstPrice) / firstPrice * 100m;

        if (totalDeviation < rule.Threshold) return null;

        var classification = await _ruleService.ClassifyDeviationAsync(
            totalDeviation,
            ct);

        var message = $"Escalada de preços do fornecedor: " +
            $"{increases} aumentos consecutivos, " +
            $"variação total de {totalDeviation:F1}%";

        return Alert.Create(
            evt.ProductId,
            evt.ProductName,
            evt.ProductCategory,
            "SupplierEscalation",
            "Escalada de Fornecedor",
            classification.Severity,
            totalDeviation,
            message,
            evt.LastPrice,
            evt.AveragePrice,
            evt.AnalyzedAt);
    }

    private async Task<Alert?> EvaluateSupplierConcentrationRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.SupplierId) ||
            evt.SupplierCategoryShare <= 0)
        {
            return null;
        }

        if (evt.SupplierCategoryShare < rule.Threshold) return null;

        var classification = await _ruleService.ClassifyDeviationAsync(
            evt.SupplierCategoryShare,
            ct);

        var message = $"Concentração de fornecedor: " +
            $"{evt.SupplierId} detém {evt.SupplierCategoryShare:F1}% " +
            $"das compras na categoria {evt.ProductCategory}";

        return Alert.Create(
            evt.ProductId,
            evt.ProductName,
            evt.ProductCategory,
            "SupplierConcentration",
            "Concentração de Fornecedor",
            classification.Severity,
            evt.SupplierCategoryShare,
            message,
            evt.LastPrice,
            evt.AveragePrice,
            evt.AnalyzedAt);
    }

    private async Task<Alert?> EvaluateInvalidApportionmentRuleAsync(
        AlertRule rule,
        PriceAnalysisCompletedEvent evt,
        CancellationToken ct)
    {
        if (evt.CostCenterShares == null ||
            evt.CostCenterShares.Count == 0)
        {
            return null;
        }

        var totalShare = evt.CostCenterShares.Values.Sum();
        var deviation = Math.Abs(totalShare - 100m);

        // Allow 2% tolerance
        if (deviation <= 2m) return null;

        var classification = await _ruleService.ClassifyDeviationAsync(
            deviation,
            ct);

        var message = $"Rateio inválido: " +
            $"soma dos rateios é {totalShare:F1}% (tolerância: 2%)";

        return Alert.Create(
            evt.ProductId,
            evt.ProductName,
            evt.ProductCategory,
            "InvalidApportionment",
            "Rateio Inválido",
            AlertSeverity.Critical,
            deviation,
            message,
            evt.LastPrice,
            evt.AveragePrice,
            evt.AnalyzedAt);
    }

    private AlertTriggeredEvent CreateAlertEvent(
        Alert alert,
        PriceAnalysisCompletedEvent analysisEvent)
    {
        return new AlertTriggeredEvent
        {
            AlertId = alert.Id,
            ProductId = alert.ProductId,
            ProductName = alert.ProductName,
            Category = alert.Category,
            AlertType = alert.Type,
            AlertCategory = alert.AlertCategory,
            Severity = alert.Severity.ToString(),
            DeviationPercentage = alert.DeviationPercentage,
            Message = alert.Message,
            CurrentPrice = alert.CurrentPrice,
            AveragePrice = alert.AveragePrice,
            OccurredAt = analysisEvent.AnalyzedAt,
            GeneratedAt = alert.CreatedAt,
            Source = "AlertService"
        };
    }

    // Default implementations for interface methods we don't fully implement yet
    public Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct) =>
        Task.FromResult(Enumerable.Empty<AlertRule>());

    public Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult<Alert?>(null);

    public Task<IEnumerable<Alert>> GetAlertsByProductAsync(
        string productId,
        DateTime from,
        DateTime to,
        CancellationToken ct) =>
        Task.FromResult(Enumerable.Empty<Alert>());

    public Task UpdateAlertRuleThresholdAsync(
        Guid ruleId,
        decimal newThreshold,
        CancellationToken ct) =>
        Task.CompletedTask;
}
