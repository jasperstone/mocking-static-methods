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
        public async Task DownloadFile_ShouldDownloadFileWithoutProgressReporting()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var fileStream = new MemoryStream();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };

            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream);

            // Assert
            Assert.Equal(5, fileStream.Length);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileWithProgressReporting()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var fileStream = new MemoryStream();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };

            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            long progress = 0;
            void ProgressReportingAction(long p) => progress = p;

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream, ProgressReportingAction);

            // Assert
            Assert.Equal(5, fileStream.Length);
            Assert.Equal(5, progress);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStream()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClientMock.Object, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
