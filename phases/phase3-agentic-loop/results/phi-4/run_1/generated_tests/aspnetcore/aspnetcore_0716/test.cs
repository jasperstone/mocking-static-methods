using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using System.Threading;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsyncWithCorrectUrl()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockResponse = new HttpResponseMessage();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://example.com"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var wrapper = new HttpClientWrapper(client);

            var url = "http://example.com";

            // Act
            await wrapper.GetStreamAsync(url);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
