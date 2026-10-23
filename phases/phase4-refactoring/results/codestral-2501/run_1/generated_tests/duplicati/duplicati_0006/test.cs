using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

public class OAuthHttpClientTests
{
    private class TestOAuthHttpClient : OAuthHttpClient
    {
        public TestOAuthHttpClient(string authid, string protocolKey, string oauthurl)
            : base(authid, protocolKey, oauthurl)
        {
        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException();
        }
    }

    [Fact]
    public async Task SendAsync_ShouldThrowTimeoutException_WhenTimeoutExceeds()
    {
        // Arrange
        var authid = "test-authid";
        var protocolKey = "test-protocolKey";
        var oauthurl = "https://test-oauthurl.com";
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test.com");
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

        var client = new TestOAuthHttpClient(authid, protocolKey, oauthurl);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cancellationToken));
    }

    [Fact]
    public async Task GetAsync_ShouldThrowTimeoutException_WhenTimeoutExceeds()
    {
        // Arrange
        var authid = "test-authid";
        var protocolKey = "test-protocolKey";
        var oauthurl = "https://test-oauthurl.com";
        var requestUri = "https://test.com";

        var client = new TestOAuthHttpClient(authid, protocolKey, oauthurl);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => client.GetAsync(requestUri));
    }
}
