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
        public async Task DownloadFile_Stream_NoProgress_CopiesStream()
        {
            // Arrange
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var outputStream = new MemoryStream();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            await httpClient.DownloadFile(request, outputStream, null, CancellationToken.None);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task DownloadFile_Stream_WithProgress_CallsProgressAction()
        {
            // Arrange
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;
            var contentStream = new MemoryStream(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var outputStream = new MemoryStream();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            long totalProgress = 0;
            void ProgressAction(long bytes)
            {
                totalProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction, CancellationToken.None);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            Assert.True(totalProgress > 0);
            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(expectedResponse)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");

            // Act
            var response = await httpClient.UploadStream(request, CancellationToken.None);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());

            Assert.Equal(expectedResponse, response);
        }
    }
}
