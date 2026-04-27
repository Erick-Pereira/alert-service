using Simcag.AlertService.Api;
using Simcag.AlertService.Application.EvaluationStrategies;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Application.UseCases.EvaluateAlert;
using Simcag.AlertService.Application.Workers;
using Simcag.AlertService.Domain.Services;
using Simcag.AlertService.Infrastructure.Cache;
using Simcag.AlertService.Infrastructure.Messaging;
using Simcag.AlertService.Infrastructure.Persistence.Repositories;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging;
using Simcag.Shared.Messaging.Configuration;
using Simcag.Shared.Messaging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

static string? GetEnv(params string[] keys)
{
    foreach (var key in keys)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(value))
            return value;
    }
    return null;
}

static bool EfMigrationsOptOut() =>
    GetEnv("SKIP_EF_MIGRATIONS", "MIGRATIONS__SKIP") is { } s
    && (s is "1"
        || s.Equals("true", StringComparison.OrdinalIgnoreCase)
        || s.Equals("yes", StringComparison.OrdinalIgnoreCase)
        || s.Equals("on", StringComparison.OrdinalIgnoreCase));

// Database (.env: ConnectionStrings__DefaultConnection; sem appsettings para secretos)
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AlertDbContext>(options =>
        options.UseInMemoryDatabase("alert_testing"));
}
else
{
    var defaultConnection = GetEnv("ConnectionStrings__DefaultConnection", "CONNECTIONSTRINGS__DEFAULTCONNECTION")
        ?? throw new InvalidOperationException("Defina ConnectionStrings__DefaultConnection no .env (PostgreSQL).");
    builder.Services.AddDbContext<AlertDbContext>(options =>
        options.UseNpgsql(defaultConnection));
}

// Redis opcional: deduplicação de alertas; sem REDIS, usa memória in-process
var redisConnection = GetEnv("REDIS__CONNECTION", "REDIS_CONNECTION");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "AlertService:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// RabbitMQ (omitted in the Testing host so WebApplicationFactory does not require a broker)
if (!builder.Environment.IsEnvironment("Testing"))
{
    var rabbitMqOptions = new RabbitMqOptions
    {
        Host = GetEnv("RABBITMQ__HOST", "RABBITMQ_HOST") ?? "localhost",
        Port = int.Parse(GetEnv("RABBITMQ__PORT", "RABBITMQ_PORT") ?? "5672"),
        UserName = GetEnv("RABBITMQ__USERNAME", "RABBITMQ_USERNAME") ?? "guest",
        Password = GetEnv("RABBITMQ__PASSWORD", "RABBITMQ_PASSWORD") ?? "guest",
        VirtualHost = GetEnv("RABBITMQ__VIRTUALHOST", "RABBITMQ_VIRTUALHOST") ?? "/"
    };

    builder.Services.AddSingleton(rabbitMqOptions);
    builder.Services.AddRabbitMqMessaging(rabbitMqOptions);
    builder.Services.AddRabbitMqPublisher("alert-monitoring-exchange");
    var eventsExchange = EventBusConstants.GetEventsExchangeName();
    builder.Services.AddRabbitMqEventConsumer<PriceAnalysisCompletedEvent>(EventNames.PriceAnalysisCompleted, eventsExchange);
    builder.Services.AddHostedService<PriceAnalysisAlertWorker>();
    builder.Services.AddScoped<IEventBus, RabbitMqEventBus>();
}
else
{
    builder.Services.AddScoped<IEventBus, NoOpEventBus>();
}

// Infrastructure Services
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
builder.Services.AddScoped<ICacheService, RedisAlertDeduplicationCache>();

// Domain Evaluation Strategies (registered as Scoped)
builder.Services.AddScoped<IAlertEvaluationStrategy, OverpriceMarketEvaluationStrategy>();
builder.Services.AddScoped<IAlertEvaluationStrategy, HistoricalOverpriceEvaluationStrategy>();
builder.Services.AddScoped<IAlertEvaluationStrategy, SupplierEscalationEvaluationStrategy>();
builder.Services.AddScoped<IAlertEvaluationStrategy, SupplierConcentrationEvaluationStrategy>();
builder.Services.AddScoped<IAlertEvaluationStrategy, InvalidApportionmentEvaluationStrategy>();

// Application Services
builder.Services.AddScoped<IAlertRuleService, AlertClassificationService>();
builder.Services.AddScoped<IAlertService, EvaluateAlertHandler>();

// Controllers & API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment() && !EfMigrationsOptOut())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
        await db.Database.MigrateAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}
