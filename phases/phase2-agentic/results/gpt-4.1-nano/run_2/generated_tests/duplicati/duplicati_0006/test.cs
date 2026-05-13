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
        public async Task SendAsync_Should_Call_SendAsync_With_Correct_Parameters()
        {
            // Arrange
            var authenticator = new DummyAuthenticator();
            var client = new OAuthHttpClient(authenticator);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, authenticate: true, cancellationToken: cts.Token);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(request, authenticator.LastRequest);
        }

        [Fact]
        public async Task SendAsync_Should_Call_PreventAuthentication_When_Not_Authenticating()
        {
            // Arrange
            var authenticator = new DummyAuthenticator();
            var client = new OAuthHttpClient(authenticator);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            await client.SendAsync(request, authenticate: false, CancellationToken.None);

            // Assert
            Assert.Equal(request, authenticator.LastRequest);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_TimeoutException_On_OperationCanceledException()
        {
            // Arrange
            var handler = new DelegatingHandlerStub((req, ct) => throw new OperationCanceledException()) { InnerHandler = new DummyHandler() };
            var client = new OAuthHttpClient("authid", "protocol", "url");
            var clientInstance = new OAuthHttpClient(handler);
            // Use reflection to set Timeout property for test
            var timeoutProperty = typeof(HttpClient).GetProperty("Timeout");
            timeoutProperty.SetValue(clientInstance, TimeSpan.FromMilliseconds(1));

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => clientInstance.GetAsync("http://test"));
        }

        private class DummyHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }

        private class DelegatingHandlerStub : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }
    }
}
