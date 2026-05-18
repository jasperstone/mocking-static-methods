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
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
