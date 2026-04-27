using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Infrastructure.Persistence.Repositories;

public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly AlertDbContext _context;

    public AlertRuleRepository(AlertDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct)
    {
        return await _context.AlertRules
            .Where(r => r.IsEnabled)
            .ToListAsync(ct);
    }

    public async Task<AlertRule?> GetByIdAsync(Guid ruleId, CancellationToken ct)
    {
        return await _context.AlertRules
            .FirstOrDefaultAsync(r => r.Id == ruleId, ct);
    }

    public async Task AddAsync(AlertRule rule, CancellationToken ct)
    {
        await _context.AlertRules.AddAsync(rule, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AlertRule rule, CancellationToken ct)
    {
        _context.AlertRules.Update(rule);
        await _context.SaveChangesAsync(ct);
    }
}
