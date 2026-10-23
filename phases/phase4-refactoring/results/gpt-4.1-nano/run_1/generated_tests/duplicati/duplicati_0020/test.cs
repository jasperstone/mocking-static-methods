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
        public async Task DownloadFile_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, filename);

                // Assert
                handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
                Assert.True(File.Exists(filename));
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsSendAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new MemoryStream(new byte[] { 1, 2, 3, 4 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContent)
            };

            handlerMock
               .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var filename = Path.GetTempFileName();
            var progressCalled = false;

            try
            {
                // Act
                await httpClient.DownloadFile(request, filename, progress =>
                {
                    progressCalled = true;
                });

                // Assert
                handlerMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req => req == request), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
                Assert.True(File.Exists(filename));
                Assert.True(progressCalled);
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }
    }
}
