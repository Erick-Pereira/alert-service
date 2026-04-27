using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Simcag.AlertService.Infrastructure.Persistence.DbContext;
using Simcag.AlertService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Simcag.AlertService.Tests;

public sealed class AlertRepositoryEfTests
{
    private static (AlertDbContext Context, AlertRepository Repository) CreateSut()
    {
        var options = new DbContextOptionsBuilder<AlertDbContext>()
            .UseInMemoryDatabase("alerts-" + Guid.NewGuid().ToString("N"))
            .Options;
        var context = new AlertDbContext(options);
        _ = context.Database.EnsureCreated();
        return (context, new AlertRepository(context));
    }

    private static Alert Sample(string productId, string type) => Alert.Create(
        productId: productId,
        productName: "N",
        category: "C",
        type: type,
        alertCategory: "AC",
        severity: AlertSeverity.Medium,
        deviationPercentage: 1m,
        message: "M",
        currentPrice: 10m,
        averagePrice: 8m,
        analyzedAt: DateTime.UtcNow);

    [Fact]
    public async Task GetPageAsync_filters_type_case_insensitively()
    {
        var (ctx, repo) = CreateSut();
        await using (ctx)
        {
            await repo.AddAsync(Sample("p1", "OVERRUN"), default);
            await repo.AddAsync(Sample("p2", "overrun"), default);
            await repo.AddAsync(Sample("p3", "OTHER"), default);

            var page = await repo.GetPageAsync(1, 10, "overrun", default);

            page.TotalCount.Should().Be(2);
            page.Items.Count.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetPageAsync_paginates()
    {
        var (ctx, repo) = CreateSut();
        await using (ctx)
        {
            for (var i = 0; i < 5; i++)
                await repo.AddAsync(Sample("p" + i, "T"), default);

            var page1 = await repo.GetPageAsync(1, 2, null, default);
            var page2 = await repo.GetPageAsync(2, 2, null, default);

            page1.Items.Count.Should().Be(2);
            page1.TotalCount.Should().Be(5);
            page2.Items.Count.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetStatsAsync_counts_by_type_and_unread()
    {
        var (ctx, repo) = CreateSut();
        await using (ctx)
        {
            await repo.AddAsync(Sample("a1", "X"), default);
            await repo.AddAsync(Sample("a2", "X"), default);
            var third = Sample("a3", "Y");
            await repo.AddAsync(third, default);

            var alertY = await repo.GetByIdForUpdateAsync(third.Id, CancellationToken.None);
            alertY.Should().NotBeNull();
            alertY!.MarkAsResolved();
            await repo.UpdateAsync(alertY, CancellationToken.None);

            var q = await repo.GetStatsAsync(default);
            q.Total.Should().Be(3);
            q.UnreadCount.Should().Be(2);
            q.ByType["X"].Should().Be(2);
            q.ByType["Y"].Should().Be(1);
        }
    }
}
