using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_SuccessfulDownload()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("file content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

            var tempFile = Path.GetTempFileName();

            // Act
            await httpClient.DownloadFile(request, tempFile);

            // Assert
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.Equal("file content", fileContent);
            File.Delete(tempFile);
        }

        [Fact]
        public async Task DownloadFile_WithProgressReporting()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("file content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

            var progressCalled = false;
            Action<long> progressAction = _ => progressCalled = true;

            var tempFile = Path.GetTempFileName();

            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);

            // Assert
            Assert.True(progressCalled);
            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.Equal("file content", fileContent);
            File.Delete(tempFile);
        }

        [Fact]
        public async Task DownloadFile_ThrowsOnNonSuccessStatusCode()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

            var tempFile = Path.GetTempFileName();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await httpClient.DownloadFile(request, tempFile));
        }
    }
}
