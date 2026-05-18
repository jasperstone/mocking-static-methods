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
        public async Task GetResponseUncheckedAsync_CallsHttpClientSendAsync_ReturnsResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               // Setup the PROTECTED method SendAsync (which is called by HttpClient.SendAsync)
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
        public async Task GetResponseUncheckedAsync_WhenSendAsyncThrows_AttemptParseAndThrowExceptionAsyncCalledAndExceptionRethrown()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var sendException = new HttpRequestException("Send failed");

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ThrowsAsync(sendException)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // We need to subclass to override AttemptParseAndThrowExceptionAsync to track call
            var helper = new TestJsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

            Assert.Equal(sendException, ex);
            Assert.True(helper.AttemptParseAndThrowExceptionAsyncCalled);
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req => req == request),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        private class TestJsonWebHelperHttpClient : JsonWebHelperHttpClient
        {
            public bool AttemptParseAndThrowExceptionAsyncCalled { get; private set; }

            public TestJsonWebHelperHttpClient(HttpClient httpClient) : base(httpClient)
            {
            }

            protected override Task AttemptParseAndThrowExceptionAsync(Exception ex, HttpResponseMessage? response, CancellationToken cancellationToken)
            {
                AttemptParseAndThrowExceptionAsyncCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}
