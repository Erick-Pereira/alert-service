using Simcag.AlertService.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken ct);
    Task<Alert?> GetLastByProductIdAsync(string productId, CancellationToken ct);
}

