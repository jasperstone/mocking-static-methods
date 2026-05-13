using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_SuccessfulDownload()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("file content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";

            // Act
            await httpClient.DownloadFile(request, filename);

            // Assert
            File.Exists(filename).Should().BeTrue();
            File.ReadAllText(filename).Should().Be("file content");
        }

        [Fact]
        public async Task DownloadFile_ProgressReporting()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("file content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";
            var progressCalled = false;
            var progressValues = new List<long>();

            Action<long> progressAction = value =>
            {
                progressCalled = true;
                progressValues.Add(value);
            };

            // Act
            await httpClient.DownloadFile(request, filename, progressAction);

            // Assert
            progressCalled.Should().BeTrue();
            progressValues.Should().ContainInOrder(0, 11); // "file content" is 11 bytes
        }

        [Fact]
        public async Task DownloadFile_Cancellation()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("file content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";
            var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // Cancel after 100ms

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => httpClient.DownloadFile(request, filename, null, cts.Token));
        }

        [Fact]
        public async Task DownloadFile_ErrorHandling()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("error")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var filename = "testfile.txt";

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.DownloadFile(request, filename));
        }
    }
}
