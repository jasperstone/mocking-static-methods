using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_WithAuthentication_RequestIsAuthenticated()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act
            var response = await oauthHttpClient.SendAsync(request, true, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.RequestMessage.Headers.Authorization != null);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthentication_RequestIsNotAuthenticated()
        {
            // Arrange
            var authid = "authid";
            var protocolKey = "protocolKey";
            var oauthurl = "oauthurl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var oauthHttpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            // Act
            var response = await oauthHttpClient.SendAsync(request, false, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Null(response.RequestMessage.Headers.Authorization);
        }

        [Fact]
        public async Task SendAsync_WithTimeout_ThrowsTimeoutException()
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
    }
}
