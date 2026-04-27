using System;
using Microsoft.AspNetCore.Mvc;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Api.Contracts.Requests;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleService _ruleService;
    private readonly IAlertRuleRepository _ruleRepository;
    private readonly IAlertService _alertService;
    private readonly ILogger<AlertRulesController> _logger;

    public AlertRulesController(
        IAlertRuleService ruleService,
        IAlertRuleRepository ruleRepository,
        IAlertService alertService,
        ILogger<AlertRulesController> logger)
    {
        _ruleService = ruleService;
        _ruleRepository = ruleRepository;
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as regras de alerta ativas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRules(CancellationToken ct)
    {
        var rules = await _ruleService.GetApplicableRulesAsync(string.Empty, null, ct);
        var ruleDtos = rules.Select(MapToDto);
        return Ok(ruleDtos);
    }

    /// <summary>
    /// Obtém uma regra pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRule(Guid id, CancellationToken ct)
    {
        var rule = await _ruleRepository.GetByIdAsync(id, ct);
        if (rule == null)
            return NotFound();

        return Ok(MapToDto(rule));
    }

    /// <summary>
    /// Atualiza o threshold de uma regra
    /// </summary>
    [HttpPut("{id}/threshold")]
    public async Task<IActionResult> UpdateThreshold(
        Guid id,
        [FromBody] UpdateAlertRuleThresholdRequest request,
        CancellationToken ct)
    {
        if (request.NewThreshold <= 0)
            return BadRequest(new { Message = "Threshold deve ser positivo" });

        try
        {
            await _alertService.UpdateAlertRuleThresholdAsync(id, request.NewThreshold, ct);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        _logger.LogInformation("Rule {RuleId} threshold persisted as {Threshold}", id, request.NewThreshold);

        return Ok(new { Message = "Threshold atualizado com sucesso", RuleId = id, NewThreshold = request.NewThreshold });
    }

    private static AlertRuleDto MapToDto(AlertRule rule) => new()
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
