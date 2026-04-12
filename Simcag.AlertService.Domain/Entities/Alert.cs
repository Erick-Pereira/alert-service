using System;
using System.Collections.Generic;
using System.Text;

namespace Simcag.AlertService.Domain.Entities;

public class Alert
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }

    public decimal OriginalPrice { get; private set; }
    public decimal MarketPrice { get; private set; }

    public decimal DeviationPercentage { get; private set; }

    public string AlertLevel { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsRead { get; private set; }

    private Alert() { } // EF Core

    public Alert(
        Guid productId,
        string productName,
        decimal originalPrice,
        decimal marketPrice,
        decimal deviationPercentage,
        string alertLevel)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        OriginalPrice = originalPrice;
        MarketPrice = marketPrice;
        DeviationPercentage = deviationPercentage;
        AlertLevel = alertLevel;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
