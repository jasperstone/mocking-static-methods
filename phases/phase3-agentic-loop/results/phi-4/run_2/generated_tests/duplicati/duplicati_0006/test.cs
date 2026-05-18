using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_SuccessfulSendWithoutAuthentication()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            var authid = "testAuthId";
            var protocolKey = "testProtocolKey";
            var oauthurl = "http://oauthurl.com";

            var httpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await httpClient.SendAsync(request, false, cancellationToken);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutExceptionOnTimeout()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var authid = "testAuthId";
            var protocolKey = "testProtocolKey";
            var oauthurl = "http://oauthurl.com";

            var httpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => httpClient.SendAsync(request, false, cancellationToken));
        }

        [Fact]
        public async Task SendAsync_RespectsUserCancellation()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var authid = "testAuthId";
            var protocolKey = "testProtocolKey";
            var oauthurl = "http://oauthurl.com";

            var httpClient = new OAuthHttpClient(authid, protocolKey, oauthurl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = new CancellationToken(true);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => httpClient.SendAsync(request, false, cancellationToken));
        }
    }
}
