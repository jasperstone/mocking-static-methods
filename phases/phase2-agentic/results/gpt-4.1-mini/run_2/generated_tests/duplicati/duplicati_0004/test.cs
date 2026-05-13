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
        public async Task GetResponseUncheckedAsync_CallsSendAsync_ReturnsResponse()
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
            Assert.Equal(expectedResponse, response);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_WhenSendAsyncThrows_AttemptParseAndThrowExceptionAsyncCalledAndExceptionRethrown()
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
            var helperMock = new Mock<JsonWebHelperHttpClient>(httpClient) { CallBase = true };

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            helperMock
                .Protected()
                .Setup<Task>("AttemptParseAndThrowExceptionAsync", 
                    ItExpr.IsAny<Exception>(), 
                    ItExpr.IsAny<HttpResponseMessage?>(), 
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                helperMock.Object.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

            Assert.Equal(exception.Message, ex.Message);

            helperMock.Protected().Verify(
                "AttemptParseAndThrowExceptionAsync",
                Times.Once(),
                ItExpr.Is<Exception>(e => e == exception),
                ItExpr.IsAny<HttpResponseMessage?>(),
                ItExpr.IsAny<CancellationToken>());

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
