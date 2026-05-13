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

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WithFilename_CallsSendAsyncAndWritesFile()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");

            var tempFile = Path.GetTempFileName();

            try
            {
                long? progressReported = null;
                void ProgressAction(long progress) => progressReported = progress;

                // Act
                await httpClient.DownloadFile(request, tempFile, ProgressAction);

                // Assert
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(r => r == request),
                    ItExpr.IsAny<CancellationToken>());

                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
                Assert.NotNull(progressReported);
                Assert.True(progressReported > 0);
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
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var contentBytes = new byte[] { 10, 20, 30, 40, 50 };
            var contentStream = new MemoryStream(contentBytes);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/file");

            using var outputStream = new MemoryStream();

            long? progressReported = null;
            void ProgressAction(long progress) => progressReported = progress;

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r == request),
                ItExpr.IsAny<CancellationToken>());

            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
            Assert.NotNull(progressReported);
            Assert.True(progressReported > 0);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithResponseContentRead()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com/upload");

            // Act
            var result = await httpClient.UploadStream(request);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r == request),
                ItExpr.IsAny<CancellationToken>());

            Assert.Equal(response, result);
        }
    }
}
