using System;
using System.IO;
using System.Net;
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
        public async Task DownloadFile_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<handlerMock>(h => h.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<HttpCompletionOption>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";

            // Act and Assert
            await Assert.DoesNotThrowAsync(async () => await HttpClientExtensions.DownloadFile(client, request, filename));
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<handlerMock>(h => h.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<HttpCompletionOption>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var client = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://invalid-url");
            var filename = "test.txt";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename));
        }

        [Fact]
        public async Task DownloadFile_CancelledRequest_ThrowsOperationCanceledException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<handlerMock>(h => h.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<HttpCompletionOption>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, cancellationToken: cts.Token));
        }
    }
}
