using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WithFilename_CallsSendAsyncAndWritesFile()
        {
            // Arrange
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);
            var httpContent = new StreamContent(contentStream);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContent
            };

            var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Setup SendAsync to return the response
            mockHttpClient
                .Setup(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await mockHttpClient.Object.DownloadFile(request, tempFile);

                // Assert
                mockHttpClient.Verify(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);

                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_CallsSendAsyncAndWritesStream()
        {
            // Arrange
            var contentBytes = new byte[] { 10, 20, 30, 40, 50 };
            var contentStream = new MemoryStream(contentBytes);
            var httpContent = new StreamContent(contentStream);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContent
            };

            var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            mockHttpClient
                .Setup(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            using var outputStream = new MemoryStream();

            // Act
            await mockHttpClient.Object.DownloadFile(request, outputStream);

            // Assert
            mockHttpClient.Verify(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);

            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task DownloadFile_WithProgressAction_ReportsProgress()
        {
            // Arrange
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;
            var contentStream = new MemoryStream(contentBytes);
            var httpContent = new StreamContent(contentStream);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContent
            };

            var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            mockHttpClient
                .Setup(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var progressReports = new System.Collections.Generic.List<long>();
            void ProgressAction(long bytes) => progressReports.Add(bytes);

            using var outputStream = new MemoryStream();

            // Act
            await mockHttpClient.Object.DownloadFile(request, outputStream, ProgressAction);

            // Assert
            mockHttpClient.Verify(c => c.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotEmpty(progressReports);
            Assert.Equal(contentBytes.Length, progressReports[^1]);
            Assert.Equal(contentBytes, outputStream.ToArray());
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpClient
                .Setup(c => c.SendAsync(request, HttpCompletionOption.ResponseContentRead, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await mockHttpClient.Object.UploadStream(request);

            // Assert
            mockHttpClient.Verify(c => c.SendAsync(request, HttpCompletionOption.ResponseContentRead, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(response, result);
        }
    }
}
