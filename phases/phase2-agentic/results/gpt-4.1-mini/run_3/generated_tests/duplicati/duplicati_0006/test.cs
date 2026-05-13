using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var client = new OAuthHttpClient(handlerMock.Object);

            var cancellationToken = CancellationToken.None;

            // Setup PreventAuthentication to return the same request
            handlerMock.Setup(h => h.PreventAuthentication(It.IsAny<HttpRequestMessage>())).Returns<HttpRequestMessage>(r => r);

            // Setup SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken) to return a dummy response
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var sendAsyncProtected = new Mock<HttpMessageHandler>();
            // We cannot mock protected SendAsync on HttpClient easily, so we use the real client but override SendAsync on handlerMock
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<HttpCompletionOption>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await client.SendAsync(request, false, cancellationToken);

            // Assert
            handlerMock.Verify(h => h.PreventAuthentication(request), Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_DoesNotCallPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var client = new OAuthHttpClient(handlerMock.Object);

            var cancellationToken = CancellationToken.None;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await client.SendAsync(request, true, cancellationToken);

            // Assert
            handlerMock.Verify(h => h.PreventAuthentication(It.IsAny<HttpRequestMessage>()), Times.Never);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutException_WhenOperationCanceledExceptionAndNotCancelledByToken()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var client = new OAuthHttpClient(handlerMock.Object);

            var cancellationToken = CancellationToken.None;

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, true, cancellationToken));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public async Task SendAsync_ThrowsOperationCanceledException_WhenOperationCanceledExceptionAndCancelledByToken()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var client = new OAuthHttpClient(handlerMock.Object);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, true, cts.Token));
        }
    }
}
