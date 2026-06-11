using System;
using System.Collections.Generic;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Events;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.UseCases.EvaluateAlert;

/// <summary>
/// Handler principal para avaliação de alertas (CASO DE USO)
/// Responsável por orquestrar o fluxo completo:
/// 1. Buscar regras
/// 2. Avaliar cada regra usando estratégias
/// 3. Verificar duplicação
/// 4. Persistir alerta (se necessário)
/// 5. Publicar evento
/// </summary>
public sealed class EvaluateAlertHandler : IAlertService
{
    private readonly IAlertRuleService _ruleService;
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<EvaluateAlertHandler> _logger;
    private readonly IAlertEvaluationStrategy[] _strategies;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromHours(24);

    public EvaluateAlertHandler(
        IAlertRuleService ruleService,
        IAlertRuleRepository alertRuleRepository,
        ICacheService cacheService,
        IEventBus eventBus,
        IAlertRepository alertRepository,
        ILogger<EvaluateAlertHandler> logger,
        IEnumerable<IAlertEvaluationStrategy> strategies)
    {
        _ruleService = ruleService;
        _alertRuleRepository = alertRuleRepository;
        _cacheService = cacheService;
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _logger = logger;
        _strategies = strategies.ToArray();
    }

    public async Task<Alert?> EvaluateAlertAsync(
        PriceAnalyzedEvent analysisEvent,
        CancellationToken ct)
    {
        using var scope = _logger.BeginScope(
            "Evaluating alerts for product {ProductId}",
            analysisEvent.ProductId);

        try
        {
            // 1. Buscar regras aplicáveis
            var rules = await _ruleService.GetApplicableRulesAsync(
                analysisEvent.Category,
                analysisEvent.SupplierId,
                ct);

            _logger.LogInformation(
                "Found {RuleCount} applicable rules for product {ProductId}",
                rules.Count(),
                analysisEvent.ProductId);

            Alert? mostSevereAlert = null;

            // 2. Avaliar cada regra usando Strategy Pattern
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

            // 3. Se encontrou alerta, aplicar deduplicação e persistir
            if (mostSevereAlert != null)
            {
                // Verificar duplicação
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
                    return mostSevereAlert;
                }

                // Persistir alerta
                await _alertRepository.AddAsync(mostSevereAlert, ct);

                // Marcar como enviado no cache
                await _cacheService.MarkAlertAsSentAsync(
                    "Product",
                    mostSevereAlert.ProductId,
                    AlertCooldown,
                    ct);

                // 4. Publicar evento
                Guid? tenantId = null;
                if (!string.IsNullOrWhiteSpace(analysisEvent.TenantId)
                    && Guid.TryParse(analysisEvent.TenantId, out var parsedTenant))
                {
                    tenantId = parsedTenant;
                }

                var domainEvent = new AlertTriggeredDomainEvent(
                    mostSevereAlert.Id,
                    mostSevereAlert.ProductId,
                    mostSevereAlert.ProductName,
                    mostSevereAlert.Category,
                    analysisEvent.SupplierId,
                    mostSevereAlert.Severity,
                    mostSevereAlert.DeviationPercentage,
                    mostSevereAlert.Type,
                    mostSevereAlert.AlertCategory,
                    mostSevereAlert.Message,
                    mostSevereAlert.CurrentPrice,
                    mostSevereAlert.AveragePrice,
                    analysisEvent.AnalysisDate,
                    "AlertEvaluationService",
                    analysisEvent.NotifyUserId,
                    tenantId,
                    mostSevereAlert.ExpenseId ?? analysisEvent.ExpenseId);

                await _eventBus.PublishAsync(domainEvent, ct);

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
        PriceAnalyzedEvent evt,
        CancellationToken ct)
    {
        try
        {
            var strategy = _strategies.FirstOrDefault(s => s.SupportedType == rule.Type)
                ?? throw new NotSupportedException($"No strategy found for rule type {rule.Type}");
            return await strategy.EvaluateAsync(rule, evt, ct);
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

    // Implementações default dos demais métodos de IAlertService
    public Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct) =>
        _alertRuleRepository.GetActiveRulesAsync(ct);

    public Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _alertRepository.GetByIdAsync(id, ct);

    public Task<IEnumerable<Alert>> GetAlertsByProductAsync(
        string productId, DateTime from, DateTime to, CancellationToken ct) =>
        _alertRepository.GetByProductAsync(productId, from, to, ct);

    public async Task UpdateAlertRuleThresholdAsync(
        Guid ruleId, decimal newThreshold, CancellationToken ct)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, ct);
        if (rule is null)
            throw new KeyNotFoundException($"Alert rule '{ruleId}' not found");

        rule.UpdateThreshold(newThreshold);
        await _alertRuleRepository.UpdateAsync(rule, ct);

        _logger.LogInformation("Rule {RuleId} threshold updated to {Threshold}", ruleId, newThreshold);
    }
}