using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_ValidRequest_ReturnsHttpResponseMessage()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");

            // Act
            var response = await oauthHttpClient.SendAsync(request, true, cancellationToken);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "invalid-url");
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => oauthHttpClient.SendAsync(request, true, cancellationToken));
        }

        [Fact]
        public async Task SendAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");
            oauthHttpClient.Timeout = TimeSpan.FromSeconds(1);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, cancellationToken));
        }
    }
}
