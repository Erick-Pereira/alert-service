using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.UseCases;

public class PublishAlertUseCase
{
    private readonly IEventPublisher<AlertTriggeredEvent> _eventPublisher;
    private readonly ILogger<PublishAlertUseCase> _logger;

    public PublishAlertUseCase(
        IEventPublisher<AlertTriggeredEvent> eventPublisher,
        ILogger<PublishAlertUseCase> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(
        Alert alert,
        PriceAnalysisCompletedEvent sourceEvent,
        CancellationToken ct)
    {
        var alertEvent = new AlertTriggeredEvent
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
            OccurredAt = sourceEvent.AnalyzedAt,
            GeneratedAt = alert.CreatedAt,
            Source = "AlertService"
        };

        await _eventPublisher.PublishAsync(alertEvent, ct);

        _logger.LogInformation(
            "Published AlertTriggeredEvent: {AlertId} (severity: {Severity})",
            alert.Id,
            alert.Severity);
    }
}
