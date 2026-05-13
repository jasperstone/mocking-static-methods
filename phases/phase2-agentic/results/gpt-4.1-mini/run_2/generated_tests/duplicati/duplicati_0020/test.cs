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
            var httpContentMock = new Mock<HttpContent>();
            httpContentMock.Setup(c => c.ReadAsStreamAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contentStream);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContentMock.Object
            };

            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Setup SendAsync to return the prepared response
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var tempFile = Path.GetTempFileName();

            try
            {
                long reportedProgress = 0;
                void ProgressAction(long progress) => reportedProgress = progress;

                // Act
                await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, tempFile, ProgressAction);

                // Assert
                httpClientMock
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(r => r == request),
                        ItExpr.Is<HttpCompletionOption>(o => o == HttpCompletionOption.ResponseHeadersRead),
                        ItExpr.IsAny<CancellationToken>());

                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
                Assert.True(reportedProgress > 0);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_CallsSendAsyncAndWritesToStream()
        {
            // Arrange
            var contentBytes = new byte[] { 10, 20, 30, 40, 50 };
            var contentStream = new MemoryStream(contentBytes);
            var httpContentMock = new Mock<HttpContent>();
            httpContentMock.Setup(c => c.ReadAsStreamAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contentStream);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = httpContentMock.Object
            };

            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            using var outputStream = new MemoryStream();

            long reportedProgress = 0;
            void ProgressAction(long progress) => reportedProgress = progress;

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, outputStream, ProgressAction);

            // Assert
            httpClientMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(r => r == request),
                    ItExpr.Is<HttpCompletionOption>(o => o == HttpCompletionOption.ResponseHeadersRead),
                    ItExpr.IsAny<CancellationToken>());

            var writtenBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, writtenBytes);
            Assert.True(reportedProgress > 0);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClientMock.Object, request);

            // Assert
            httpClientMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(r => r == request),
                    ItExpr.Is<HttpCompletionOption>(o => o == HttpCompletionOption.ResponseContentRead),
                    ItExpr.IsAny<CancellationToken>());

            Assert.Equal(response, result);
        }
    }
}
