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
        public async Task DownloadFile_CallsSendAsync_WithExpectedParameters()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, tempFile);

                // Assert
                handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
                Assert.True(File.Exists(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsProgressAction()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var data = new byte[] { 10, 20, 30, 40, 50 };
            var responseContent = new ByteArrayContent(data);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var tempFile = Path.GetTempFileName();
            long totalBytesRead = 0;
            Action<long> progressAction = (long bytesRead) => { totalBytesRead = bytesRead; };

            try
            {
                // Act
                await httpClient.DownloadFile(request, tempFile, progressAction);

                // Assert
                Assert.True(File.Exists(tempFile));
                Assert.Equal(data.Length, totalBytesRead);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStreamParameter_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var data = new byte[] { 5, 6, 7, 8 };
            var responseContent = new ByteArrayContent(data);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            using var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, outputStream);

            // Assert
            handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(data.Length, outputStream.Length);
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
            handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseContentRead, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
