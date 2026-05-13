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
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-resource");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, new MemoryStream()));
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com")
            {
                Content = new StringContent("Hello World")
            };

            // Act and Assert
            await client.UploadStream(request);
        }

        [Fact]
        public async Task UploadStream_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com/non-existent-resource")
            {
                Content = new StringContent("Hello World")
            };

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.UploadStream(request));
        }
    }
}
