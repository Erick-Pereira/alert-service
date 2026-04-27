using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Application.DTOs;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;

namespace Simcag.AlertService.Infrastructure.Persistence.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AlertDbContext _context;

    public AlertRepository(AlertDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Alert alert, CancellationToken ct)
    {
        await _context.Alerts.AddAsync(alert, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _context.Alerts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Alert?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
        await _context.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<AlertListQueryResult> GetPageAsync(
        int page, int pageSize, string? type, CancellationToken ct)
    {
        IQueryable<Alert> query = _context.Alerts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim();
            query = query.Where(a => a.Type.ToLower() == t.ToLower());
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new AlertListQueryResult(items, total);
    }

    public async Task<AlertStatsQueryResult> GetStatsAsync(CancellationToken ct)
    {
        var total = await _context.Alerts.CountAsync(ct);
        var rows = await _context.Alerts
            .AsNoTracking()
            .GroupBy(a => a.Type)
            .Select(g => new { g.Key, Cnt = g.Count() })
            .ToListAsync(ct);
        var byType = rows.ToDictionary(x => x.Key, x => x.Cnt);
        var unread = await _context.Alerts.CountAsync(a => !a.Resolved, ct);
        return new AlertStatsQueryResult(total, byType, unread);
    }

    public async Task UpdateAsync(Alert alert, CancellationToken ct) =>
        await _context.SaveChangesAsync(ct);

    public async Task<Alert?> GetLastByProductIdAsync(string productId, CancellationToken ct)
    {
        return await _context.Alerts
            .Where(a => a.ProductId == productId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Alert>> GetByProductAsync(
        string productId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        return await _context.Alerts
            .Where(a => a.ProductId == productId &&
                        a.CreatedAt >= from &&
                        a.CreatedAt <= to)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}