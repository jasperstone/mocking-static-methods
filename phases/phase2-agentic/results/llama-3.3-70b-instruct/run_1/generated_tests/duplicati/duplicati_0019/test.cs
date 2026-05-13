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
            await Assert.DoesNotThrowAsync(async () => await HttpClientExtensions.DownloadFile(client, request, filename));
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/non-existent-resource");
            var filename = "test.txt";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename));
        }

        [Fact]
        public async Task DownloadFile_CancellationToken_CancelsRequest()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var cts = new CancellationTokenSource(100);

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, cancellationToken: cts.Token));
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
            await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction: (bytesTransferred) => progressCalled = true);

            // Assert
            Assert.True(progressCalled);
        }
    }
}
