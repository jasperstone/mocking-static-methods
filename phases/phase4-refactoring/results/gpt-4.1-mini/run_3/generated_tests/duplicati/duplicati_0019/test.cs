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
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }

        [Fact]
        public async Task DownloadFile_WithFilename_WritesContentToFile()
        {
            // Arrange
            var contentString = "Hello, world!";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(contentString);
            var handler = new MockHttpMessageHandler(async (req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await client.DownloadFile(request, tempFile);

                // Assert
                var fileContent = await File.ReadAllTextAsync(tempFile);
                Assert.Equal(contentString, fileContent);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_WritesContentToStream()
        {
            // Arrange
            var contentString = "Stream content test";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(contentString);
            var handler = new MockHttpMessageHandler(async (req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            using var memoryStream = new MemoryStream();

            // Act
            await client.DownloadFile(request, memoryStream);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var result = await reader.ReadToEndAsync();
            Assert.Equal(contentString, result);
        }

        [Fact]
        public async Task DownloadFile_WithProgressAction_ReportsProgress()
        {
            // Arrange
            var contentString = "Progress test content";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(contentString);
            var handler = new MockHttpMessageHandler(async (req, ct) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/file");
            var tempFile = Path.GetTempFileName();

            long reportedProgress = 0;
            void ProgressAction(long bytes)
            {
                reportedProgress = bytes;
            }

            try
            {
                // Act
                await client.DownloadFile(request, tempFile, ProgressAction);

                // Assert
                Assert.True(reportedProgress > 0);
                var fileContent = await File.ReadAllTextAsync(tempFile);
                Assert.Equal(contentString, fileContent);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
