using System;

namespace Simcag.AlertService.Application.DTOs;

/// <summary>
/// Data Transfer Object para representação de alerta
/// </summary>
public class AlertDto
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Resolved { get; set; }
}
