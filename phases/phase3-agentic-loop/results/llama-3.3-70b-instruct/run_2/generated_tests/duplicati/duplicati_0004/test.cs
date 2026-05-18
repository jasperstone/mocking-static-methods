using Duplicati.Library;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseAsync_InvalidRequest_ThrowsException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelperHttpClient.GetResponseAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
