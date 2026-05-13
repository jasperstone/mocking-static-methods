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
            var authId = "auth-id";
            var protocolKey = "protocol-key";
            var oauthUrl = "https://example.com/oauth";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);

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
            var authId = "auth-id";
            var protocolKey = "protocol-key";
            var oauthUrl = "https://example.com/oauth";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);

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
            var authId = "auth-id";
            var protocolKey = "protocol-key";
            var oauthUrl = "https://example.com/oauth";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);
            oauthHttpClient.Timeout = TimeSpan.FromMilliseconds(1);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, CancellationToken.None));
        }
    }
}
