using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            mockHttpClient
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var response = await jsonWebHelperHttpClient.GetResponseAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            mockHttpClient
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(mockHttpClient.Object);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
