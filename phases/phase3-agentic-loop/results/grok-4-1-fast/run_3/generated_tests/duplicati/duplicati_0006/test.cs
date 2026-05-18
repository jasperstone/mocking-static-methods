using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class OAuthHttpClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly OAuthHttpMessageHandler _oauthHandler;
        private readonly OAuthHttpClient _client;

        public OAuthHttpClientTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _oauthHandler = new OAuthHttpMessageHandler("test-authid", "test-protocol", "test-oauthurl");
            _client = new OAuthHttpClient(_oauthHandler);
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_CallsBaseSendAsyncAndReturnsResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var cancellationToken = CancellationToken.None;

            SetupMockHandler(response);

            // Act
            var result = await _client.SendAsync(request, true, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            _mockHandler.Verify();
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var cancellationToken = CancellationToken.None;

            SetupMockHandler(response);

            // Act
            var result = await _client.SendAsync(request, false, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            // Verify PreventAuthentication was called by checking the Options
            Assert.True(request.Options.TryGetValue(OAuthHttpMessageHandler.PreventAuthenticationOption, out bool preventAuth));
            Assert.True(preventAuth);
        }

        [Fact]
        public async Task SendAsync_WithCancellationTokenRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => _client.SendAsync(request, true, cancellationToken));
        }

        [Fact]
        public async Task SendAsync_WithTimeoutException_ThrowsTimeoutException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = CancellationToken.None;

            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new OperationCanceledException())
                .Verifiable();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => _client.SendAsync(request, true, cancellationToken));
            Assert.Contains("HTTP timeout 00:00:30", ex.Message);
            _mockHandler.Verify();
        }

        [Fact]
        public void PreventAuthentication_CallsHandlerPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var result = _client.PreventAuthentication(request);

            // Assert
            Assert.Same(request, result);
            Assert.True(request.Options.TryGetValue(OAuthHttpMessageHandler.PreventAuthenticationOption, out bool preventAuth));
            Assert.True(preventAuth);
        }

        private void SetupMockHandler(HttpResponseMessage response)
        {
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();
        }
    }
}
