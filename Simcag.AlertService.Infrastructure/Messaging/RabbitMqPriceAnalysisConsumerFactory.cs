using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Infrastructure.Messaging;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Messaging;

public interface IRabbitMqPriceAnalysisConsumerFactory
{
    IEventConsumer<PriceAnalysisCompletedEvent> CreateConsumer(
        IServiceProvider serviceProvider);
}

public class RabbitMqPriceAnalysisConsumerFactory : IRabbitMqPriceAnalysisConsumerFactory
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPriceAnalysisConsumerFactory> _logger;

    public RabbitMqPriceAnalysisConsumerFactory(
        RabbitMqOptions options,
        ILogger<RabbitMqPriceAnalysisConsumerFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    public IEventConsumer<PriceAnalysisCompletedEvent> CreateConsumer(
        IServiceProvider serviceProvider)
    {
        return new RabbitMqPriceAnalysisConsumer(
            _options,
            serviceProvider,
            _logger);
    }
}

public class RabbitMqPriceAnalysisConsumer : IEventConsumer<PriceAnalysisCompletedEvent>
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqPriceAnalysisConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string _queueName = string.Empty;
    private bool _isRunning;

    public RabbitMqPriceAnalysisConsumer(
        RabbitMqOptions options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqPriceAnalysisConsumer> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(ct);

        await _channel.ExchangeDeclareAsync(
            exchange: "price-monitoring-exchange",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        _queueName = await _channel.QueueDeclareAsync(
            queue: "alert-service-price-analysis-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx-price-monitoring" },
                { "x-message-ttl", 3600000 } // 1 hour TTL
            },
            cancellationToken: ct);

        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: "price-monitoring-exchange",
            routingKey: "price.analysis.completed",
            cancellationToken: ct);

        _isRunning = true;

        _logger.LogInformation(
            "RabbitMQ consumer started. Listening on queue {QueueName}",
            _queueName);
    }

    public async IAsyncEnumerable<MessageEnvelope<PriceAnalysisCompletedEvent>>
        ReadMessagesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        if (_channel == null) throw new InvalidOperationException("Channel not initialized");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var evt = JsonSerializer.Deserialize<PriceAnalysisCompletedEvent>(
                    message,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (evt != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<PriceAnalysisEventConsumer>();

                    await handler.HandleAsync(evt, ct);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, ct);

                    _logger.LogInformation(
                        "Processed PriceAnalysisCompletedEvent for {ProductId}",
                        evt.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message from queue {QueueName}",
                    _queueName);

                if (_channel?.IsOpen ?? false)
                {
                    await _channel.BasicNackAsync(
                        ea.DeliveryTag,
                        false,
                        requeue: false,
                        cancellationToken: ct);
                }
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        while (_isRunning && !ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
    }

    public Task AcknowledgeMessageAsync(
        MessageEnvelope<PriceAnalysisCompletedEvent> message,
        CancellationToken ct) =>
        Task.CompletedTask;

    public Task RejectMessageAsync(
        MessageEnvelope<PriceAnalysisCompletedEvent> message,
        CancellationToken ct) =>
        Task.CompletedTask;

    public async Task StopAsync(CancellationToken ct)
    {
        _isRunning = false;

        if (_channel?.IsOpen ?? false)
        {
            await _channel.CloseAsync(cancellationToken: ct);
            await _channel.DisposeAsync();
        }

        if (_connection?.IsOpen ?? false)
        {
            await _connection.CloseAsync(cancellationToken: ct);
            await _connection.DisposeAsync();
        }

        _logger.LogInformation("RabbitMQ consumer stopped");
    }
}
