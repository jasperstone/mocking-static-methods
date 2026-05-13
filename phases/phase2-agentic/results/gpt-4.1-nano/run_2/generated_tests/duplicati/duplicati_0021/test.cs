using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_Should_Call_SendAsync_With_Correct_Parameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(responseContent)
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, outputStream);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());
            Assert.True(outputStream.Length > 0);
        }
    }
}
