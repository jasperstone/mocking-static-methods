using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_WithAuthentication_RequestIsAuthenticated()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            await httpClient.SendAsync(request, true, CancellationToken.None);

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthentication_RequestIsNotAuthenticated()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            await httpClient.SendAsync(request, false, CancellationToken.None);

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithTimeout_ThrowsTimeoutException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new OperationCanceledException());
            var httpClient = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => httpClient.SendAsync(request, true, CancellationToken.None));
        }
    }
}
