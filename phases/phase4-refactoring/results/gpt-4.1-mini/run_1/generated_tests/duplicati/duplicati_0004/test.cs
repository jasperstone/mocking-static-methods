using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Duplicati.Library;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_ReturnsResponse_WhenSuccess()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
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
               ItExpr.IsAny<CancellationToken>()
            );
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
                  ItExpr.IsAny<CancellationToken>()
               )
               .ThrowsAsync(exception)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var helper = new JsonWebHelperHttpClientForTest(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

            Assert.Same(exception, ex);
            Assert.True(helper.AttemptParseAndThrowExceptionAsyncCalled);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req => req == request),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        // Subclass to override AttemptParseAndThrowExceptionAsync for test verification
        private class JsonWebHelperHttpClientForTest : JsonWebHelperHttpClient
        {
            public bool AttemptParseAndThrowExceptionAsyncCalled { get; private set; }

            public JsonWebHelperHttpClientForTest(HttpClient httpClient) : base(httpClient)
            {
            }

            public override async Task AttemptParseAndThrowExceptionAsync(Exception ex, HttpResponseMessage? response, CancellationToken cancellationToken)
            {
                AttemptParseAndThrowExceptionAsyncCalled = true;
                await Task.CompletedTask;
            }
        }
    }
}
