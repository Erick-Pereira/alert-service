using System.Net;
using FluentAssertions;
using Xunit;

namespace Simcag.AlertService.Tests.Integration;

public class AlertApiHealthTests : IClassFixture<AlertApiTestFactory>
{
    private readonly AlertApiTestFactory _factory;

    public AlertApiHealthTests(AlertApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Health_Returns_200_Healthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Health_Live_Returns_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Health_Ready_Returns_200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
