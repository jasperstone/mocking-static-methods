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

            using var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, null, CancellationToken.None);

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
            var contentBytes = new byte[] { 10, 20, 30, 40, 50 };
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

            using var outputStream = new MemoryStream();

            long reportedProgress = 0;
            void ProgressAction(long bytes)
            {
                reportedProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, ProgressAction, CancellationToken.None);

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

            // The progress action should have been called with a positive number
            Assert.True(reportedProgress > 0);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithCorrectOptions()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpRequestMessage? capturedRequest = null;
            CancellationToken capturedToken = default;
            HttpCompletionOption? capturedOption = null;

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

            var request = new HttpRequestMessage(HttpMethod.Post, "http://upload")
            {
                Content = new StringContent("test content")
            };

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            var result = await httpClient.UploadStream(request, cancellationToken);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req == request),
                ItExpr.Is<CancellationToken>(token => token == cancellationToken));

            Assert.Equal(response, result);
        }
    }
}
