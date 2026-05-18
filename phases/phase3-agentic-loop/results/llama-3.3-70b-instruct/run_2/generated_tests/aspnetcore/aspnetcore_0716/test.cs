using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;

namespace Microsoft.DotNet.Openapi.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage());
            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetStreamAsync(url);

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once,
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task GetResponseAsync_CallsHttpClientGetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage());
            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetResponseAsync(url);

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once,
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public void Dispose_CallsHttpClientDispose()
        {
            // Arrange
            var httpClient = new HttpClient();
            var httpClientWrapper = new HttpClientWrapper(httpClient);

            // Act
            httpClientWrapper.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => httpClient.Dispose());
        }
    }
}
