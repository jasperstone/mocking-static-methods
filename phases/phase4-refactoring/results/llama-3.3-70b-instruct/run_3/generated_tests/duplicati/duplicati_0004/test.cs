using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var httpClient = new HttpClient(handlerMock.Object);
            var jsonWebHelperHttpClient = new JsonWebHelperHttpClient(httpClient);

            // Act
            var response = await jsonWebHelperHttpClient.GetResponseUncheckedAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
