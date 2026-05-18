using System;
using System.IO;
using System.Net;
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
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Test content")
                });

            httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, filename);

            // Assert
            Assert.True(File.Exists(filename));
            var content = await File.ReadAllTextAsync(filename);
            Assert.Equal("Test content", content);
        }

        [Fact]
        public async Task DownloadFile_ShouldThrowExceptionOnFailure()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpClientExtensions.DownloadFile(httpClient, request, filename));
        }
    }
}
