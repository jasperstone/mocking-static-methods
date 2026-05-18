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
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;
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

            long lastProgress = -1;
            void ProgressAction(long progress) => lastProgress = progress;

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            Assert.Equal(contentBytes.Length, lastProgress);
            var outputBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, outputBytes);
        }

        [Fact]
        public async Task DownloadFile_WithStreamWithoutProgress_CallsSendAsyncAndWritesStream()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var contentBytes = new byte[] { 10, 20, 30, 40 };
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

            // Act
            await httpClient.DownloadFile(request, outputStream);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            var outputBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, outputBytes);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithCorrectOption()
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
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());

            Assert.Equal(response, result);
        }
    }
}
