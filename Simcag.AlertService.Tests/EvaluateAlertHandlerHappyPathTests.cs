using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.UseCases.EvaluateAlert;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Domain.Events;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class EvaluateAlertHandlerHappyPathTests
{
    private sealed class FixedOverpriceStrategy : IAlertEvaluationStrategy
    {
        public AlertType SupportedType => AlertType.OverpriceMarket;

        public Task<Alert?> EvaluateAsync(AlertRule rule, PriceAnalysisCompletedEvent evt, CancellationToken ct) =>
            Task.FromResult<Alert?>(Alert.Create(
                evt.ProductId, evt.ProductName, evt.Category,
                "OverpriceMarket", "Superfaturamento",
                AlertSeverity.High, 20m, "over threshold",
                evt.LastPrice, evt.AveragePrice, evt.AnalyzedAt));
    }

    [Fact]
    public async Task EvaluateAlertAsync_persists_and_publishes_when_rule_fires()
    {
        var rule = AlertRule.Create(
            "R1", "d", "cat", null, AlertType.OverpriceMarket, 5m);

        var rules = new Mock<IAlertRuleService>();
        rules.Setup(s => s.GetApplicableRulesAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { rule });

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.IsDuplicateAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var bus = new Mock<IEventBus>();

        var alerts = new Mock<IAlertRepository>();
        var strategies = new IAlertEvaluationStrategy[] { new FixedOverpriceStrategy() };

        var handler = new EvaluateAlertHandler(
            rules.Object,
            new Mock<IAlertRuleRepository>().Object,
            cache.Object,
            bus.Object,
            alerts.Object,
            NullLogger<EvaluateAlertHandler>.Instance,
            strategies);

        var evt = new PriceAnalysisCompletedEvent
        {
            ProductId = "P1",
            ProductName = "Prod",
            Category = "C1",
            LastPrice = 120m,
            MarketPrice = 100m,
            AveragePrice = 100m,
            AnalyzedAt = DateTime.UtcNow
        };

        var result = await handler.EvaluateAlertAsync(evt, default);

        result.Should().NotBeNull();
        alerts.Verify(
            a => a.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()),
            Times.Once);
        bus.Verify(
            b => b.PublishAsync(It.IsAny<AlertTriggeredDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        cache.Verify(
            c => c.MarkAlertAsSentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
