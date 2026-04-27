using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class AlertClassificationServiceTests
{
    [Fact]
    public async Task GetApplicableRulesAsync_filters_by_supplier_when_both_match()
    {
        var globalRule = AlertRule.Create("G", "", "c1", null, AlertType.OverpriceMarket, 15m);
        var forSupplierA = AlertRule.Create("A", "", "c1", "SUP-A", AlertType.OverpriceHistorical, 12m);

        var repo = new Mock<IAlertRuleRepository>();
        repo.Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { globalRule, forSupplierA });

        var svc = new AlertClassificationService(repo.Object, NullLogger<AlertClassificationService>.Instance);

        var forSupA = (await svc.GetApplicableRulesAsync("c1", "SUP-A", default)).ToList();
        forSupA.Should().HaveCount(2);

        var forSupB = (await svc.GetApplicableRulesAsync("c1", "SUP-B", default)).ToList();
        forSupB.Should().HaveCount(1);
        forSupB[0].Should().Be(globalRule);
    }
}
