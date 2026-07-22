using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System;

namespace Duplicati.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseAsync_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var helper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await helper.GetResponseAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Assert
            Assert.Equal(responseMessage, result);
            mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
