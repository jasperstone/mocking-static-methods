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

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>(offset => { });
            var cancellationToken = new CancellationToken();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };

            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream, progressReportingAction, cancellationToken);

            // Assert
            Assert.Equal(5, fileStream.Length);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com")
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            var cancellationToken = new CancellationToken();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            var response = await HttpClientExtensions.UploadStream(httpClientMock.Object, request, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
