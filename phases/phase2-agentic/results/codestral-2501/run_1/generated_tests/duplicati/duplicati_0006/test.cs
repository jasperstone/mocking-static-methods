using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

namespace Duplicati.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_ShouldThrowTimeoutException_WhenOperationCanceledExceptionIsThrown()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
            mockHandler.Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
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
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var client = new OAuthHttpClient(mockHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, true, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task SendAsync_ShouldNotAuthenticate_WhenAuthenticateIsFalse()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl");
            mockHandler.Setup(handler => handler.PreventAuthentication(It.IsAny<HttpRequestMessage>()))
                .Returns((HttpRequestMessage request) => request);

            var client = new OAuthHttpClient(mockHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = new CancellationToken();

            // Act
            await client.SendAsync(request, false, cancellationToken);

            // Assert
            mockHandler.Verify(handler => handler.PreventAuthentication(request), Times.Once);
        }
    }
}
