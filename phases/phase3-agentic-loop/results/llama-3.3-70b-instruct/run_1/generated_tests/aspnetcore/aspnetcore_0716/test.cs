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
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<HttpCompletionOption>()
               )
               .ReturnsAsync(new HttpResponseMessage());
            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetStreamAsync(url);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once,
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                ItExpr.IsAny<HttpCompletionOption>());
        }

        [Fact]
        public async Task GetStreamAsync_ThrowsHttpRequestException_WhenHttpClientGetStreamAsyncFails()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<HttpCompletionOption>()
               )
               .Throws(new HttpRequestException("Test exception"));
            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);
            var url = "https://example.com";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => httpClientWrapper.GetStreamAsync(url));
        }
    }
}
