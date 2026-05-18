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
        public async Task SendAsync_WithAuthentication_RequestIsAuthenticated()
        {
            // Arrange
            var authId = "authId";
            var protocolKey = "protocolKey";
            var oauthUrl = "oauthUrl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/path");
            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");

            // Act
            var response = await oauthHttpClient.SendAsync(request, true, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.RequestMessage.Headers.Authorization);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthentication_RequestIsNotAuthenticated()
        {
            // Arrange
            var authId = "authId";
            var protocolKey = "protocolKey";
            var oauthUrl = "oauthUrl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/path");
            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");

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
            var authId = "authId";
            var protocolKey = "protocolKey";
            var oauthUrl = "oauthUrl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/path");
            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);
            oauthHttpClient.BaseAddress = new Uri("https://example.com");
            oauthHttpClient.Timeout = TimeSpan.FromSeconds(1);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, CancellationToken.None));
        }
    }
}
