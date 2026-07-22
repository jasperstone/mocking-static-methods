using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;

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
            var progressReportingAction = new Action<long>((bytes) => { });

            // Act and Assert
            await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://invalid-url");
            var filename = "test.txt";
            var progressReportingAction = new Action<long>((bytes) => { });

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction));
        }

        [Fact]
        public async Task DownloadFile_CancelledRequest_ThrowsTaskCanceledException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressReportingAction = new Action<long>((bytes) => { });
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");
            var stream = new MemoryStream();
            request.Content = new StreamContent(stream);
            var cancellationToken = new CancellationToken();

            // Act and Assert
            await HttpClientExtensions.UploadStream(client, request, cancellationToken);
        }
    }
}
