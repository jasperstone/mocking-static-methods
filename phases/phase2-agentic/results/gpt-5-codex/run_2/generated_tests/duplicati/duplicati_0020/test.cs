using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WritesContentToProvidedStream()
        {
            // Arrange
            var testContent = "Duplicati unit test content";
            using var destinationStream = new MemoryStream();

            var handler = new FakeHandler(async (request, cancellationToken) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(testContent)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/data");

            // Act
            await client.DownloadFile(request, destinationStream);

            // Assert
            destinationStream.Position = 0;
            using var reader = new StreamReader(destinationStream);
            var result = reader.ReadToEnd();
            Assert.Equal(testContent, result);
            Assert.True(handler.SendAsyncCalled);
        }

        [Fact]
        public async Task DownloadFile_WithProgressReportsBytesRead()
        {
            // Arrange
            var testContent = "Progress reporting content";
            using var destinationStream = new MemoryStream();
            long lastReportedValue = -1;

            var handler = new FakeHandler(async (request, cancellationToken) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(testContent)
                };
                return await Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/progress");

            void ProgressReporter(long value)
            {
                Assert.True(value >= 0);
                lastReportedValue = value;
            }

            // Act
            await client.DownloadFile(request, destinationStream, ProgressReporter);

            // Assert
            Assert.Equal(testContent.Length, lastReportedValue);
            Assert.True(handler.SendAsyncCalled);
        }

        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public bool SendAsyncCalled { get; private set; }

            public FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                SendAsyncCalled = true;
                return await _handlerFunc(request, cancellationToken);
            }
        }
    }
}
