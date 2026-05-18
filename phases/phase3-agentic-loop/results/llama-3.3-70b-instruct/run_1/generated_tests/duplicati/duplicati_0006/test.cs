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
            var baseAddress = new Uri("https://example.com");
            var request = new HttpRequestMessage(HttpMethod.Get, baseAddress);

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act
            var response = await oauthHttpClient.SendAsync(request, true, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_InvalidRequest_ThrowsTimeoutException()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);
            oauthHttpClient.Timeout = TimeSpan.FromMilliseconds(1);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, CancellationToken.None));
        }

        [Fact]
        public async Task SendAsync_PreventAuthentication_ReturnsHttpResponseMessage()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var baseAddress = new Uri("https://example.com");
            var request = new HttpRequestMessage(HttpMethod.Get, baseAddress);

            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);
            oauthHttpClient.PreventAuthentication(request);

            // Act
            var response = await oauthHttpClient.SendAsync(request, false, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }
    }
}
