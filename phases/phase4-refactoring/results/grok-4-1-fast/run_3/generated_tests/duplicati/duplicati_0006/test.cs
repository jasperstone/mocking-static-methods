using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class OAuthHttpClientTests
    {
        private readonly Mock<OAuthHttpMessageHandler> _mockHandler;
        private readonly OAuthHttpClient _client;

        public OAuthHttpClientTests()
        {
            _mockHandler = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            _client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            
            // Inject mock handler using reflection
            var field = typeof(OAuthHttpClient).GetField("m_authenticator", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_client, _mockHandler.Object);
        }

        [Fact]
        public async Task SendAsync_AuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            _mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), 
                    It.IsAny<HttpCompletionOption>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _mockHandler
                .Setup(h => h.PreventAuthentication(request))
                .Returns(request);

            // Act
            var result = await _client.SendAsync(request, authenticate: false, CancellationToken.None);

            // Assert
            _mockHandler.Verify(h => h.PreventAuthentication(request), Times.Once);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendAsync_AuthenticateTrue_DoesNotCallPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            _mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), 
                    It.IsAny<HttpCompletionOption>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            // Act
            var result = await _client.SendAsync(request, authenticate: true, CancellationToken.None);

            // Assert
            _mockHandler.Verify(h => h.PreventAuthentication(It.IsAny<HttpRequestMessage>()), Times.Never);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendAsync_OperationCanceledNotFromToken_ThrowsTimeoutException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();
            _mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), 
                    It.IsAny<HttpCompletionOption>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException("timeout"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TimeoutException>(
                () => _client.SendAsync(request, authenticate: true, cts.Token));
            
            Assert.Contains("HTTP timeout", exception.Message);
        }

        [Fact]
        public async Task SendAsync_TokenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();
            cts.Cancel();
            _mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), 
                    It.IsAny<HttpCompletionOption>(), 
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _client.SendAsync(request, authenticate: true, cts.Token));
        }

        [Fact]
        public void PreventAuthentication_CallsHandlerPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            _mockHandler
                .Setup(h => h.PreventAuthentication(request))
                .Returns(request);

            // Act
            var result = _client.PreventAuthentication(request);

            // Assert
            _mockHandler.Verify(h => h.PreventAuthentication(request), Times.Once);
            Assert.Same(request, result);
        }
    }
}
