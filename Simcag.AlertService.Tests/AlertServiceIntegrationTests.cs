using Microsoft.Extensions.Logging;
using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public class AlertServiceIntegrationTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<ILogger<AlertEvaluationService>> _loggerMock;
    private readonly AlertEvaluationService _alertService;

    public AlertServiceIntegrationTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _loggerMock = new Mock<ILogger<AlertEvaluationService>>();
        _alertService = new AlertEvaluationService(_alertRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CompleteAlertFlow_ShouldCreateAppropriateAlerts()
    {
        // Arrange - Setup repository to return no existing alerts
        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        var testCases = new[]
        {
            // DROP alert scenario
            new
            {
                Event = new PriceAnalyzedEvent
                {
                    ProductId = "drop-product",
                    AveragePrice = 100m,
                    LastPrice = 90m,
                    PriceVariation = -10m,
                    Trend = "DOWN"
                },
                ExpectedType = "DROP",
                ExpectedMessageContains = "Price drop detected"
            },

            // RISE alert scenario
            new
            {
                Event = new PriceAnalyzedEvent
                {
                    ProductId = "rise-product",
                    AveragePrice = 100m,
                    LastPrice = 110m,
                    PriceVariation = 10m,
                    Trend = "UP"
                },
                ExpectedType = "RISE",
                ExpectedMessageContains = "Price increase detected"
            },

            // TREND alert scenario
            new
            {
                Event = new PriceAnalyzedEvent
                {
                    ProductId = "trend-product",
                    AveragePrice = 100m,
                    LastPrice = 95m,
                    PriceVariation = -5m,
                    Trend = "DOWN"
                },
                ExpectedType = "TREND",
                ExpectedMessageContains = "Downward trend detected"
            },

            // No alert scenario
            new
            {
                Event = new PriceAnalyzedEvent
                {
                    ProductId = "no-alert-product",
                    AveragePrice = 100m,
                    LastPrice = 102m,
                    PriceVariation = 2m,
                    Trend = "STABLE"
                },
                ExpectedType = (string?)null,
                ExpectedMessageContains = (string?)null
            }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            // Reset mock for each test case
            _alertRepositoryMock.Invocations.Clear();

            // Act
            await _alertService.EvaluateAsync(testCase.Event, CancellationToken.None);

            // Assert
            if (testCase.ExpectedType != null)
            {
                _alertRepositoryMock.Verify(x => x.AddAsync(
                    It.Is<Alert>(alert =>
                        alert.ProductId == testCase.Event.ProductId &&
                        alert.Type == testCase.ExpectedType &&
                        alert.Message.Contains(testCase.ExpectedMessageContains!)),
                    It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                // No alert should be created
                _alertRepositoryMock.Verify(x => x.AddAsync(
                    It.IsAny<Alert>(),
                    It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }

    [Fact]
    public async Task IdempotenceCheck_ShouldPreventDuplicateAlertsWithin24Hours()
    {
        // Arrange
        var productId = "duplicate-product";
        var recentAlert = Alert.Create(productId, "DROP", "Recent drop alert");
        // Set creation time to 12 hours ago (within 24h window)
        typeof(Alert).GetProperty("CreatedAt")?.SetValue(recentAlert, DateTime.UtcNow.AddHours(-12));

        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentAlert);

        var priceEvent = new PriceAnalyzedEvent
        {
            ProductId = productId,
            AveragePrice = 100m,
            LastPrice = 85m,
            PriceVariation = -15m, // Would normally trigger DROP alert
            Trend = "DOWN"
        };

        // Act
        await _alertService.EvaluateAsync(priceEvent, CancellationToken.None);

        // Assert - No new alert should be created due to recent alert
        _alertRepositoryMock.Verify(x => x.AddAsync(
            It.IsAny<Alert>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AlertCreation_ShouldUseCorrectMessageFormat()
    {
        // Arrange
        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert?)null);

        var priceEvent = new PriceAnalyzedEvent
        {
            ProductId = "message-test-product",
            AveragePrice = 200.50m,
            LastPrice = 180.45m,
            PriceVariation = -10m,
            Trend = "DOWN"
        };

        // Act
        await _alertService.EvaluateAsync(priceEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Alert>(alert =>
                alert.Message == "Price drop detected: -10.00% variation (Last: $180.45, Avg: $200.50)"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}