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

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        private HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(responseMessage)
               .Verifiable();

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task DownloadFile_Stream_NoProgress_WritesContentToStream()
        {
            // Arrange
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(contentBytes)
            };
            var httpClient = CreateHttpClient(responseMessage);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

            using var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, outputStream, progressReportingAction: null, cancellationToken: CancellationToken.None);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task DownloadFile_Stream_WithProgress_ReportsProgressAndWritesContent()
        {
            // Arrange
            var contentBytes = new byte[100];
            for (int i = 0; i < contentBytes.Length; i++) contentBytes[i] = (byte)i;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(contentBytes)
            };
            var httpClient = CreateHttpClient(responseMessage);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");

            using var outputStream = new MemoryStream();

            long lastProgress = -1;
            void ProgressAction(long bytes)
            {
                lastProgress = bytes;
            }

            // Act
            await httpClient.DownloadFile(request, outputStream, ProgressAction, CancellationToken.None);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
            Assert.True(lastProgress > 0, "Progress action should be called with positive bytes count");
            Assert.Equal(contentBytes.Length, lastProgress);
        }
    }
}
