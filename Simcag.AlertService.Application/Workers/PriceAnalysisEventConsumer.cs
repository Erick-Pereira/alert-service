using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.Shared.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Workers;

public class PriceAnalysisEventConsumer : BackgroundService
{
    private readonly IEventConsumer<PriceAnalysisCompletedEvent> _eventConsumer;
    private readonly AlertOrchestrator _orchestrator;
    private readonly ILogger<PriceAnalysisEventConsumer> _logger;

    public PriceAnalysisEventConsumer(
        IEventConsumer<PriceAnalysisCompletedEvent> eventConsumer,
        AlertOrchestrator orchestrator,
        ILogger<PriceAnalysisEventConsumer> logger)
    {
        _eventConsumer = eventConsumer;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting Price Analysis Event Consumer for alert evaluation");

        await foreach (var messageEnvelope in _eventConsumer
            .ReadMessagesAsync(stoppingToken))
        {
            try
            {
                await _orchestrator.OrchestrateAlertEvaluationAsync(
                    messageEnvelope.Data,
                    stoppingToken);

                await _eventConsumer.AcknowledgeMessageAsync(
                    messageEnvelope,
                    stoppingToken);

                _logger.LogInformation(
                    "Successfully processed PriceAnalysisCompletedEvent for " +
                    "{ProductId}",
                    messageEnvelope.Data.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process PriceAnalysisCompletedEvent for " +
                    "{ProductId}",
                    messageEnvelope.Data.ProductId);

                await _eventConsumer.RejectMessageAsync(
                    messageEnvelope,
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Price Analysis Event Consumer stopped");
    }
}
