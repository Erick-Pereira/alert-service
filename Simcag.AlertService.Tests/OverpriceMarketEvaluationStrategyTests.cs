using Simcag.AlertService.Application.EvaluationStrategies;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.ValueObjects;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class OverpriceMarketEvaluationStrategyTests
{
    private readonly OverpriceMarketEvaluationStrategy _strategy = new();

    [Fact]
    public void Evaluate_camera_890_vs_mercado_185_dispara_critical()
    {
        var rule = AlertRule.Create("OverpriceMarket", "dev", "Segurança", null, AlertType.OverpriceMarket, 15m);
        var evt = new PriceAnalyzedEvent
        {
            ProductId = "7587f4c1-d4a1-4564-9224-5a79778a5464:1",
            ProductName = "Camera IP Full HD 2MP",
            Category = "Segurança Eletrônica",
            LastPrice = 890m,
            MarketAverage = 185m,
            AnalysisDate = DateTime.UtcNow
        };

        var alert = _strategy.Evaluate(rule, evt);

        Assert.NotNull(alert);
        Assert.Equal(AlertSeverity.Critical, alert!.Severity);
        Assert.True(alert.DeviationPercentage > 100m);
        Assert.Contains("890", alert.Message, StringComparison.Ordinal);
        Assert.Contains("185", alert.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DeviationPercentage_aceita_desvio_acima_de_100()
    {
        var d = DeviationPercentage.Create(381.08m);
        Assert.Equal(381.08m, d.Value);
        Assert.True(d.IsAboveThreshold(15m));
    }
}
