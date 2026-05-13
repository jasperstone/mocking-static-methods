using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_InvokesHttpClientGetStreamAsync_ReturnsExpectedStream()
        {
            // Arrange
            var expectedContent = "Hello, world!";
            var expectedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(expectedContent));

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StreamContent(expectedStream)
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var wrapper = new HttpClientWrapper(httpClient);

            // Act
            var stream = await wrapper.GetStreamAsync("http://example.com");

            // Assert
            Assert.NotNull(stream);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal(expectedContent, content);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == new Uri("http://example.com")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
