using Simcag.Shared.Events;
using System;

namespace Simcag.AlertService.Domain.Events
{
    /// <summary>
    /// Evento publicado quando um alerta é disparado
    /// </summary>
    public class AlertTriggeredEvent : BaseEvent
    {
        public Guid AlertId { get; init; }
        public string ProductId { get; init; }
        public string ProductName { get; init; }
        public decimal ActualPrice { get; init; }
        public decimal? MarketPrice { get; init; }
        public decimal DeviationPercentage { get; init; }
        public AlertLevel Level { get; init; }
        public string Message { get; init; }
        public string? Source { get; init; }

        public override string EventType => "alert.triggered";

        public AlertTriggeredEvent(
            Guid eventId,
            Guid alertId,
            string productId,
            string productName,
            decimal actualPrice,
            decimal? marketPrice = null,
            decimal deviationPercentage = 0m,
            AlertLevel level = AlertLevel.Normal,
            string message = null,
            string? source = null)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("EventId não pode ser vazio", nameof(eventId));

            EventId = eventId;
            AlertId = alertId;
            ProductId = productId;
            ProductName = productName;
            ActualPrice = actualPrice;
            MarketPrice = marketPrice;
            DeviationPercentage = deviationPercentage;
            Level = level;
            Message = message ?? $"Alerta de nível {level} para {ProductName}";
            Source = source ?? "alert-service";
        }
    }
}