using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;

using Microsoft.Extensions.Logging;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging.Contracts;

namespace Simcag.AlertService.Application.Services;

public class AlertEvaluationService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AlertEvaluationService> _logger;
    private readonly IEventPublisher<AlertCreatedEvent>? _eventPublisher;

    public AlertEvaluationService(
        IAlertRepository alertRepository,
        ILogger<AlertEvaluationService> _logger,
        IEventPublisher<AlertCreatedEvent>? eventPublisher = null)
    {
        _alertRepository = alertRepository;
        this._logger = _logger;
        this._eventPublisher = eventPublisher;
    }

    public async Task EvaluateAsync(PriceAnalyzedEvent input, CancellationToken ct)
    {
        using var scope = _logger.BeginScope("{ProductId} {EventId}", input.ProductId, input.EventId);

        _logger.LogInformation("Evaluating alerts for product {ProductId}", input.ProductId);

        // Check idempotence: no alert for same product within 24h
        var lastAlert = await _alertRepository.GetLastByProductIdAsync(input.ProductId, ct);
        if (lastAlert != null && lastAlert.CreatedAt > DateTime.UtcNow.AddHours(-24))
        {
            _logger.LogInformation("Alert already generated for product {ProductId} within 24h, skipping", input.ProductId);
            return;
        }

        // Evaluate rules
        var alertType = EvaluateAlertType(input);
        if (alertType == null)
        {
            _logger.LogInformation("No alert conditions met for product {ProductId}", input.ProductId);
            return;
        }

        var message = GenerateAlertMessage(alertType, input);

        var alert = Alert.Create(input.ProductId, alertType, message);

        await _alertRepository.AddAsync(alert, ct);

        _logger.LogInformation("Alert {Type} generated for product {ProductId}", alertType, input.ProductId);

// Publish AlertCreatedEvent for notification service
        if (_eventPublisher != null)
        {
            var alertEvent = new AlertCreatedEvent(
                alertId: alert.Id,
                productId: input.ProductId,
                productName: input.ProductId,
                alertType: alertType,
                message: message,
                currentPrice: input.LastPrice,
                priceVariation: input.PriceVariation,
                source: "PriceAnalysis",
                occurredAt: DateTime.UtcNow
            );

            await _eventPublisher.PublishAsync(alertEvent, ct);
            _logger.LogInformation("Published AlertCreatedEvent for product {ProductId}", input.ProductId);
        }
    }

    private string? EvaluateAlertType(PriceAnalyzedEvent input)
    {
        // Rule 1: PriceVariation <= -10% → DROP
        if (input.PriceVariation <= -10)
            return "DROP";

        // Rule 2: PriceVariation >= 10% → RISE
        if (input.PriceVariation >= 10)
            return "RISE";

        // Rule 3: Trend == "DOWN" → TREND
        if (input.Trend == "DOWN")
            return "TREND";

        return null;
    }

    private string GenerateAlertMessage(string alertType, PriceAnalyzedEvent input)
    {
        return alertType switch
        {
            "DROP" => $"Price drop detected: {input.PriceVariation:F2}% variation (Last: {input.LastPrice:C}, Avg: {input.AveragePrice:C})",
            "RISE" => $"Price increase detected: {input.PriceVariation:F2}% variation (Last: {input.LastPrice:C}, Avg: {input.AveragePrice:C})",
            "TREND" => $"Downward trend detected (Last: {input.LastPrice:C}, Avg: {input.AveragePrice:C})",
            _ => "Alert generated"
        };
    }
}