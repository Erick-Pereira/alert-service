using System;

namespace Simcag.AlertService.Application.DTOs;

/// <summary>
/// Data Transfer Object para representação de alerta
/// </summary>
public class AlertDto
{
    public Guid Id { get; set; }
    public Guid? ExpenseId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal DeviationPercentage { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Resolved { get; set; }
}
