using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.UseCases.EvaluateAlert;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Services;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class EvaluateAlertHandlerReadTests
{
    private static EvaluateAlertHandler CreateHandler(
        IAlertRuleService? ruleService = null,
        IAlertRuleRepository? rules = null,
        ICacheService? cache = null,
        IEventBus? bus = null,
        IAlertRepository? alerts = null,
        IAlertEvaluationStrategy[]? strategies = null)
    {
        return new EvaluateAlertHandler(
            ruleService ?? Mock.Of<IAlertRuleService>(),
            rules ?? Mock.Of<IAlertRuleRepository>(),
            cache ?? Mock.Of<ICacheService>(),
            bus ?? Mock.Of<IEventBus>(),
            alerts ?? Mock.Of<IAlertRepository>(),
            NullLogger<EvaluateAlertHandler>.Instance,
            strategies ?? Array.Empty<IAlertEvaluationStrategy>());
    }

    [Fact]
    public async Task GetByIdAsync_uses_IAlertRepository()
    {
        var id = Guid.NewGuid();
        var ar = new Mock<IAlertRepository>();
        ar.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);
        var handler = CreateHandler(alerts: ar.Object);

        await handler.GetByIdAsync(id, CancellationToken.None);

        ar.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveRulesAsync_uses_IAlertRuleRepository()
    {
        var list = new List<AlertRule>();
        var r = new Mock<IAlertRuleRepository>();
        r.Setup(x => x.GetActiveRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        var handler = CreateHandler(rules: r.Object);

        var result = await handler.GetActiveRulesAsync(CancellationToken.None);

        r.Verify(x => x.GetActiveRulesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAlertRuleThresholdAsync_throws_when_rule_missing()
    {
        var rr = new Mock<IAlertRuleRepository>();
        rr.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertRule?)null);
        var handler = CreateHandler(rules: rr.Object);

        var act = () => handler.UpdateAlertRuleThresholdAsync(Guid.NewGuid(), 10, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
