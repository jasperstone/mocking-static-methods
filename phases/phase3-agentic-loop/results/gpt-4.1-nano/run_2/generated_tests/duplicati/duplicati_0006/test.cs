using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library;

namespace Duplicati.Tests
{
    public class OAuthHttpClientTests
    {
        private class DummyAuthenticator : OAuthHttpMessageHandler
        {
            public HttpRequestMessage LastRequest { get; private set; }
            public override HttpRequestMessage PreventAuthentication(HttpRequestMessage request)
            {
                LastRequest = request;
                return request;
            }
        }

        [Fact]
        public async Task GetAsync_ShouldThrowTimeoutException_OnOperationCanceled()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            // Set a small timeout for testing
            client.Timeout = TimeSpan.FromMilliseconds(10);
            var requestUri = "http://example.com";

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.GetAsync(requestUri);
            });
        }

        [Fact]
        public async Task SendAsync_ShouldCallBaseSendAsync_WithAuthentication()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, true, cts.Token);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(request, handler.LastRequest);
        }

        [Fact]
        public async Task SendAsync_ShouldCallPreventAuthentication_WhenAuthenticateIsFalse()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act
            await client.SendAsync(request, false, cts.Token);

            // Assert
            Assert.NotNull(handler.LastRequest);
        }

        [Fact]
        public async Task SendAsync_ShouldThrowTimeoutException_OnOperationCanceledWithoutCancellationRequested()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(10);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.SendAsync(request, true, cts.Token);
            });
        }
    }
}
