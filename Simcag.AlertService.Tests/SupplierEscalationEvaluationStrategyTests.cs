using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Simcag.AlertService.Application.EvaluationStrategies;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class SupplierEscalationEvaluationStrategyTests
{
    private static readonly AlertRule Rule = AlertRule.Create(
        "Escalation", "d", "cat", null, AlertType.SupplierEscalation, 5m);

    private static PriceAnalyzedEvent CreateEvent(
        IReadOnlyList<decimal> history,
        string? supplierId = "sup-1")
    {
        return new PriceAnalyzedEvent
        {
            ProductId = "P1",
            ProductName = "P",
            Category = "cat",
            SupplierId = supplierId ?? string.Empty,
            LastPrice = history.Count > 0 ? history[^1] : 0m,
            HistoricalAverage = 100m,
            PriceHistory = new List<decimal>(history),
            AnalysisDate = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Returns_null_when_no_supplier()
    {
        var s = new SupplierEscalationEvaluationStrategy();
        var evt = CreateEvent(new List<decimal> { 100, 110, 120, 130 }, supplierId: null);

        var r = await s.EvaluateAsync(Rule, evt, default);

        r.Should().BeNull();
    }

    [Fact]
    public async Task Returns_null_when_history_too_short_for_three_consecutive_increases()
    {
        var s = new SupplierEscalationEvaluationStrategy();
        var evt = CreateEvent(new List<decimal> { 100, 110, 120 });

        var r = await s.EvaluateAsync(Rule, evt, default);

        r.Should().BeNull();
    }

    [Fact]
    public async Task Returns_null_when_increases_are_not_consecutive()
    {
        var s = new SupplierEscalationEvaluationStrategy();
        // Subidas isoladas, nunca 3 de seguida
        var evt = CreateEvent(new List<decimal> { 100, 110, 100, 110, 120, 100, 105 });

        var r = await s.EvaluateAsync(Rule, evt, default);

        r.Should().BeNull();
    }

    [Fact]
    public async Task Fires_when_three_consecutive_increases_and_total_deviation_above_threshold()
    {
        var s = new SupplierEscalationEvaluationStrategy();
        var evt = CreateEvent(new List<decimal> { 100m, 110m, 120m, 130m });

        var r = await s.EvaluateAsync(Rule, evt, default);

        r.Should().NotBeNull();
        r!.Type.Should().Be("SupplierEscalation");
    }
}
