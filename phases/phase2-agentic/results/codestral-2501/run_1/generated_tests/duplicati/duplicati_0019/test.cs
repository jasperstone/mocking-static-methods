using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var filename = "testfile.txt";
            var progressReportingAction = new Mock<Action<long>>();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filename, progressReportingAction.Object, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(filename));
            File.Delete(filename);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToStreamSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var memoryStream = new MemoryStream();
            var progressReportingAction = new Mock<Action<long>>();

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, memoryStream, progressReportingAction.Object, CancellationToken.None);

            // Assert
            Assert.Equal(5, memoryStream.Length);
        }

        [Fact]
        public async Task DownloadFile_ShouldThrowExceptionOnFailedRequest()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var filename = "testfile.txt";
            var progressReportingAction = new Mock<Action<long>>();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => HttpClientExtensions.DownloadFile(httpClientMock.Object, request, filename, progressReportingAction.Object, CancellationToken.None));
        }
    }
}
