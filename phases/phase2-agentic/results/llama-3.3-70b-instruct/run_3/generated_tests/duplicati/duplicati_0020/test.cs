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
            await Assert.DoesNotThrowAsync(async () => await client.DownloadFile(request, filename));
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://invalid.example.com");
            var filename = "test.txt";

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await client.DownloadFile(request, filename));
        }

        [Fact]
        public async Task DownloadFile_Stream_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var stream = new MemoryStream();

            // Act and Assert
            await Assert.DoesNotThrowAsync(async () => await client.DownloadFile(request, stream));
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");
            request.Content = new StringContent("test");

            // Act and Assert
            await Assert.DoesNotThrowAsync(async () => await client.UploadStream(request));
        }

        [Fact]
        public async Task UploadStream_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://invalid.example.com");
            request.Content = new StringContent("test");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await client.UploadStream(request));
        }
    }
}
