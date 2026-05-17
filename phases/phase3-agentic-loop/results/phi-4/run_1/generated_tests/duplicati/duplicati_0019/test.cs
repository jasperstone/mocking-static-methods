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
            var progressReportingAction = new Action<long>(progress => { });

            // Act
            await httpClient.DownloadFile(request, filename, progressReportingAction);

            // Assert
            Assert.True(File.Exists(filename));
            Assert.Equal("file content", File.ReadAllText(filename));
        }

        [Fact]
        public async Task DownloadFile_NonSuccessStatusCode_ThrowsException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
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

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.DownloadFile(request, "testfile.txt"));
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
            var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // Cancel after 100ms

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => httpClient.DownloadFile(request, "testfile.txt", null, cts.Token));
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
            var progressCalls = 0;
            var progressReportingAction = new Action<long>(progress =>
            {
                progressCalls++;
            });

            // Act
            await httpClient.DownloadFile(request, filename, progressReportingAction);

            // Assert
            Assert.True(progressCalls > 0);
        }
    }
}
