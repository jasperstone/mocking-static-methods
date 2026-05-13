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
            var fileStream = new MemoryStream();

            // Act and Assert
            await client.DownloadFile(request, fileStream);
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com");
            request.Content = new StringContent("Hello World");

            // Act and Assert
            var response = await client.UploadStream(request);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-page");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, new MemoryStream()));
        }

        [Fact]
        public async Task UploadStream_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com/non-existent-page");
            request.Content = new StringContent("Hello World");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.UploadStream(request));
        }
    }
}
