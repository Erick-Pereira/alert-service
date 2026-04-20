
using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task<Alert?> GetLastByProductIdAsync(string productId, CancellationToken ct)
    {
        return await _context.Alerts
            .Where(a => a.ProductId == productId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
