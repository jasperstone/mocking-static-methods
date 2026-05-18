using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Duplicati.Library.Utility;

namespace Duplicati.Tests.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WithProgressReporting_CallsSendAsyncAndCopiesStream()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContentStream)
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var outputStream = new MemoryStream();
            bool progressReported = false;
            Action<long> progressAction = (long progress) => { progressReported = true; };

            // Act
            await httpClient.DownloadFile(request, outputStream, progressAction);

            // Assert
            Assert.True(progressReported);
            Assert.Equal(5, outputStream.Length);
        }

        [Fact]
        public async Task DownloadFile_WithoutProgressReporting_CallsSendAsyncAndCopiesStream()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContentStream = new MemoryStream(new byte[] { 10, 20, 30 });
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseContentStream)
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, outputStream);

            // Assert
            Assert.Equal(3, outputStream.Length);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");
            var streamContent = new MemoryStream(new byte[] { 1, 2, 3 });
            request.Content = new StreamContent(streamContent);

            // Act
            var response = await httpClient.UploadStream(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
