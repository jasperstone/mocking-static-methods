using System;
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
        public async Task SendAsync_ThrowsTimeoutException_WhenOperationCanceledDueToTimeout()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler
                .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var httpClient = new OAuthHttpClient(null, null, null)
            {
                InnerHandler = mockHandler.Object
            };

            var request = new HttpRequestMessage();
            var cancellationToken = new CancellationToken(false);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => httpClient.SendAsync(request, true, cancellationToken));
        }

        [Fact]
        public async Task SendAsync_PreventsAuthentication_WhenAuthenticateIsFalse()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler
                .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage());

            var mockAuthenticator = new Mock<OAuthHttpMessageHandler>();
            mockAuthenticator
                .Setup(auth => auth.PreventAuthentication(It.IsAny<HttpRequestMessage>()))
                .Returns((HttpRequestMessage req) => req);

            var httpClient = new OAuthHttpClient(null, null, null)
            {
                InnerHandler = mockHandler.Object,
                m_authenticator = mockAuthenticator.Object
            };

            var request = new HttpRequestMessage();
            var cancellationToken = new CancellationToken(false);

            // Act
            await httpClient.SendAsync(request, false, cancellationToken);

            // Assert
            mockAuthenticator.Verify(auth => auth.PreventAuthentication(request), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ReturnsHttpResponseMessage_WhenNoExceptions()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var response = new HttpResponseMessage();
            mockHandler
                .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var httpClient = new OAuthHttpClient(null, null, null)
            {
                InnerHandler = mockHandler.Object
            };

            var request = new HttpRequestMessage();
            var cancellationToken = new CancellationToken(false);

            // Act
            var result = await httpClient.SendAsync(request, true, cancellationToken);

            // Assert
            Assert.Equal(response, result);
            mockHandler.Verify(handler => handler.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
        }
    }
}
