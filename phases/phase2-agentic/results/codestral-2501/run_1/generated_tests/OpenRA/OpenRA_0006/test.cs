using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class ItchIntegrationTests
{
    [Fact]
    public async Task GetPlayerName_ShouldCallGetAsync()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"user\":{\"username\":\"testuser\",\"display_name\":\"Test User\"}}")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        HttpClientFactory.Create = () => httpClient;

        var itchIntegration = new ItchIntegration();
        var callbackCalled = false;
        Action<string> callback = name =>
        {
            callbackCalled = true;
            Assert.Equal("Test User", name);
        };

        // Act
        itchIntegration.GetPlayerName(callback);
        await Task.Delay(1000); // Wait for the async operation to complete

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
        Assert.True(callbackCalled);
    }
}
