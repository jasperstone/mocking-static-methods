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
                .Returns<HttpRequestMessage>(req =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(expectedStream)
                    };
                    return response;
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var wrapper = new HttpClientWrapper(httpClient);
            var url = "http://test";

            // Act
            var stream = await wrapper.GetStreamAsync(url);

            // Assert
            Assert.NotNull(stream);
            Assert.IsType<MemoryStream>(stream);
            // Optionally, verify that the underlying HttpClient's GetStreamAsync was called
            // but since it's a non-virtual method, we rely on the mock setup
        }
    }
}
