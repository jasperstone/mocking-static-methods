using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WithProgressReportingAction_ShouldCallProgressAction()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://example.com") // Ensure using System.Uri
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "/testfile");
            var filename = "testfile.txt";
            var progressCalled = false;

            Action<long> progressAction = progress =>
            {
                progressCalled = true;
            };

            // Act
            await client.DownloadFile(request, filename, progressAction);

            // Assert
            Assert.True(progressCalled);
        }

        [Fact]
        public async Task DownloadFile_WithoutProgressReportingAction_ShouldDownloadFile()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            };
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://example.com") // Ensure using System.Uri
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "/testfile");
            var filename = "testfile.txt";

            // Act
            await client.DownloadFile(request, filename);

            // Assert
            using var fileStream = System.IO.File.OpenRead(filename);
            var fileContent = new byte[5];
            await fileStream.ReadAsync(fileContent, 0, fileContent.Length);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, fileContent);
        }
    }
}
