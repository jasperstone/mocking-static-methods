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
        public async Task DownloadFile_Stream_NoProgress_CopiesStream()
        {
            // Arrange
            var expectedContent = "Hello, world!";
            var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(expectedContent));
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
            await httpClient.DownloadFile(request, outputStream);

            // Assert
            outputStream.Position = 0;
            using var reader = new StreamReader(outputStream);
            var result = await reader.ReadToEndAsync();
            Assert.Equal(expectedContent, result);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
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

            long reportedProgress = 0;
            void ProgressAction(long bytes)
            {
                reportedProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction);

            // Assert
            Assert.True(reportedProgress > 0);
            outputStream.Position = 0;
            var outputBytes = outputStream.ToArray();
            Assert.Equal(contentBytes.Length, outputBytes.Length);
            Assert.Equal(contentBytes, outputBytes);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

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

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");

            // Act
            var result = await httpClient.UploadStream(request);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
