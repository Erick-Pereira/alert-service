using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Events;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Contracts;
using Microsoft.Extensions.Logging;

namespace Simcag.AlertService.Infrastructure.Messaging;

/// <summary>
/// Publicador de eventos de domínio via RabbitMQ
/// Converte eventos de domínio em eventos compartilhados e publica
/// </summary>
public sealed class RabbitMqEventBus : IEventBus
{
    private readonly Simcag.Shared.Messaging.Contracts.IEventPublisher<AlertTriggeredEvent> _publisher;
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(
        Simcag.Shared.Messaging.Contracts.IEventPublisher<AlertTriggeredEvent> publisher,
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
            Source = domainEvent.Source ?? "RabbitMqEventBus",
            UserId = domainEvent.NotifyUserId,
            TenantId = domainEvent.TenantId
        };

        try
        {
            // Exchange events + routing key AlertTriggeredEvent (see RabbitMqEventConsumer / RabbitMqQueueInitializer).
            await _publisher.PublishAsync(sharedEvent, cancellationToken: ct);
            _logger.LogInformation(
                "Published AlertTriggeredEvent {AlertId} to RabbitMQ (exchange=events, routingKey={RoutingKey})",
                domainEvent.AlertId,
                nameof(AlertTriggeredEvent));
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
