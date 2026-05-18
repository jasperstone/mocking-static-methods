using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Moq.Protected;
using Xunit;

public class OAuthHttpClientTests
{
    [Fact]
    public async Task SendAsync_ShouldThrowTimeoutException_WhenOperationCanceledExceptionIsThrown()
    {
        // Arrange
        var mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new OperationCanceledException());

        var client = new OAuthHttpClient(mockHandler.Object);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cancellationToken));
    }

    [Fact]
    public async Task SendAsync_ShouldNotThrowTimeoutException_WhenCancellationTokenIsRequested()
    {
        // Arrange
        var mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var client = new OAuthHttpClient(mockHandler.Object);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = cts.Token;

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, true, cancellationToken));
    }

    [Fact]
    public async Task SendAsync_ShouldCallPreventAuthentication_WhenAuthenticateIsFalse()
    {
        // Arrange
        var mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
        var client = new OAuthHttpClient(mockHandler.Object);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act
        await client.SendAsync(request, false, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.PreventAuthentication(request), Times.Once);
    }
}
