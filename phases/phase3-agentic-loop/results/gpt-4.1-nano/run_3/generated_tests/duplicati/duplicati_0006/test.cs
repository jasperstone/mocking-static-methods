using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Duplicati.Library;
using Moq;

namespace Duplicati.Tests
{
    public class OAuthHttpClientTests
    {
        private class DummyAuthenticator : OAuthHttpMessageHandler
        {
            public override HttpRequestMessage PreventAuthentication(HttpRequestMessage request)
            {
                // For testing, just return the request
                return request;
            }
        }

        [Fact]
        public async Task GetAsync_Should_Throw_TimeoutException_On_OperationCanceledException()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object);
            client.Timeout = TimeSpan.FromSeconds(1);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.GetAsync("http://test");
            });
        }

        [Fact]
        public async Task SendAsync_Should_Call_SendAsync_With_Correct_Params_And_Handle_Timeout()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var client = new OAuthHttpClient(mockHandler.Object);
            client.Timeout = TimeSpan.FromSeconds(1);

            // Act
            var response = await client.SendAsync(request, true, cts.Token);

            // Assert
            Assert.Equal(responseMessage, response);
            mockHandler.Verify(h => h.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_Should_Throw_TimeoutException_When_OperationCanceledException_Without_CancellationRequested()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object);
            client.Timeout = TimeSpan.FromSeconds(1);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.SendAsync(request, true, cts.Token);
            });
        }
    }
}
