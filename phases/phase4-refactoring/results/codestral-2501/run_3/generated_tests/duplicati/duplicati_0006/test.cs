using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

public class OAuthHttpClientTests
{
    [Fact]
    public async Task SendAsync_ShouldThrowTimeoutException_WhenOperationCanceledExceptionIsThrown()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<OAuthHttpMessageHandler>();
        mockHttpMessageHandler
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var oauthHttpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, cancellationToken));
    }

    [Fact]
    public async Task SendAsync_ShouldNotThrowTimeoutException_WhenCancellationTokenIsRequested()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<OAuthHttpMessageHandler>();
        mockHttpMessageHandler
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var oauthHttpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => oauthHttpClient.SendAsync(request, true, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResponse_WhenRequestIsValid()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<OAuthHttpMessageHandler>();
        mockHttpMessageHandler
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var oauthHttpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        // Act
        var response = await oauthHttpClient.SendAsync(request, true, cancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
