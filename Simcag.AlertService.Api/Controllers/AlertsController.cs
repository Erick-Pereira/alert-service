using Microsoft.AspNetCore.Mvc;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly IAlertRuleService _ruleService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(
        IAlertService alertService,
        IAlertRuleService ruleService,
        ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _ruleService = ruleService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém um alerta pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AlertDto>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var alert = await _alertService.GetByIdAsync(id, ct);

        if (alert == null)
        {
            return NotFound(new { Message = "Alerta não encontrado" });
        }

        return Ok(MapToDto(alert));
    }

    /// <summary>
    /// Lista alertas de um produto em um intervalo de datas
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetByProduct(
        string productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var alerts = await _alertService.GetAlertsByProductAsync(
            productId,
            startDate,
            endDate,
            ct);

        return Ok(alerts.Select(MapToDto));
    }

    /// <summary>
    /// Atualiza o threshold (limite) de uma regra de alerta
    /// </summary>
    [HttpPut("rules/{ruleId}/threshold")]
    public async Task<IActionResult> UpdateRuleThreshold(
        Guid ruleId,
        [FromBody] UpdateThresholdRequest request,
        CancellationToken ct)
    {
        if (request.NewThreshold <= 0)
        {
            return BadRequest(new
            {
                Message = "O threshold deve ser um valor positivo"
            });
        }

        try
        {
            await _alertService.UpdateAlertRuleThresholdAsync(
                ruleId,
                request.NewThreshold,
                ct);

            _logger.LogInformation(
                "Threshold da regra {RuleId} atualizado para {Threshold}",
                ruleId,
                request.NewThreshold);

            return Ok(new
            {
                Message = "Threshold atualizado com sucesso",
                RuleId = ruleId,
                NewThreshold = request.NewThreshold
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar threshold da regra {RuleId}",
                ruleId);

            return StatusCode(500, new
            {
                Message = "Erro interno ao atualizar threshold"
            });
        }
    }

    /// <summary>
    /// Retorna as regras de alerta ativas
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<IEnumerable<AlertRuleDto>>> GetActiveRules(
        CancellationToken ct)
    {
        var rules = await _ruleService.GetActiveRulesAsync(ct);
        return Ok(rules.Select(MapToRuleDto));
    }

    /// <summary>
    /// Health check do serviço de alertas
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "healthy",
            Service = "AlertService",
            Version = "1.0.0",
            Features = new[]
            {
                "price_deviation_detection",
                "supplier_escalation_detection",
                "supplier_concentration_detection",
                "invalid_apportionment_detection",
                "historical_deviation_detection"
            }
        });
    }

    private static AlertDto MapToDto(Alert alert) => new()
    {
        Id = alert.Id,
        ProductId = alert.ProductId,
        ProductName = alert.ProductName,
        Category = alert.Category,
        Type = alert.Type,
        AlertCategory = alert.AlertCategory,
        Severity = alert.Severity.ToString(),
        DeviationPercentage = alert.DeviationPercentage,
        Message = alert.Message,
        CurrentPrice = alert.CurrentPrice,
        AveragePrice = alert.AveragePrice,
        CreatedAt = alert.CreatedAt,
        AnalyzedAt = alert.AnalyzedAt,
        Resolved = alert.Resolved,
        ResolvedAt = alert.ResolvedAt
    };

    private static AlertRuleDto MapToRuleDto(AlertRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        Type = rule.Type.ToString(),
        Threshold = rule.Threshold,
        IsEnabled = rule.IsEnabled,
        MinDataPoints = rule.MinDataPoints,
        DefaultSeverity = rule.DefaultSeverity.ToString()
    };
}

public class AlertDto
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AlertCategory { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal DeviationPercentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

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

public class UpdateThresholdRequest
{
    public decimal NewThreshold { get; set; }
}
