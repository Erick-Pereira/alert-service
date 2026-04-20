using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Events;
using Simcag.Shared.Messaging.Contracts;
using Simcag.Shared.Messaging.Events;

namespace Simcag.AlertService.Domain.Events;

/// <summary>
/// Evento publicado quando um alerta é criado.
/// </summary>
public class AlertCreatedEvent : BaseEvent
{
    public string AlertId { get; }
    public string ProductId { get; }
    public string ProductName { get; }
    public string AlertType { get; } // DROP, RISE, TREND
    public string AlertCategory { get; }
    public AlertSeverity Severity { get; }
    public decimal DeviationPercentage { get; }
    public string Message { get; }
    public DateTime AnalyzedAt { get; }
    public string? Source { get; }

    private AlertCreatedEvent(
        Guid eventId,
        string alertId,
        string productId,
        string productName,
        string alertType,
        string alertCategory,
        AlertSeverity severity,
        decimal deviationPercentage,
        string message,
        DateTime analyzedAt,
        string? source)
        : base(eventId)
    {
        AlertId = alertId;
        ProductId = productId;
        ProductName = productName;
        AlertType = alertType;
        AlertCategory = alertCategory;
        Severity = severity;
        DeviationPercentage = deviationPercentage;
        Message = message;
        AnalyzedAt = analyzedAt;
        Source = source;
    }

    public static AlertCreatedEvent Create(
        string alertId,
        string productId,
        string productName,
        string alertType,
        string alertCategory,
        AlertSeverity severity,
        decimal deviationPercentage,
        string message,
        DateTime analyzedAt,
        string? source = null)
    {
        return new AlertCreatedEvent(
            eventId: Guid.NewGuid(),
            alertId: alertId,
            productId: productId,
            productName: productName,
            alertType: alertType,
            alertCategory: alertCategory,
            severity: severity,
            deviationPercentage: deviationPercentage,
            message: message,
            analyzedAt: analyzedAt,
            source: source);
    }
}