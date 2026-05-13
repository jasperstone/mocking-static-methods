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
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/non-existent-resource");
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
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => client.DownloadFile(request, filename, cancellationToken: cancellationTokenSource.Token));
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingAction_IsCalled()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressReportingActionCalled = false;
            Action<long> progressReportingAction = (bytesTransferred) => progressReportingActionCalled = true;

            // Act
            await client.DownloadFile(request, filename, progressReportingAction);

            // Assert
            Assert.True(progressReportingActionCalled);
        }
    }
}
