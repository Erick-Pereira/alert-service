using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Simcag.AlertService.Api;

/// <summary>
/// Used when the host is started under the <c>Testing</c> environment to avoid a RabbitMQ dependency.
/// </summary>
internal sealed class NoOpEventBus : IEventBus
{
    public Task PublishAsync(AlertTriggeredDomainEvent domainEvent, CancellationToken ct) => Task.CompletedTask;
}
