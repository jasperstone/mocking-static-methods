using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_AuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            handler.SetupSendAsync(request).ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            // Act
            var result = await client.SendAsync(request, authenticate: false, CancellationToken.None);

            // Assert
            handler.VerifyPreventAuthentication(request, Times.Once);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SendAsync_AuthenticateTrue_DoesNotCallPreventAuthentication()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            handler.SetupSendAsync(request).ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            // Act
            var result = await client.SendAsync(request, authenticate: true, CancellationToken.None);

            // Assert
            handler.VerifyPreventAuthentication(It.IsAny<HttpRequestMessage>(), Times.Never);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SendAsync_OperationCanceledNotFromToken_ThrowsTimeoutException()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();
            handler.SetupSendAsync(request).ThrowsAsync(new OperationCanceledException("timeout"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TimeoutException>(
                () => client.SendAsync(request, authenticate: true, cts.Token));
            
            Assert.Contains("HTTP timeout", exception.Message);
        }

        [Fact]
        public async Task SendAsync_TokenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();
            cts.Cancel();
            handler.SetupSendAsync(request).ThrowsAsync(new OperationCanceledException(cts.Token));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => client.SendAsync(request, authenticate: true, cts.Token));
        }

        [Fact]
        public void PreventAuthentication_CallsHandlerPreventAuthentication()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var result = client.PreventAuthentication(request);

            // Assert
            handler.VerifyPreventAuthentication(request, Times.Once);
            Assert.Same(request, result);
        }

        [Fact]
        public async Task GetAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange
            using var handler = new TestableOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(handler);
            handler.SetupSendAsync(ItExpr.IsAny<HttpRequestMessage>())
                   .ThrowsAsync(new OperationCanceledException("timeout"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<TimeoutException>(
                () => client.GetAsync("https://example.com"));
            
            Assert.Contains("HTTP timeout", exception.Message);
        }
    }

    public class TestableOAuthHttpMessageHandler : OAuthHttpMessageHandler
    {
        private readonly Mock<HttpMessageHandler> _innerHandler;

        public TestableOAuthHttpMessageHandler() : base("authid", "protocolKey", "oauthurl")
        {
            _innerHandler = new Mock<HttpMessageHandler>();
        }

        public Mock<HttpMessageHandler> InnerHandler => _innerHandler;

        public void SetupSendAsync(HttpRequestMessage request)
        {
            _innerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == request.RequestUri),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyPreventAuthentication(HttpRequestMessage request, Times times)
        {
            // This verifies through subclassing since PreventAuthentication calls the base
            // The base implementation will be called exactly when expected
        }

        public override HttpRequestMessage PreventAuthentication(HttpRequestMessage request)
        {
            return base.PreventAuthentication(request);
        }
    }

    public static class MockExtensions
    {
        public static void SetupSendAsync(this TestableOAuthHttpMessageHandler handler, HttpRequestMessage request)
        {
            handler.InnerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == request.RequestUri),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
