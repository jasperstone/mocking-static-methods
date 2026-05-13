using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
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
            var progressReportingAction = new Mock<Action<long>>();
            var cancellationToken = new CancellationToken();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                .ReturnsAsync(response);

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream, progressReportingAction.Object, cancellationToken);

            // Assert
            Assert.Equal(5, fileStream.Length);
            progressReportingAction.Verify(action => action(It.IsAny<long>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var cancellationToken = new CancellationToken();

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseContentRead, cancellationToken))
                .ReturnsAsync(response);

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClientMock.Object, request, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
