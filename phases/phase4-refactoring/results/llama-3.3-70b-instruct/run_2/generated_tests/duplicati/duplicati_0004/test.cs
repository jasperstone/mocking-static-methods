using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(handlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(handlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
