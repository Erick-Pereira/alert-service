using Simcag.AlertService.Application.Interfaces;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Messaging;

public class RabbitMqAlertPublisher : IEventPublisher<AlertTriggeredEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RabbitMqAlertPublisher> _logger;

    public RabbitMqAlertPublisher(
        IEventPublisher eventPublisher,
        ILogger<RabbitMqAlertPublisher> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(
        AlertTriggeredEvent message,
        CancellationToken ct)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);

            await _eventPublisher.PublishAsync(
                "alert-monitoring-exchange",
                "alert.triggered",
                messageBody,
                new Dictionary<string, object>
                {
                    { "AlertId", message.AlertId },
                    { "Severity", message.Severity },
                    { "ProductId", message.ProductId }
                });

            _logger.LogInformation(
                "Published AlertTriggeredEvent {AlertId} to RabbitMQ",
                message.AlertId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish AlertTriggeredEvent {AlertId}",
                message.AlertId);
            throw;
        }
    }
}
