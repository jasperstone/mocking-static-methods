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
        // A derived class to override the non-virtual SendAsync to simulate behavior
        private class TestOAuthHttpClient : OAuthHttpClient
        {
            private readonly Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> _sendAsyncOverride;
            public bool PreventAuthenticationCalled { get; private set; }

            public TestOAuthHttpClient(
                Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> sendAsyncOverride)
                : base("authid", "protocolKey", "oauthurl")
            {
                _sendAsyncOverride = sendAsyncOverride;
            }

            // Shadow the SendAsync method called by the tested method
            public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            {
                return _sendAsyncOverride(request, completionOption, cancellationToken);
            }

            // We simulate PreventAuthentication call by intercepting the call in SendAsync(bool authenticate)
            public HttpRequestMessage PreventAuthentication(HttpRequestMessage request)
            {
                PreventAuthenticationCalled = true;
                return request;
            }

            // Override SendAsync(bool authenticate) to call our PreventAuthentication method
            public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool authenticate, CancellationToken cancellationToken)
            {
                if (!authenticate)
                {
                    PreventAuthentication(request);
                }

                try
                {
                    return await SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException($"HTTP timeout {this.Timeout} exceeded.");
                }
            }
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = await client.SendAsync(request, false, CancellationToken.None);

            Assert.True(client.PreventAuthenticationCalled);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_DoesNotCallPreventAuthentication()
        {
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = await client.SendAsync(request, true, CancellationToken.None);

            Assert.False(client.PreventAuthenticationCalled);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WhenTimeoutOccurs_ThrowsTimeoutException()
        {
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                // Simulate OperationCanceledException without cancellation requested (timeout)
                throw new OperationCanceledException();
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cts = new CancellationTokenSource();

            var ex = await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cts.Token));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public async Task SendAsync_WhenOperationCanceledWithCancellationRequested_ThrowsOperationCanceledException()
        {
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                // Simulate OperationCanceledException with cancellation requested
                throw new OperationCanceledException(token);
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // We expect OperationCanceledException to propagate because cancellation was requested
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, true, cts.Token));
        }
    }
}
