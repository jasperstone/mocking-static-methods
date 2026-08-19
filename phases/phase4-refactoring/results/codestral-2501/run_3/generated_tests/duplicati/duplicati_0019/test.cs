using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var tempFilePath = Path.GetTempFileName();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, tempFilePath);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

            var fileContent = await File.ReadAllTextAsync(tempFilePath);
            Assert.Equal("Test content", fileContent);

            // Clean up
            File.Delete(tempFilePath);
        }
    }
}
