using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.DotNet.OpenApi.Tools;

namespace Microsoft.DotNet.Openapi.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsync()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var testUrl = "http://test.com/stream";

            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockHttpClient
                .Setup(c => c.GetStreamAsync(testUrl))
                .ReturnsAsync(expectedStream);

            var wrapper = new HttpClientWrapper(mockHttpClient.Object);

            // Act
            var resultStream = await wrapper.GetStreamAsync(testUrl);

            // Assert
            Assert.NotNull(resultStream);
            Assert.Equal(expectedStream, resultStream);
            mockHttpClient.Verify(c => c.GetStreamAsync(testUrl), Times.Once);
        }
    }
}
