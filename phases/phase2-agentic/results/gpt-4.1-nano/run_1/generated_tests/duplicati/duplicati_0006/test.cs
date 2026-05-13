using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library;
using Moq;

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
            public DummyAuthenticator() : base("authid", "protocol", "url") { }
            public async Task<string> GetAccessTokenAsync(CancellationToken token)
            {
                await Task.Delay(10);
                return "dummy_token";
            }
        }

        [Fact]
        public async Task GetAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange
            var handler = new Mock<OAuthHttpClient>("authid", "protocol", "url");
            handler.CallBase = true;
            handler.Setup(h => h.Timeout).Returns(TimeSpan.FromMilliseconds(1));
            var client = handler.Object;
            // Force the base GetAsync to delay to trigger timeout
            var delayTask = Task.Delay(50);
            handler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns<HttpRequestMessage, CancellationToken>((req, token) => delayTask.ContinueWith(t => new HttpResponseMessage()));

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.GetAsync("http://test");
            });
        }

        [Fact]
        public async Task SendAsync_WithAuthenticate_CallsSendAsyncWithAuthHeader()
        {
            // Arrange
            var authenticator = new DummyAuthenticator();
            var client = new OAuthHttpClient(authenticator);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, true, cts.Token);

            // Assert
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            Assert.Equal("dummy_token", request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthenticate_CallsPreventAuthentication()
        {
            // Arrange
            var authenticator = new DummyAuthenticator();
            var client = new OAuthHttpClient(authenticator);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var called = false;
            authenticator.PreventAuthentication = (req) =>
            {
                called = true;
                return req;
            };

            // Act
            var response = await client.SendAsync(request, false, CancellationToken.None);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public async Task SendAsync_OperationCanceledExceptionWithoutCancellationToken_ThrowsTimeout()
        {
            // Arrange
            var handler = new Mock<OAuthHttpClient>("authid", "protocol", "url");
            handler.CallBase = true;
            handler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var client = handler.Object;
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
