using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.UseCases.EvaluateAlert;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Services;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class EvaluateAlertHandlerPipelineTests
{
    [Fact]
    public async Task EvaluateAlertAsync_with_no_applicable_rules_does_not_persist()
    {
        var rules = new Mock<IAlertRuleService>();
        rules.Setup(s => s.GetApplicableRulesAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AlertRule>());

        var alerts = new Mock<IAlertRepository>();

        var handler = new EvaluateAlertHandler(
            rules.Object,
            new Mock<IAlertRuleRepository>().Object,
            new Mock<ICacheService>().Object,
            new Mock<IEventBus>().Object,
            alerts.Object,
            NullLogger<EvaluateAlertHandler>.Instance,
            Array.Empty<IAlertEvaluationStrategy>());

        var evt = new PriceAnalyzedEvent
        {
            ProductId = "P1",
            ProductName = "N",
            Category = "C",
            AnalysisDate = DateTime.UtcNow
        };

        var result = await handler.EvaluateAlertAsync(evt, default);

        result.Should().BeNull();
        alerts.Verify(
            a => a.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
