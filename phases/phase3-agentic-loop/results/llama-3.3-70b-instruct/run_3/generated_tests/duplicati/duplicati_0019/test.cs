using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "testfile.txt";
            var progressReportingAction = new Action<long>((bytes) => { });

            // Act and Assert
            await client.DownloadFile(request, filename, progressReportingAction);
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/non-existent-resource");
            var filename = "testfile.txt";
            var progressReportingAction = new Action<long>((bytes) => { });

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, filename, progressReportingAction));
        }

        [Fact]
        public async Task DownloadFile_CancellationToken_CancelsDownload()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "testfile.txt";
            var progressReportingAction = new Action<long>((bytes) => { });
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => client.DownloadFile(request, filename, progressReportingAction, cancellationTokenSource.Token));
        }
    }
}
