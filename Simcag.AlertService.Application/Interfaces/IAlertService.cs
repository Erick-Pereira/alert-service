using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.ValueObjects;

namespace Simcag.AlertService.Application.Interfaces;

public interface IAlertService
{
    Task<Alert?> EvaluateAlertAsync(
        PriceAnalysisCompletedEvent analysisEvent,
        CancellationToken ct);
    Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct);
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Alert>> GetAlertsByProductAsync(
        string productId,
        DateTime from,
        DateTime to,
        CancellationToken ct);
    Task UpdateAlertRuleThresholdAsync(
        Guid ruleId,
        decimal newThreshold,
        CancellationToken ct);
}

public interface IAlertRuleService
{
    Task<IEnumerable<AlertRule>> GetApplicableRulesAsync(
        string category,
        string? supplierId,
        CancellationToken ct);
    Task<DeviationPercentage> CalculateDeviationAsync(
        decimal currentValue,
        decimal baseline,
        CancellationToken ct);
    Task<AlertClassification> ClassifyDeviationAsync(
        decimal deviationPercentage,
        CancellationToken ct);
}

public interface IRedisCacheService
{
    Task<bool> IsDuplicateAlertAsync(
        string entityType,
        string entityId,
        TimeSpan cooldown,
        CancellationToken ct);
    Task MarkAlertAsSentAsync(
        string entityType,
        string entityId,
        TimeSpan ttl,
        CancellationToken ct);
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken ct) where T : class;
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken ct) where T : class;
}

public interface IEventPublisher<T> where T : class
{
    Task PublishAsync(T message, CancellationToken ct);
}

public interface IPriceAnalysisEventConsumer
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
}