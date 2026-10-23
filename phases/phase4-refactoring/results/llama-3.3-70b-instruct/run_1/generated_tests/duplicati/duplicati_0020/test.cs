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
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressReportingAction = new Action<long>((progress) => { });

            // Act and Assert
            await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_Throws()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/non-existent-resource");
            var filename = "test.txt";
            var progressReportingAction = new Action<long>((progress) => { });

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction));
        }

        [Fact]
        public async Task DownloadFile_ValidRequestWithProgressReporting_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressReportingAction = new Action<long>((progress) => { });

            // Act and Assert
            await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);
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
            await Assert.DoesNotThrowAsync(async () => await HttpClientExtensions.UploadStream(client, request, cancellationToken));
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingActionIsCalled()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var filename = "test.txt";
            var progressReportingActionMock = new Mock<Action<long>>();
            var progressReportingAction = progressReportingActionMock.Object;

            // Act
            await HttpClientExtensions.DownloadFile(client, request, filename, progressReportingAction);

            // Assert
            progressReportingActionMock.Verify(action => action(It.IsAny<long>()), Times.AtLeastOnce);
        }
    }
}
