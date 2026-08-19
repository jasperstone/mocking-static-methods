using System;
using System.IO;
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
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, filename);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected no exception, but got {ex.Message}");
            }
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
        public async Task DownloadFile_CancelledRequest_ThrowsTaskCanceledException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, cancellationToken: cancellationTokenSource.Token));
        }
    }
}
