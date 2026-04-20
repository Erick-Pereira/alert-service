using Simcag.Shared.Common;

namespace Simcag.AlertService.Domain.Entities;

public class Alert : BaseEntity
{
    public string ProductId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // DROP, RISE, TREND
    public string Message { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Alert() { } // EF Core

    public static Alert Create(string productId, string type, string message)
    {
        return new Alert
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Type = type,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };
    }
}
