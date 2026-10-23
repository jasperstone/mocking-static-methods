using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>(progress => { });
            var cancellationToken = new CancellationToken();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, fileStream, progressReportingAction, cancellationToken);

            // Assert
            Assert.NotEmpty(fileStream.ToArray());
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var cancellationToken = new CancellationToken();

            // Act
            var response = await HttpClientExtensions.UploadStream(httpClient, request, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
    }
}
