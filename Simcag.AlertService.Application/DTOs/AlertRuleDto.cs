using System;

namespace Simcag.AlertService.Application.DTOs;

/// <summary>
/// Data Transfer Object para representação de regra de alerta
/// </summary>
public class AlertRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public bool IsEnabled { get; set; }
    public int MinDataPoints { get; set; }
    public string DefaultSeverity { get; set; } = string.Empty;
}
