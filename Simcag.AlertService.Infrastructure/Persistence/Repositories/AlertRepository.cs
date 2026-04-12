using Microsoft.EntityFrameworkCore;
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

    public async Task AddAsync(Alert alert)
    {
        await _context.Alerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await _context.Alerts.FindAsync(id);
    }

    public async Task<IEnumerable<Alert>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetFilteredAsync(
        int page,
        int pageSize,
        string? level,
        bool? isRead)
    {
        var query = _context.Alerts.AsQueryable();

        if (!string.IsNullOrEmpty(level))
            query = query.Where(a => a.AlertLevel == level);

        if (isRead.HasValue)
            query = query.Where(a => a.IsRead == isRead.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(bool? isRead = null, string? level = null)
    {
        var query = _context.Alerts.AsQueryable();

        if (!string.IsNullOrEmpty(level))
            query = query.Where(a => a.AlertLevel == level);

        if (isRead.HasValue)
            query = query.Where(a => a.IsRead == isRead.Value);

        return await query.CountAsync();
    }

    public async Task UpdateAsync(Alert alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
    }
}