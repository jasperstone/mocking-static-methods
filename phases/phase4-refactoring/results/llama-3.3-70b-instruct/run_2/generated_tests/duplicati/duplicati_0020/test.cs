using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ValidRequest_DownloadsFile()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream())
            };
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>(offset => { });

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, httpRequestMessage, fileStream, progressReportingAction);

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StreamContent(new MemoryStream())
            };
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>(offset => { });

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpClientExtensions.DownloadFile(httpClient, httpRequestMessage, fileStream, progressReportingAction));
        }

        [Fact]
        public async Task UploadStream_ValidRequest_UploadsStream()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            handlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://example.com");
            var stream = new MemoryStream();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClient, httpRequestMessage, cancellationToken);

            // Assert
            handlerMock.Verify(
                h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
            Assert.NotNull(result);
        }
    }
}
