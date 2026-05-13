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
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(expectedStream)
                    };
                    return await Task.FromResult(response);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var wrapper = new HttpClientWrapper(httpClient);

            var testUrl = "http://test";

            // Act
            var stream = await wrapper.GetStreamAsync(testUrl);

            // Assert
            Assert.NotNull(stream);
            Assert.Equal(expectedStream, stream);
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
        }
    }
}
