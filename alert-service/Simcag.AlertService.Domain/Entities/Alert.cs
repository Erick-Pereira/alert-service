using System;

namespace Simcag.AlertService.Domain.Entities
{
    /// <summary>
    /// Entidade Alert que representa uma notificação de anomalia detectada
    /// </summary>
    public class Alert
    {
        public Guid Id { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal ActualPrice { get; private set; }
        public decimal? MarketPrice { get; private set; }
        public decimal DeviationPercentage { get; private set; }
        public AlertLevel Level { get; private set; }
        public string? Message { get; private set; }
        public DateTime DetectedAt { get; private set; }
        public DateTime? DismissedAt { get; private set; }
        public bool IsRead { get; private set; }
        public string? Source { get; private set; }

        // Constructor privado para pattern de factory
        private Alert() { }

        // Factory method
        public static Alert Create(
            string productId,
            string productName,
            decimal actualPrice,
            decimal? marketPrice = null,
            decimal deviationPercentage = 0m,
            AlertLevel level = AlertLevel.Normal,
            string? message = null,
            string? source = null)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new InvalidOperationException("ProductId é obrigatório");
            
            if (string.IsNullOrWhiteSpace(productName))
                throw new InvalidOperationException("ProductName é obrigatório");

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductName = productName,
                ActualPrice = actualPrice,
                MarketPrice = marketPrice,
                DeviationPercentage = deviationPercentage,
                Level = level,
                Message = message ?? $"Desvio de {deviationPercentage:P0} no produto {productName}",
                Source = source ?? "system",
                DetectedAt = DateTime.UtcNow,
                IsRead = false
            };

            alert.DetermineLevelIfMarketPriceExists();
            return alert;
        }

        private void DetermineLevelIfMarketPriceExists()
        {
            if (MarketPrice.HasValue)
            {
                if (DeviationPercentage > 30m)
                    Level = AlertLevel.Superfaturado;
                else if (DeviationPercentage > 15m)
                    Level = AlertLevel.Suspeito;
                else
                    Level = AlertLevel.Normal;
            }
            else
            {
                Level = AlertLevel.OutOfStock;
            }
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public void Dismiss()
        {
            DismissedAt = DateTime.UtcNow;
        }
    }
}