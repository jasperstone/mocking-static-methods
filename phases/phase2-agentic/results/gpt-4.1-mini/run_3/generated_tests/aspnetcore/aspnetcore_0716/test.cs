using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsync_ReturnsStream()
        {
            // Arrange
            var expectedUrl = "http://example.com/test";
            var expectedStream = new MemoryStream();
            var httpClientMock = new Mock<HttpClient>(MockBehavior.Strict);

            // Setup HttpClient.GetStreamAsync to return expectedStream when called with expectedUrl
            httpClientMock
                .Setup(client => client.GetStreamAsync(expectedUrl))
                .ReturnsAsync(expectedStream);

            var wrapper = new HttpClientWrapper(httpClientMock.Object);

            // Act
            var actualStream = await wrapper.GetStreamAsync(expectedUrl);

            // Assert
            Assert.Same(expectedStream, actualStream);
            httpClientMock.Verify(client => client.GetStreamAsync(expectedUrl), Times.Once);
        }
    }
}
