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
            var httpClientMock = new Mock<HttpClient>();
            var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetStreamAsync(url);

            // Assert
            httpClientMock.Verify(c => c.GetStreamAsync(url), Times.Once);
        }

        [Fact]
        public async Task GetStreamAsync_ReturnsStream()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var stream = new MemoryStream();
            httpClientMock.Setup(c => c.GetStreamAsync(It.IsAny<string>())).ReturnsAsync(stream);
            var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
            var url = "https://example.com";

            // Act
            var result = await httpClientWrapper.GetStreamAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Stream>(result);
        }

        [Fact]
        public async Task GetResponseAsync_CallsHttpClientGetAsync()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
            var url = "https://example.com";

            // Act
            await httpClientWrapper.GetResponseAsync(url);

            // Assert
            httpClientMock.Verify(c => c.GetAsync(url), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_ReturnsHttpResponseMessageWrapper()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage();
            httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            var httpClientWrapper = new HttpClientWrapper(httpClientMock.Object);
            var url = "https://example.com";

            // Act
            var result = await httpClientWrapper.GetResponseAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IHttpResponseMessageWrapper>(result);
        }
    }
}
