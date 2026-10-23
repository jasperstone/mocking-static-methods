using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;
using Moq;
using Moq.Protected;

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var memoryStream = new MemoryStream();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, memoryStream);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
            Assert.Equal("Test content", System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com");

            // Act
            var result = await HttpClientExtensions.UploadStream(httpClient, request);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }
    }
}
