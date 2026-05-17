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
                // Simulate a delay that causes timeout
                await client.GetAsync(requestUri);
            });
        }

        [Fact]
        public async Task SendAsync_ShouldCallBaseSendAsync_WithCorrectParameters()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, authenticate: true, cancellationToken: cts.Token);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(request.Method, response.RequestMessage.Method);
            Assert.Equal(request.RequestUri, response.RequestMessage.RequestUri);
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
            await client.SendAsync(request, authenticate: false, cancellationToken: cts.Token);

            // Assert
            Assert.NotNull(handler.LastRequest);
        }
    }
}
