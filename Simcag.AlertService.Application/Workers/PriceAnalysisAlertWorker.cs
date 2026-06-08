using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcag.AlertService.Application.Interfaces;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Contracts;
using Simcag.Shared.Messaging.Telemetry;

namespace Simcag.AlertService.Application.Workers;

/// <summary>
/// Consome <see cref="PriceAnalyzedEvent"/> (fila dedicada <c>alert-price-analyzed-events</c>) e executa o motor de regras de alerta.
/// </summary>
public sealed class PriceAnalysisAlertWorker : BackgroundService
{
    private readonly IEventConsumer<PriceAnalyzedEvent> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PriceAnalysisAlertWorker> _logger;

    public PriceAnalysisAlertWorker(
        IEventConsumer<PriceAnalyzedEvent> consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<PriceAnalysisAlertWorker> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceAnalysisAlertWorker: listening for {Event} on {Queue}", nameof(PriceAnalyzedEvent), EventNames.AlertPriceAnalyzed);

        try
        {
            await foreach (var envelope in _consumer.ReadMessagesAsync(stoppingToken))
            {
                using (MessagingConsumeTelemetry.BeginConsume(envelope, out _))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    try
                    {
                        var analysis = envelope.Data;
                        _logger.LogDebug("Evaluating alert rules for product {ProductId}", analysis.ProductId);
                        await alertService.EvaluateAlertAsync(analysis, stoppingToken);
                        await _consumer.AcknowledgeMessageAsync(envelope, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to evaluate alert for product {ProductId} (EventId {EventId})",
                            envelope.Data.ProductId,
                            envelope.Data.EventId);
                        await _consumer.RejectMessageAsync(envelope, stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutdown
        }
    }
}
