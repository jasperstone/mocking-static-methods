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

            try
            {
                await client.DownloadFile(request, filename);
                Assert.False(true, "Expected HttpRequestException to be thrown");
            }
            catch (HttpRequestException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public async Task DownloadFile_Stream_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var stream = new MemoryStream();

            // Act and Assert
            await client.DownloadFile(request, stream);
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com");
            var stream = new MemoryStream();

            // Act and Assert
            await client.UploadStream(request);
        }
    }
}
