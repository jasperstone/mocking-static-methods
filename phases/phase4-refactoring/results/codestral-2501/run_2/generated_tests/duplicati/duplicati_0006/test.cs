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
        var mockOAuthHttpMessageHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
        mockOAuthHttpMessageHandler
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var client = new OAuthHttpClient(mockOAuthHttpMessageHandler.Object)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, CancellationToken.None));
    }
}
