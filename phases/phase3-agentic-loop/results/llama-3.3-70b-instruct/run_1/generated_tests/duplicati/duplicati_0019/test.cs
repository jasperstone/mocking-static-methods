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
            var filename = "test.txt";

            // Act and Assert
            await client.DownloadFile(request, filename);
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://non-existent-url.com");
            var filename = "test.txt";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, filename));
        }

        [Fact]
        public async Task DownloadFile_CancelationToken_CancelsDownload()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var cts = new CancellationTokenSource();

            // Act and Assert
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => client.DownloadFile(request, filename, cancellationToken: cts.Token));
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingAction_IsCalled()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressCalled = false;

            // Act
            await client.DownloadFile(request, filename, progressReportingAction: (long progress) => progressCalled = true);

            // Assert
            Assert.True(progressCalled);
        }
    }
}
