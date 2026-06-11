using Simcag.AlertService.Domain.Enums;
using Simcag.Shared.Events;

namespace Simcag.AlertService.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um alerta é criado/avaliado
/// </summary>
public sealed class AlertTriggeredDomainEvent : BaseEvent
{
    public override string EventType => "alert.triggered.domain";
    
    public Guid AlertId { get; }
    public string ProductId { get; }
    public string ProductName { get; }
    public string Category { get; }
    public string? SupplierId { get; }
    public AlertSeverity Severity { get; }
    public decimal DeviationPercentage { get; }
    public string AlertType { get; }
    public string AlertCategory { get; }
    public string Message { get; }
    public decimal CurrentPrice { get; }
    public decimal AveragePrice { get; }
    public DateTime OccurredAt { get; }
    public string? Source { get; }
    public Guid? NotifyUserId { get; }
    public Guid? TenantId { get; }
    public Guid? ExpenseId { get; }

    public AlertTriggeredDomainEvent(
        Guid alertId,
        string productId,
        string productName,
        string category,
        string? supplierId,
        AlertSeverity severity,
        decimal deviationPercentage,
        string alertType,
        string alertCategory,
        string message,
        decimal currentPrice,
        decimal averagePrice,
        DateTime occurredAt,
        string? source = null,
        Guid? notifyUserId = null,
        Guid? tenantId = null,
        Guid? expenseId = null)
    {
        AlertId = alertId;
        ProductId = productId;
        ProductName = productName;
        Category = category;
        SupplierId = supplierId;
        Severity = severity;
        DeviationPercentage = deviationPercentage;
        AlertType = alertType;
        AlertCategory = alertCategory;
        Message = message;
        CurrentPrice = currentPrice;
        AveragePrice = averagePrice;
        OccurredAt = occurredAt;
        Source = source ?? "AlertEvaluationService";
        NotifyUserId = notifyUserId is { } u && u != Guid.Empty ? u : null;
        TenantId = tenantId is { } t && t != Guid.Empty ? t : null;
        ExpenseId = expenseId is { } e && e != Guid.Empty ? e : null;
    }
}
