using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public void Constructor_SetsUserAgent()
        {
            // Arrange & Act
            var client = new OAuthHttpClient("test-authid", "test-protocol", "test-oauthurl");

            // Assert
            Assert.Contains("Duplicati", client.DefaultRequestHeaders.UserAgent.ToString());
        }

        [Fact]
        public void PreventAuthentication_ReturnsSameRequest()
        {
            // Arrange
            var client = new OAuthHttpClient("test-authid", "test-protocol", "test-oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var result = client.PreventAuthentication(request);

            // Assert
            Assert.Same(request, result);
        }

        [Fact]
        public async Task SendAsync_AuthenticateTrue_SuccessfulResponse()
        {
            // Arrange
            var client = new OAuthHttpClient("test-authid", "test-protocol", "https://example.com");
            client.BaseAddress = new Uri("https://example.com/");
            var request = new HttpRequestMessage(HttpMethod.Get, "/test");

            // Act
            var result = await client.SendAsync(request, authenticate: true, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendAsync_AuthenticateFalse_SuccessfulResponse()
        {
            // Arrange
            var client = new OAuthHttpClient("test-authid", "test-protocol", "https://example.com");
            client.BaseAddress = new Uri("https://example.com/");
            var request = new HttpRequestMessage(HttpMethod.Get, "/test");

            // Act
            var result = await client.SendAsync(request, authenticate: false, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendAsync_TokenCancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange
            var client = new OAuthHttpClient("test-authid", "test-protocol", "https://example.com");
            client.BaseAddress = new Uri("https://example.com/");
            var request = new HttpRequestMessage(HttpMethod.Get, "/test");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => client.SendAsync(request, authenticate: true, cts.Token));
            
            Assert.True(cts.Token.IsCancellationRequested);
        }

        [Fact]
        public async Task GetAsync_NoCancellationToken_ThrowsTimeoutOnTimeout()
        {
            // Arrange
            var client = new OAuthHttpClient("test-authid", "test-protocol", "https://example.com");
            client.Timeout = TimeSpan.FromMilliseconds(1);
            client.BaseAddress = new Uri("https://example.com/");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(
                () => client.GetAsync("/test"));
            
            Assert.Contains("HTTP timeout", ex.Message);
        }
    }
}
