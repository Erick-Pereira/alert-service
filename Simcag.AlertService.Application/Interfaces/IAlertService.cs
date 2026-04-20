using Simcag.Shared.Events;

namespace Simcag.AlertService.Application.Interfaces;

public interface IAlertService
{
    Task EvaluateAsync(PriceAnalyzedEvent input, CancellationToken ct);
}