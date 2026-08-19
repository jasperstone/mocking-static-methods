using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Tests.Library.Utility
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
        public async Task DownloadFile_WithFilename_WritesFileAndReportsProgress()
        {
            // Arrange
            var contentBytes = new byte[] { 1, 2, 3, 4, 5 };
            var contentStream = new MemoryStream(contentBytes);
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                long reportedProgress = 0;
                void ProgressAction(long bytes)
                {
                    reportedProgress = bytes;
                }

                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), tempFile, ProgressAction);

                // Assert
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
                Assert.True(reportedProgress > 0);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_WritesStreamAndReportsProgress()
        {
            // Arrange
            var contentBytes = new byte[] { 10, 20, 30, 40, 50 };
            var contentStream = new MemoryStream(contentBytes);
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });
            var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();
            long reportedProgress = 0;
            void ProgressAction(long bytes)
            {
                reportedProgress = bytes;
            }

            // Act
            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), outputStream, ProgressAction);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
            Assert.True(reportedProgress > 0);
        }

        [Fact]
        public async Task DownloadFile_WithFilename_NoProgress_WritesFile()
        {
            // Arrange
            var contentBytes = new byte[] { 100, 101, 102 };
            var contentStream = new MemoryStream(contentBytes);
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });
            var client = new HttpClient(handler);

            var tempFile = Path.GetTempFileName();
            try
            {
                // Act
                await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), tempFile);

                // Assert
                var fileBytes = await File.ReadAllBytesAsync(tempFile);
                Assert.Equal(contentBytes, fileBytes);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_NoProgress_WritesStream()
        {
            // Arrange
            var contentBytes = new byte[] { 200, 201, 202 };
            var contentStream = new MemoryStream(contentBytes);
            var handler = new TestHttpMessageHandler((req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            });
            var client = new HttpClient(handler);

            using var outputStream = new MemoryStream();

            // Act
            await client.DownloadFile(new HttpRequestMessage(HttpMethod.Get, "http://localhost"), outputStream);

            // Assert
            var resultBytes = outputStream.ToArray();
            Assert.Equal(contentBytes, resultBytes);
        }
    }
}
