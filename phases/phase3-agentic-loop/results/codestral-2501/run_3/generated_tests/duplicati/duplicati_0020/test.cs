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
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var filePath = Path.GetTempFileName();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath, null, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = await File.ReadAllBytesAsync(filePath);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileWithProgressReporting()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var filePath = Path.GetTempFileName();
            long progress = 0;
            Action<long> progressReportingAction = (p) => progress = p;

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filePath, progressReportingAction, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = await File.ReadAllBytesAsync(filePath);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
            Assert.Equal(5, progress);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToStreamSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            using var memoryStream = new MemoryStream();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, memoryStream, null, CancellationToken.None);

            // Assert
            var fileContent = memoryStream.ToArray();
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToStreamWithProgressReporting()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            using var memoryStream = new MemoryStream();
            long progress = 0;
            Action<long> progressReportingAction = (p) => progress = p;

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, memoryStream, progressReportingAction, CancellationToken.None);

            // Assert
            var fileContent = memoryStream.ToArray();
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
            Assert.Equal(5, progress);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClientMock.Object, request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
