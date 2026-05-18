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
        public async Task DownloadFile_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var memoryStream = new MemoryStream();

            // Act
            await HttpClientExtensions.DownloadFile(httpClient, request, memoryStream);

            // Assert
            memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStream);
            var content = reader.ReadToEnd();
            Assert.Equal("Test content", content);
        }

        [Fact]
        public async Task UploadStream_ShouldUploadStreamSuccessfully()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com")
            {
                Content = new StringContent("Test content")
            };

            // Act
            var response = await HttpClientExtensions.UploadStream(httpClient, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
