using System.Collections.Generic;

namespace Simcag.AlertService.Application.DTOs;

/// <summary>
/// Data Transfer Object para estatísticas de alertas
/// </summary>
public class AlertStatsDto
{
    public int Total { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
    public int UnreadCount { get; set; }
}
