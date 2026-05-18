using Microsoft.DotNet.OpenApi;
using Microsoft.DotNet.OpenApi.Tools;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetStreamAsync(url);

            // Assert
            httpClientHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once,
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetStreamAsync_ThrowsHttpRequestException_WhenHttpClientGetStreamAsyncFails()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            var exception = new HttpRequestException("Test exception");
            httpClientHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).Throws(exception);
            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => httpClientWrapper.GetStreamAsync(url));
        }
    }
}
