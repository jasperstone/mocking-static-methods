using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class JSONWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_SuccessfulResponse_ReturnsResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("response content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act
            var response = await jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("response content", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ThrowsException_ThrowsException()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var jsonWebHelper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => jsonWebHelper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None));
        }
    }
}
