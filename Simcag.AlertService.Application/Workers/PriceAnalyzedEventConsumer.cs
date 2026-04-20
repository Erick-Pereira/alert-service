using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Simcag.Shared.Messaging.Contracts;

using Simcag.AlertService.Application.Interfaces;
using Simcag.Shared.Events;

namespace Simcag.AlertService.Application.Workers;

public class PriceAnalyzedEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PriceAnalyzedEventConsumer> _logger;
    private readonly IEventConsumer<PriceAnalyzedEvent> _eventConsumer;

    public PriceAnalyzedEventConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<PriceAnalyzedEventConsumer> logger,
        IEventConsumer<PriceAnalyzedEvent> eventConsumer)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _eventConsumer = eventConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting PriceAnalyzedEvent consumer");

        await foreach (var messageEnvelope in _eventConsumer.ReadMessagesAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

            try
            {
                await alertService.EvaluateAsync(messageEnvelope.Data, stoppingToken);
                await _eventConsumer.AcknowledgeMessageAsync(messageEnvelope, stoppingToken);
                _logger.LogInformation("Successfully processed PriceAnalyzedEvent for product {ProductId}",
                    messageEnvelope.Data.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process PriceAnalyzedEvent for product {ProductId}",
                    messageEnvelope.Data.ProductId);
                await _eventConsumer.RejectMessageAsync(messageEnvelope, stoppingToken);
            }
        }

        _logger.LogInformation("PriceAnalyzedEvent consumer stopped");
    }
}