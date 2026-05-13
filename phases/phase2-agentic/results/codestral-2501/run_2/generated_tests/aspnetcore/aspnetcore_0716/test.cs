using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
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
            var mockHttpClient = new Mock<HttpClient>();
            var expectedStream = new MemoryStream();
            mockHttpClient.Setup(client => client.GetStreamAsync(It.IsAny<string>()))
                          .ReturnsAsync(expectedStream);

            var httpClientWrapper = new HttpClientWrapper(mockHttpClient.Object);

            // Act
            var result = await httpClientWrapper.GetStreamAsync("http://example.com");

            // Assert
            Assert.Equal(expectedStream, result);
        }
    }
}
