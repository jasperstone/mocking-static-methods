using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
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
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, filename);
            }
            catch (Exception ex)
            {
                Assert.Fail("DownloadFile threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_Throws()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-resource");
            var filename = "example.txt";

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, filename);
                Assert.Fail("DownloadFile did not throw an exception");
            }
            catch (HttpRequestException)
            {
                // Expected
            }
        }

        [Fact]
        public async Task DownloadFile_ValidRequestWithProgressReporting_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var filename = "example.txt";
            Action<long> progressReportingAction = (progress) => { };

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);
            }
            catch (Exception ex)
            {
                Assert.Fail("DownloadFile threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task DownloadFile_InvalidRequestWithProgressReporting_Throws()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-resource");
            var filename = "example.txt";
            Action<long> progressReportingAction = (progress) => { };

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);
                Assert.Fail("DownloadFile did not throw an exception");
            }
            catch (HttpRequestException)
            {
                // Expected
            }
        }

        [Fact]
        public async Task DownloadFile_ValidRequestWithFileStream_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com");
            var fileStream = new MemoryStream();

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, fileStream);
            }
            catch (Exception ex)
            {
                Assert.Fail("DownloadFile threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task DownloadFile_InvalidRequestWithFileStream_Throws()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/non-existent-resource");
            var fileStream = new MemoryStream();

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, fileStream);
                Assert.Fail("DownloadFile did not throw an exception");
            }
            catch (HttpRequestException)
            {
                // Expected
            }
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com");
            request.Content = new StringContent("example content");

            // Act and Assert
            try
            {
                await HttpClientExtensions.UploadStream(client, request);
            }
            catch (Exception ex)
            {
                Assert.Fail("UploadStream threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task UploadStream_InvalidRequest_Throws()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.example.com/non-existent-resource");
            request.Content = new StringContent("example content");

            // Act and Assert
            try
            {
                await HttpClientExtensions.UploadStream(client, request);
                Assert.Fail("UploadStream did not throw an exception");
            }
            catch (HttpRequestException)
            {
                // Expected
            }
        }
    }
}
