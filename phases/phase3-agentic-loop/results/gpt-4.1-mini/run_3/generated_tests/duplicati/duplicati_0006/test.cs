using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class OAuthHttpClientTests
    {
        // A derived class to override the protected SendAsync to simulate behavior for testing
        private class TestOAuthHttpClient : OAuthHttpClient
        {
            private readonly Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> _sendAsyncOverride;

            public TestOAuthHttpClient(
                Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> sendAsyncOverride)
                : base("authid", "protocolKey", "oauthurl")
            {
                _sendAsyncOverride = sendAsyncOverride;
            }

            // Override the SendAsync method called by the tested method to redirect to the delegate
            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            {
                return _sendAsyncOverride(request, completionOption, cancellationToken);
            }
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_CallsSendAsyncAndReturnsResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                Assert.Equal(HttpCompletionOption.ResponseHeadersRead, opt);
                return Task.FromResult(expectedResponse);
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act
            var response = await client.SendAsync(request, authenticate: true, CancellationToken.None);

            // Assert
            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                // The request passed here should have the PreventAuthentication option set
                Assert.True(req.Options.TryGetValue(new HttpRequestOptionsKey<bool>("PreventAuthentication"), out var preventAuth) && preventAuth);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // We call PreventAuthentication to set the option on the request
            var preventAuthRequest = client.PreventAuthentication(request);

            // Act
            var response = await client.SendAsync(preventAuthRequest, authenticate: false, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutExceptionOnOperationCanceledExceptionWithoutCancellationRequested()
        {
            // Arrange
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                throw new OperationCanceledException();
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cts = new CancellationTokenSource();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, authenticate: true, cts.Token));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public async Task SendAsync_PropagatesOperationCanceledExceptionWhenCancellationRequested()
        {
            // Arrange
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                throw new OperationCanceledException();
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, authenticate: true, cts.Token));
        }
    }
}
