using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Duplicati.Library.Utility;

namespace Duplicati.Tests.Library.Utility
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
                // Act
                await httpClient.DownloadFile(request, tempFile);

                // Assert
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());

                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStreamAndProgress_CallsSendAsyncAndReportsProgress()
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
            var outputStream = new MemoryStream();

            long totalProgress = 0;
            void ProgressAction(long bytes)
            {
                totalProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
            Assert.True(totalProgress > 0);
        }
    }
}
