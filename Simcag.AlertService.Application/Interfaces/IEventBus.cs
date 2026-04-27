using Simcag.AlertService.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Application.Interfaces;

/// <summary>
/// Barramento de eventos para publicação de eventos de aplicação/domínio
/// </summary>
public interface IEventBus
{
    Task PublishAsync(AlertTriggeredDomainEvent domainEvent, CancellationToken ct);
}
