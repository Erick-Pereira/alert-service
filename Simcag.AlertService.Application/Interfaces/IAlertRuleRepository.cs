using Simcag.AlertService.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Repositório para acesso a regras de alerta
/// </summary>
public interface IAlertRuleRepository
{
    Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct);
    Task<AlertRule?> GetByIdAsync(Guid ruleId, CancellationToken ct);
    Task AddAsync(AlertRule rule, CancellationToken ct);
    Task UpdateAsync(AlertRule rule, CancellationToken ct);
}
