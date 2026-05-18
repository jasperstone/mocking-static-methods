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
        private const string AuthId = "dummyAuthId";
        private const string ProtocolKey = "dummyProtocolKey";
        private const string OAuthUrl = "http://dummy.oauth.url";

        [Fact]
        public async Task SendAsync_SuccessfulSendWithoutAuthentication()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            var client = new OAuthHttpClient(mockHandler.Object, AuthId, ProtocolKey, OAuthUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await client.SendAsync(request, false, cancellationToken);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutExceptionOnTimeout()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object, AuthId, ProtocolKey, OAuthUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, false, cancellationToken));
        }

        [Fact]
        public async Task SendAsync_RespectsUserCancellation()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object, AuthId, ProtocolKey, OAuthUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            cancellationTokenSource.Cancel();

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, false, cancellationToken));
        }
    }
}
