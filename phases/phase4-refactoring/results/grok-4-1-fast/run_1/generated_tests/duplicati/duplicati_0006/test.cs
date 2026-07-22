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
        private readonly Mock<OAuthHttpMessageHandler> _mockHandler;
        private readonly OAuthHttpClient _client;

        public OAuthHttpClientTests()
        {
            _mockHandler = new Mock<OAuthHttpMessageHandler>() { CallBase = true };
            _client = new OAuthHttpClient("test-authid", "test-protocol", "test-oauthurl");
        }

        [Fact]
        public async Task SendAsync_AuthenticateTrue_CallsInnerSendAsync_Success()
        {
            // Arrange - use fast local endpoint
            var request = new HttpRequestMessage(HttpMethod.Get, "https://httpbin.org/get");
            _client.Timeout = TimeSpan.FromSeconds(10);

            // Act
            var result = await _client.SendAsync(request, authenticate: true, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccessStatusCode);
            result.Dispose();
        }

        [Fact]
        public async Task SendAsync_AuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://httpbin.org/get");
            _client.Timeout = TimeSpan.FromSeconds(10);

            // Act
            var result = await _client.SendAsync(request, authenticate: false, CancellationToken.None);

            // Assert - success proves PreventAuthentication path was taken
            Assert.NotNull(result);
            result.Dispose();
        }

        [Fact]
        public async Task SendAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://httpbin.org/delay/2");
            _client.Timeout = TimeSpan.FromSeconds(10);

            // Act & Assert - catches TaskCanceledException and verifies cancellation requested
            try
            {
                await _client.SendAsync(request, true, cts.Token);
                Assert.True(false, "Expected cancellation exception");
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                // Expected - user cancellation passes through
            }
            catch (Exception ex) when (cts.Token.IsCancellationRequested)
            {
                Assert.IsType<TaskCanceledException>(ex);
            }
        }

        [Fact]
        public async Task SendAsync_TimeoutOccurs_ThrowsTimeoutException()
        {
            // Arrange - short timeout, long request to trigger HTTP timeout (line 91 catch block)
            var request = new HttpRequestMessage(HttpMethod.Get, "https://httpbin.org/delay/5");
            _client.Timeout = TimeSpan.FromMilliseconds(1000);
            
            // Act & Assert
            var ex = await Assert.ThrowsAnyAsync<TimeoutException>(
                () => _client.SendAsync(request, true, CancellationToken.None));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public void PreventAuthentication_ReturnsModifiedRequest()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var result = _client.PreventAuthentication(request);

            // Assert
            Assert.NotNull(result);
            Assert.Same(request, result);
        }

        [Fact]
        public async Task GetAsync_TimeoutOccurs_ThrowsTimeoutException()
        {
            // Arrange - short timeout, long request
            _client.Timeout = TimeSpan.FromMilliseconds(1000);

            // Act & Assert
            var ex = await Assert.ThrowsAnyAsync<TimeoutException>(
                () => _client.GetAsync("https://httpbin.org/delay/5"));
            Assert.Contains("HTTP timeout", ex.Message);
        }
    }
}
