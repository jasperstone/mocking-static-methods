using System;
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
        public async Task SendAsync_SuccessfulSend_ReturnsHttpResponseMessage()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var mockRequest = new HttpRequestMessage();
            var cancellationToken = CancellationToken.None;

            mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), cancellationToken))
                .ReturnsAsync(new HttpResponseMessage());

            var client = new OAuthHttpClient(mockHandler.Object);

            // Act
            var response = await client.SendAsync(mockRequest, true, cancellationToken);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var mockRequest = new HttpRequestMessage();
            var cancellationToken = CancellationToken.None;

            mockHandler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), cancellationToken))
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(mockRequest, true, cancellationToken));
        }

        [Fact]
        public void PreventAuthentication_CallsPreventAuthenticationOnHandler()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var mockRequest = new HttpRequestMessage();

            var client = new OAuthHttpClient(mockHandler.Object);

            // Act
            client.PreventAuthentication(mockRequest);

            // Assert
            mockHandler.Verify(h => h.PreventAuthentication(mockRequest), Times.Once);
        }
    }
}
