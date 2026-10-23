using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
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
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);

            // Act
            var response = await oauthHttpClient.SendAsync(request, true, cancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.RequestMessage.Headers.Authorization != null);
        }

        [Fact]
        public async Task SendAsync_WithoutAuthentication_RequestIsNotAuthenticated()
        {
            // Arrange
            var authId = "authId";
            var protocolKey = "protocolKey";
            var oauthUrl = "oauthUrl";
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/path");
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);

            // Act
            var response = await oauthHttpClient.SendAsync(request, false, cancellationToken);

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
            var cancellationToken = new CancellationToken();

            var oauthHttpClient = new OAuthHttpClient(authId, protocolKey, oauthUrl);
            oauthHttpClient.Timeout = TimeSpan.FromMilliseconds(1);

            // Act and Assert
            await Assert.ThrowsAsync<TimeoutException>(() => oauthHttpClient.SendAsync(request, true, cancellationToken));
        }
    }
}
