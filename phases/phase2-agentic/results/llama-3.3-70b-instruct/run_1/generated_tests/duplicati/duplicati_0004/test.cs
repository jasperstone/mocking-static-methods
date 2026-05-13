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
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClientMock.Object);

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
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClientMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelperHttpClient.GetResponseAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClientMock.Object);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_InvalidRequest_ThrowsException()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClientMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelperHttpClient.GetResponseUncheckedAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
        }
    }
}
