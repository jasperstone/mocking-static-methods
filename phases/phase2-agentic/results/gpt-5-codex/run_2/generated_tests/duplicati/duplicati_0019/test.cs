using System;
using System.Collections.Generic;
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
        [Fact]
        public async Task DownloadFile_WithFilename_WritesContentToDisk()
        {
            // Arrange
            var data = new byte[] { 1, 2, 3, 4, 5 };
            using var handler = new TestMessageHandler((_, _) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(data))
                };
                response.Content.Headers.ContentLength = data.Length;
                return Task.FromResult(response);
            });
            using var client = new HttpClient(handler, disposeHandler: true);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/resource");
            var tempFile = Path.Combine(Path.GetTempPath(), $"duplicati-test-{Guid.NewGuid():N}.bin");

            try
            {
                // Act
                await client.DownloadFile(request, tempFile);

                // Assert
                Assert.Equal(1, handler.CallCount);
                Assert.True(File.Exists(tempFile));
                Assert.Equal(data, File.ReadAllBytes(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task DownloadFile_WithStream_ReportsProgressAndCopiesContent()
        {
            // Arrange
            var data = new byte[8192 + 123]; // ensure multiple reads
            new Random(42).NextBytes(data);
            var progressReports = new List<long>();
            using var handler = new TestMessageHandler((_, _) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(data, writable: false))
                };
                response.Content.Headers.ContentLength = data.Length;
                return Task.FromResult(response);
            });
            using var client = new HttpClient(handler, disposeHandler: true);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/stream");
            using var destination = new MemoryStream();

            // Act
            await client.DownloadFile(request, destination, progressReports.Add, CancellationToken.None);

            // Assert
            Assert.Equal(1, handler.CallCount);
            Assert.Equal(data, destination.ToArray());
            Assert.NotEmpty(progressReports);
            Assert.Contains(data.Length, progressReports);
            foreach (var value in progressReports)
            {
                Assert.InRange(value, 1, data.Length);
            }
        }

        private sealed class TestMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

            public TestMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                return _handler(request, cancellationToken);
            }
        }
    }
}
