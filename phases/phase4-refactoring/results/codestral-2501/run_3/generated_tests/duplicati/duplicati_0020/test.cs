using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToStream()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>(_ => { });
            var cancellationToken = new CancellationToken();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, fileStream, progressReportingAction, cancellationToken);

            // Assert
            fileStream.Seek(0, SeekOrigin.Begin);
            var result = new byte[fileStream.Length];
            await fileStream.ReadAsync(result, 0, (int)fileStream.Length);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToFile()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var filename = Path.GetTempFileName();
            var progressReportingAction = new Action<long>(_ => { });
            var cancellationToken = new CancellationToken();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, filename, progressReportingAction, cancellationToken);

            // Assert
            var result = await File.ReadAllBytesAsync(filename);
            Assert.NotEmpty(result);
            File.Delete(filename);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStream()
        {
            // Arrange
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var cancellationToken = new CancellationToken();

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClient, request, cancellationToken);

            // Assert
            Assert.NotNull(result);
        }
    }
}
