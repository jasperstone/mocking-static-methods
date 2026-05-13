using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Moq;
using Moq.Protected;
using Xunit;

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToStream()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            var fileStream = new MemoryStream();

            httpClientMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, fileStream, null, CancellationToken.None);

            // Assert
            fileStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("Test content", content);
        }

        [Fact]
        public async Task DownloadFile_ShouldDownloadFileToFile()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            var tempFilePath = Path.GetTempFileName();

            httpClientMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            await HttpClientExtensions.DownloadFile(httpClientMock.Object, request, tempFilePath, null, CancellationToken.None);

            // Assert
            var fileContent = await File.ReadAllTextAsync(tempFilePath);
            Assert.Equal("Test content", fileContent);

            // Clean up
            File.Delete(tempFilePath);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStream()
        {
            // Arrange
            var httpClientMock = new Mock<HttpClient>();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClientMock.Object, request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
