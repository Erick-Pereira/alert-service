using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Domain.Entities;
using Simcag.AlertService.Domain.Enums;
using Xunit;

namespace Simcag.AlertService.Tests.Integration;

public sealed class AlertApiExpenseIdContractTests : IClassFixture<AlertApiTestFactory>
{
    private readonly AlertApiTestFactory _factory;

    public AlertApiExpenseIdContractTests(AlertApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAlertById_Should_Expose_ExpenseId_In_Envelope()
    {
        var expenseId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
        var alert = Alert.Create(
            productId: "prod-1",
            productName: "Elevador",
            category: "Manutenção",
            type: "OVERPRICE_MARKET",
            alertCategory: "Superfaturamento",
            severity: AlertSeverity.High,
            deviationPercentage: 25m,
            message: "Teste contrato ExpenseId",
            currentPrice: 100m,
            averagePrice: 80m,
            analyzedAt: DateTime.UtcNow,
            expenseId: expenseId);
        await repo.AddAsync(alert, CancellationToken.None);

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/alerts/{alert.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<ApiEnvelope>();
        json.Should().NotBeNull();
        json!.success.Should().BeTrue();
        json.data!.expenseId.Should().Be(expenseId);
    }

    private sealed class ApiEnvelope
    {
        public bool success { get; set; }
        public AlertPayload? data { get; set; }
    }

    private sealed class AlertPayload
    {
        public Guid? expenseId { get; set; }
    }
}
