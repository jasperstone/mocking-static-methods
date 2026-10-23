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
            await HttpClientExtensions.DownloadFile(client, request, fileStream);
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com");
            request.Content = new StringContent("Test content");

            // Act and Assert
            await HttpClientExtensions.UploadStream(client, request);
        }
    }
}
