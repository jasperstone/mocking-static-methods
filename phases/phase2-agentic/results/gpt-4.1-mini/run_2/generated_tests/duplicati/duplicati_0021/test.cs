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

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            var read = await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes.Length, read);
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

            long totalProgress = 0;
            void ProgressAction(long bytes)
            {
                totalProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, ProgressAction);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());

            Assert.True(totalProgress > 0, "Progress action should be called with bytes transferred");

            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            var read = await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes.Length, read);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithCorrectOptions()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpRequestMessage? capturedRequest = null;
            HttpCompletionOption? capturedOption = null;
            CancellationToken? capturedToken = null;

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
               {
                   capturedRequest = req;
                   capturedToken = token;
               })
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test")
            {
                Content = new StringContent("test content")
            };

            var cts = new CancellationTokenSource();

            // Act
            var result = await httpClient.UploadStream(request, cts.Token);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.IsAny<CancellationToken>());

            Assert.Equal(response, result);
        }
    }
}
