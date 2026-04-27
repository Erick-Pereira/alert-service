using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Events;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Messaging;

/// <summary>
/// Publicador de eventos de domínio via RabbitMQ
/// Converte eventos de domínio em eventos compartilhados e publica
/// </summary>
public sealed class RabbitMqEventBus : IEventBus
{
    private readonly IPublisher _publisher;
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(
        IPublisher publisher,
        ILogger<RabbitMqEventBus> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishAsync(AlertTriggeredDomainEvent domainEvent, CancellationToken ct)
    {
        // Mapear evento de domínio para evento compartilhado (contrato)
        var sharedEvent = new AlertTriggeredEvent
        {
            AlertId = domainEvent.AlertId.ToString(),
            ProductId = domainEvent.ProductId,
            ProductName = domainEvent.ProductName,
            SupplierId = domainEvent.SupplierId,
            Category = domainEvent.Category,
            AlertType = domainEvent.AlertType,
            AlertCategory = domainEvent.AlertCategory,
            Severity = domainEvent.Severity.ToString(),
            DeviationPercentage = domainEvent.DeviationPercentage,
            Message = domainEvent.Message,
            CurrentPrice = domainEvent.CurrentPrice,
            AveragePrice = domainEvent.AveragePrice,
            OccurredAt = domainEvent.OccurredAt,
            GeneratedAt = DateTime.UtcNow,
            Source = domainEvent.Source ?? "RabbitMqEventBus"
        };

        try
        {
            await _publisher.PublishAsync(sharedEvent, EventNames.AlertTriggered, ct);
            _logger.LogInformation(
                "Published AlertTriggeredDomainEvent {AlertId} to RabbitMQ",
                domainEvent.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish AlertTriggeredDomainEvent {AlertId}",
                domainEvent.AlertId);
            throw;
        }
    }
}
