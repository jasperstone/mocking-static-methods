using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public void PreventAuthentication_ReturnsSameRequest()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            var result = client.PreventAuthentication(request);
            Assert.Same(request, result);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_Completes()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            var cts = new CancellationTokenSource();
            var response = await client.SendAsync(request, false, cts.Token);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_Completes()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            var cts = new CancellationTokenSource();
            var response = await client.SendAsync(request, true, cts.Token);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetAsync_WithInvalidUri_ThrowsHttpRequestException()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

            // GetAsync with an invalid URI throws HttpRequestException, not TimeoutException, because no cancellation token is used.
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await client.GetAsync("http://invalid.invalid");
            });
        }
    }
}
