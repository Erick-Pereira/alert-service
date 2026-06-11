using Microsoft.AspNetCore.Mvc;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepository;

    public AlertsController(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var pageData = await _alertRepository.GetPageAsync(page, pageSize, type, ct);

        var result = new PaginatedResult<AlertDto>
        {
            Items = pageData.Items.Select(MapToDto).ToList(),
            TotalCount = pageData.TotalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(pageData.TotalCount / (double)pageSize)
        };

        return Ok(ApiResponse<PaginatedResult<AlertDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlertById(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdAsync(id, ct);

        if (alert == null)
        {
            return NotFound(ApiResponse<string>.Fail($"Alert with ID {id} not found"));
        }

        var dto = MapToDto(alert);
        return Ok(ApiResponse<AlertDto>.Ok(dto));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.GetByIdForUpdateAsync(id, ct);
        if (alert is null)
            return NotFound(ApiResponse<string>.Fail($"Alert with ID {id} not found"));

        if (alert.Resolved)
            return Ok(ApiResponse<string>.Ok("Alert already read"));

        alert.MarkAsResolved();
        await _alertRepository.UpdateAsync(alert, ct);

        return Ok(ApiResponse<string>.Ok("Alert marked as read"));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var q = await _alertRepository.GetStatsAsync(ct);

        var stats = new AlertStatsDto
        {
            Total = q.Total,
            ByType = new Dictionary<string, int>(q.ByType),
            UnreadCount = q.UnreadCount
        };

        return Ok(ApiResponse<AlertStatsDto>.Ok(stats));
    }

    private static AlertDto MapToDto(Alert alert) =>
        new()
        {
            Id = alert.Id,
            ExpenseId = alert.ExpenseId,
            ProductId = alert.ProductId,
            ProductName = alert.ProductName,
            Category = alert.Category,
            AlertCategory = alert.AlertCategory,
            Type = alert.Type,
            Message = alert.Message,
            Severity = alert.Severity.ToString(),
            DeviationPercentage = alert.DeviationPercentage,
            CurrentPrice = alert.CurrentPrice,
            AveragePrice = alert.AveragePrice,
            AnalyzedAt = alert.AnalyzedAt,
            CreatedAt = alert.CreatedAt,
            Resolved = alert.Resolved
        };
}
