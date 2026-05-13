using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_ReturnsResponse_WhenSendAsyncSucceeds()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(expectedResponse)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var helper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            var response = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

            // Assert
            Assert.Same(expectedResponse, response);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ThrowsAndCallsAttemptParseAndThrowExceptionAsync_WhenSendAsyncThrows()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var exception = new HttpRequestException("Network error");
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(exception)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // We need to create a derived class to override AttemptParseAndThrowExceptionAsync to track its call
            var helperMock = new Mock<JsonWebHelperHttpClient>(httpClient) { CallBase = true };
            bool attemptParseCalled = false;
            helperMock
                .Protected()
                .Setup<Task>("AttemptParseAndThrowExceptionAsync", ItExpr.IsAny<Exception>(), ItExpr.IsAny<HttpResponseMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask)
                .Callback(() => attemptParseCalled = true);

            var helper = helperMock.Object;

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() => helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None));
            Assert.Equal(exception.Message, ex.Message);
            Assert.True(attemptParseCalled);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
