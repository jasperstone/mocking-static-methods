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
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var filename = "example.txt";

            // Act and Assert
            await client.DownloadFile(request, filename);
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-resource");
            var filename = "example.txt";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, filename));
        }

        [Fact]
        public async Task DownloadFile_CancelledRequest_ThrowsTaskCanceledException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var filename = "example.txt";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => client.DownloadFile(request, filename, cancellationToken: cancellationTokenSource.Token));
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingAction_IsCalled()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var filename = "example.txt";
            var progressReportingActionCalled = false;
            Action<long> progressReportingAction = (progress) => progressReportingActionCalled = true;

            // Act
            await client.DownloadFile(request, filename, progressReportingAction);

            // Assert
            Assert.True(progressReportingActionCalled);
        }
    }
}
