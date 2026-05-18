using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Microsoft.DotNet.OpenApi;
using Moq;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_ShouldReturnStream()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream())
            };
            mockHttpMessageHandler
                .Setup(_ => _.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientWrapper = new HttpClientWrapper(httpClient);

            // Act
            var result = await httpClientWrapper.GetStreamAsync("http://example.com");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Stream>(result);
        }
    }
}
