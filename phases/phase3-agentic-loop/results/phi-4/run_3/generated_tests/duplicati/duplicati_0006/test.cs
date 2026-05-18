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
        public async Task SendAsync_SuccessfulSend_ReturnsResponse()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>(MockBehavior.Strict);
            var mockResponse = new HttpResponseMessage();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var client = new OAuthHttpClient(mockHandler.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act
            var response = await client.SendAsync(request, true, CancellationToken.None);

            // Assert
            Assert.Equal(mockResponse, response);
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>(MockBehavior.Strict);
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, CancellationToken.None));
        }

        [Fact]
        public async Task SendAsync_PreventAuthentication_CallsPreventAuthentication()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>(MockBehavior.Strict);
            var mockAuthenticator = new Mock<OAuthHttpMessageHandler>(MockBehavior.Strict);
            mockAuthenticator
                .Setup(a => a.PreventAuthentication(It.IsAny<HttpRequestMessage>()))
                .Returns((HttpRequestMessage req) => req);

            var client = new OAuthHttpClient(mockAuthenticator.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act
            await client.SendAsync(request, false, CancellationToken.None);

            // Assert
            mockAuthenticator.Verify(a => a.PreventAuthentication(request), Times.Once);
        }
    }
}
