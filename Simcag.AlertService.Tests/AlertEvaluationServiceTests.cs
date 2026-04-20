using Moq;
using Simcag.AlertService.Application.Interfaces;
using Simcag.AlertService.Application.Services;
using Simcag.AlertService.Domain.Entities;
using Simcag.Shared.Events;
using Xunit;

namespace Simcag.AlertService.Tests;

public class AlertEvaluationServiceTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AlertEvaluationService>> _loggerMock;
    private readonly AlertEvaluationService _service;

    public AlertEvaluationServiceTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AlertEvaluationService>>();
        _service = new AlertEvaluationService(_alertRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldCreateDropAlert_WhenPriceVariationIsMinus10()
    {
        // Arrange
        var priceAnalyzedEvent = new PriceAnalyzedEvent
        {
            ProductId = "product-1",
            AveragePrice = 100,
            LastPrice = 90,
            PriceVariation = -10,
            Trend = "DOWN"
        };

        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync("product-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Simcag.AlertService.Domain.Entities.Alert?)null);

        // Act
        await _service.EvaluateAsync(priceAnalyzedEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Simcag.AlertService.Domain.Entities.Alert>(alert =>
                alert.ProductId == "product-1" &&
                alert.Type == "DROP" &&
                alert.Message.Contains("Price drop detected") &&
                alert.Message.Contains("-10.00%")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldCreateRiseAlert_WhenPriceVariationIsPlus10()
    {
        // Arrange
        var priceAnalyzedEvent = new PriceAnalyzedEvent
        {
            ProductId = "product-2",
            AveragePrice = 100,
            LastPrice = 110,
            PriceVariation = 10,
            Trend = "UP"
        };

        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync("product-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Simcag.AlertService.Domain.Entities.Alert?)null);

        // Act
        await _service.EvaluateAsync(priceAnalyzedEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Simcag.AlertService.Domain.Entities.Alert>(alert =>
                alert.ProductId == "product-2" &&
                alert.Type == "RISE" &&
                alert.Message.Contains("Price increase detected") &&
                alert.Message.Contains("10.00%")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldCreateTrendAlert_WhenTrendIsDown()
    {
        // Arrange
        var priceAnalyzedEvent = new PriceAnalyzedEvent
        {
            ProductId = "product-3",
            AveragePrice = 100,
            LastPrice = 95,
            PriceVariation = -5,
            Trend = "DOWN"
        };

        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync("product-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Simcag.AlertService.Domain.Entities.Alert?)null);

        // Act
        await _service.EvaluateAsync(priceAnalyzedEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Simcag.AlertService.Domain.Entities.Alert>(alert =>
                alert.ProductId == "product-3" &&
                alert.Type == "TREND" &&
                alert.Message.Contains("Downward trend detected")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldNotCreateAlert_WhenPriceVariationIsSmall()
    {
        // Arrange
        var priceAnalyzedEvent = new PriceAnalyzedEvent
        {
            ProductId = "product-4",
            AveragePrice = 100,
            LastPrice = 102,
            PriceVariation = 2,
            Trend = "UP"
        };

        // Act
        await _service.EvaluateAsync(priceAnalyzedEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Simcag.AlertService.Domain.Entities.Alert>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldSkipAlert_WhenAlertExistsWithin24Hours()
    {
        // Arrange
        var priceAnalyzedEvent = new PriceAnalyzedEvent
        {
            ProductId = "product-5",
            AveragePrice = 100,
            LastPrice = 90,
            PriceVariation = -10,
            Trend = "DOWN"
        };

        var recentAlert = Alert.Create(
            "product-5", "DROP", "Recent alert");

        _alertRepositoryMock
            .Setup(x => x.GetLastByProductIdAsync("product-5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentAlert);

        // Act
        await _service.EvaluateAsync(priceAnalyzedEvent, CancellationToken.None);

        // Assert
        _alertRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Simcag.AlertService.Domain.Entities.Alert>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}