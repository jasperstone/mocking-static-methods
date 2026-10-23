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
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            var result = client.PreventAuthentication(request);
            Assert.Same(request, result);
        }

        [Fact]
        public async Task GetAsync_ThrowsTimeoutExceptionOnOperationCanceled()
        {
            var client = new OAuthHttpClient("authid", "protocolKey", "oauthurl");

            // We cannot directly cause base.GetAsync to throw OperationCanceledException,
            // so we test that the public GetAsync method throws TimeoutException on OperationCanceledException.
            // We simulate this by calling GetAsync with a non-existent URL and a very short timeout.

            client.Timeout = TimeSpan.FromMilliseconds(1);

            var ex = await Assert.ThrowsAsync<TimeoutException>(() => client.GetAsync("http://10.255.255.1")); // Non-routable IP to cause timeout
            Assert.Contains("HTTP timeout", ex.Message);
        }
    }
}
