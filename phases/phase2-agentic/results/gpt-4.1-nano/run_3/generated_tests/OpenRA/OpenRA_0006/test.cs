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
    public async Task GetPlayerName_ShouldInvokeGetAsync_WhenApiKeyIsSet()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = new StringContent("{\"user\": {\"username\": \"testuser\", \"display_name\": \"Test User\"}}");
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        };

        mockHttpMessageHandler
            .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.Create()).Returns(httpClient);

        // Set environment variable
        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

        var integration = new ItchIntegration();

        string capturedName = null;
        var callbackInvoked = new ManualResetEvent(false);

        // Replace Game.RunAfterTick to invoke callback immediately for test
        Game.RunAfterTick = (action) =>
        {
            action();
            callbackInvoked.Set();
        };

        // Act
        await Task.Run(() => integration.GetPlayerName(name =>
        {
            capturedName = name;
            callbackInvoked.Set();
        }));

        // Wait for callback
        callbackInvoked.WaitOne(1000);

        // Assert
        Assert.Equal("Test User", capturedName);
        mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
    }
}
