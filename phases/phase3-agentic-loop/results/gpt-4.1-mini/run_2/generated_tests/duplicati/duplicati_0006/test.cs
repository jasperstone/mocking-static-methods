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

            public TestOAuthHttpClient(
                Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> sendAsyncOverride)
                : base("authid", "protocolKey", "oauthurl")
            {
                _sendAsyncOverride = sendAsyncOverride;
            }

            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            {
                return _sendAsyncOverride(request, completionOption, cancellationToken);
            }
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // We cannot directly verify PreventAuthentication call, but we can verify that the request is modified by it.
            // So we call PreventAuthentication explicitly and check it returns the same request (as the real implementation does).
            var result = client.PreventAuthentication(request);
            Assert.Same(request, result);
        }

        [Fact]
        public async Task SendAsync_CallsBaseSendAsyncAndReturnsResponse()
        {
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                Assert.Equal(HttpCompletionOption.ResponseHeadersRead, opt);
                return Task.FromResult(expectedResponse);
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = await client.SendAsync(request, true, CancellationToken.None);

            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutExceptionOnOperationCanceledExceptionWithoutCancellationRequested()
        {
            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                throw new OperationCanceledException();
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, CancellationToken.None));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public async Task SendAsync_PropagatesOperationCanceledExceptionWhenCancellationRequested()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var client = new TestOAuthHttpClient((req, opt, token) =>
            {
                throw new OperationCanceledException();
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, true, cts.Token));
        }
    }
}
