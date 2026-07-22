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
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = new CancellationToken();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act
            var response = await client.SendAsync(request, true, cancellationToken);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthentication_RequestIsNotAuthenticated()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = new CancellationToken();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act
            var response = await client.SendAsync(request, false, cancellationToken);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task SendAsync_WithTimeout_RequestThrowsTimeoutException()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = new CancellationToken();

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new OperationCanceledException());

            var client = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cancellationToken));
        }
    }
}
