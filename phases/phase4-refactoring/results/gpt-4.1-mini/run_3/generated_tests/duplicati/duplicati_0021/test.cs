using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_WritesContentToStream_AndReportsProgress()
        {
            // Arrange
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(handler);

            var outputStream = new MemoryStream();
            long reportedProgress = 0;
            void ProgressAction(long progress) => reportedProgress = progress;

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream, ProgressAction);

            // Assert
            Assert.Equal(contentBytes.Length, reportedProgress);
            Assert.Equal(contentBytes.Length, outputStream.Length);
            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task DownloadFile_WithStream_WritesContentToStream_WithoutProgress()
        {
            // Arrange
            var contentBytes = new byte[] { 10, 20, 30 };
            var contentStream = new MemoryStream(contentBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(contentStream)
            };

            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                return Task.FromResult(response);
            });

            var httpClient = new HttpClient(handler);

            var outputStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://test"), outputStream);

            // Assert
            Assert.Equal(contentBytes.Length, outputStream.Length);
            outputStream.Position = 0;
            var resultBytes = new byte[contentBytes.Length];
            await outputStream.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(contentBytes, resultBytes);
        }

        [Fact]
        public async Task UploadStream_CallsSendAsync_ReturnsResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                return Task.FromResult(expectedResponse);
            });

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test");

            // Act
            var response = await httpClient.UploadStream(request);

            // Assert
            Assert.Same(expectedResponse, response);
        }
    }
}
