using Simcag.AlertService.Application.Interfaces;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Messaging;

public class RabbitMqAlertPublisher : IEventPublisher<AlertTriggeredEvent>
{
    private readonly IPublisher _rabbitMqPublisher;
    private readonly ILogger<RabbitMqAlertPublisher> _logger;

    public RabbitMqAlertPublisher(
        IPublisher rabbitMqPublisher,
        ILogger<RabbitMqAlertPublisher> logger)
    {
        _rabbitMqPublisher = rabbitMqPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(
        AlertTriggeredEvent message,
        CancellationToken ct)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);

            await _rabbitMqPublisher.PublishAsync(
                messageBody,
                EventNames.AlertTriggered,
                ct);

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
