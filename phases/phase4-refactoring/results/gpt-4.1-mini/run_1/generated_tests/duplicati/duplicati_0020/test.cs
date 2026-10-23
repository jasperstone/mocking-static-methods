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
        public async Task DownloadFile_WithFilename_ProgressAction_CopiesContentAndReportsProgress()
        {
            // Arrange
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(() =>
               {
                   var response = new HttpResponseMessage(HttpStatusCode.OK)
                   {
                       Content = new ByteArrayContent(contentBytes)
                   };
                   return response;
               })
               .Verifiable();

            var client = new HttpClient(handlerMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                long lastProgress = -1;
                void Progress(long bytes) => lastProgress = bytes;

                var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

                // Act
                await client.DownloadFile(request, tempFile, Progress);

                // Assert
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes.Length, fileBytes.Length);
                Assert.Equal(contentBytes, fileBytes);
                Assert.True(lastProgress > 0);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_NoProgressAction_CopiesContent()
        {
            // Arrange
            var contentBytes = new byte[50];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)(255 - i);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(() =>
               {
                   var response = new HttpResponseMessage(HttpStatusCode.OK)
                   {
                       Content = new ByteArrayContent(contentBytes)
                   };
                   return response;
               })
               .Verifiable();

            var client = new HttpClient(handlerMock.Object);

            using var outputStream = new MemoryStream();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            await client.DownloadFile(request, outputStream);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes.Length, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsyncWithCorrectOptions()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(expectedResponse)
               .Verifiable();

            var client = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test")
            {
                Content = new StringContent("test content")
            };

            // Act
            var response = await client.UploadStream(request);

            // Assert
            Assert.Equal(expectedResponse, response);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
