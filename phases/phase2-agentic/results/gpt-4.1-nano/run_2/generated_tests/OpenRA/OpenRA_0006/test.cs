using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using OpenRA.Mods.Common;

public class ItchIntegrationTests
{
    [Fact]
    public void GetPlayerName_Should_Call_GetAsync_When_ApiKey_Is_Present()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = new
        {
            user = new
            {
                url = "https://itch.io/user/123",
                gamer = true,
                id = 123,
                press_user = false,
                developer = true,
                username = "testuser",
                display_name = "Test User"
            }
        };
        var jsonResponse = JsonSerializer.Serialize(responseContent);
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse)
        };

        mockHttpMessageHandler
            .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

        var client = new HttpClient(mockHttpMessageHandler.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.Create()).Returns(client);

        // Set environment variable
        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

        var integration = new ItchIntegration(factoryMock.Object);

        string capturedName = null;
        var callbackInvoked = new ManualResetEvent(false);

        // Act
        integration.GetPlayerName(name =>
        {
            capturedName = name;
            callbackInvoked.Set();
        });

        // Wait for async callback
        Assert.True(callbackInvoked.WaitOne(1000), "Callback was not invoked in time.");

        // Assert
        Assert.Equal("Test User", capturedName);
        mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
    }
}
