using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library.Utility;

namespace Duplicati.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();
            long progressReported = 0;
            Action<long> progressAction = (long val) => progressReported = val;

            // Act
            await httpClient.DownloadFile(request, tempFile, progressAction);

            // Assert
            Assert.True(progressReported > 0);
            Assert.True(File.Exists(tempFile));
            File.Delete(tempFile);
        }

        [Fact]
        public async Task DownloadFile_WithStreamParameter_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new MemoryStream(new byte[] { 10, 20, 30 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            using var outputStream = new MemoryStream();
            long progressReported = 0;
            Action<long> progressAction = (long val) => progressReported = val;

            // Act
            await httpClient.DownloadFile(request, outputStream, progressAction);

            // Assert
            Assert.True(progressReported > 0);
            Assert.True(outputStream.Length > 0);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseContentRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var response = await httpClient.UploadStream(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            handlerMock.Verify();
        }
    }
}
