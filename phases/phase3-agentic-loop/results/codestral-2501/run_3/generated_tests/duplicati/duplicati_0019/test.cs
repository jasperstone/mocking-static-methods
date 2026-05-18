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

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var filePath = Path.GetTempFileName();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = await File.ReadAllTextAsync(filePath);
            Assert.Equal("Test content", fileContent);

            // Clean up
            File.Delete(filePath);
        }

        [Fact]
        public async Task DownloadFile_ShouldReportProgress()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var progressReported = false;
            Action<long> progressAction = (progress) =>
            {
                progressReported = true;
            };

            var filePath = Path.GetTempFileName();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath, progressAction);

            // Assert
            Assert.True(progressReported);

            // Clean up
            File.Delete(filePath);
        }
    }
}
