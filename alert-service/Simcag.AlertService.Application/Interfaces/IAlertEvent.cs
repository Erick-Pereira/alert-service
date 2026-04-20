using Simcag.Shared.Events;
using System;

namespace Simcag.AlertService.Application.Interfaces
{
    /// <summary>
    /// Evento recebido do Processing/Analysis Service para avaliação de alertas
    /// </summary>
    public class PriceAnalyzedEvent : BaseEvent
    {
        public string ProductId { get; init; }
        public decimal AveragePrice { get; init; }
        public decimal LastPrice { get; init; }
        public decimal PriceVariation { get; init; }
        public string Trend { get; init; }
        public string? MarketName { get; init; }
        public DateTime AnalyzedAt { get; init; }

        public override string EventType => "price.analyzed";

        public PriceAnalyzedEvent(
            Guid eventId,
            string productId,
            decimal averagePrice,
            decimal lastPrice,
            decimal priceVariation,
            string trend,
            string? marketName = null,
            DateTime analyzedAt = default)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("EventId não pode ser vazio", nameof(eventId));

            EventId = eventId;
            ProductId = productId;
            AveragePrice = averagePrice;
            LastPrice = lastPrice;
            PriceVariation = priceVariation;
            Trend = trend;
            MarketName = marketName ?? string.Empty;
            AnalyzedAt = analyzedAt;
        }
    }
}