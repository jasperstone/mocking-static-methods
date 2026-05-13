using System;
using System.Collections.Generic;
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
        [Fact]
        public async Task UploadStream_UsesUnderlyingHttpClientSendAsync()
        {
            HttpRequestMessage? capturedRequest = null;
            CancellationToken capturedToken = default;
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);

            var handler = new TestHttpMessageHandler((request, token) =>
            {
                capturedRequest = request;
                capturedToken = token;
                return Task.FromResult(expectedResponse);
            });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/upload");
            using var cts = new CancellationTokenSource();

            var response = await client.UploadStream(request, cts.Token);

            Assert.Same(request, capturedRequest);
            Assert.Equal(cts.Token, capturedToken);
            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task DownloadFile_WritesContentToStream_AndReportsProgress()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };
            var handler = new TestHttpMessageHandler((request, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(payload))
                };

                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/file");

            using var destinationStream = new MemoryStream();
            var progressUpdates = new List<long>();

            await client.DownloadFile(request, destinationStream, progressUpdates.Add, CancellationToken.None);

            Assert.Equal(payload, destinationStream.ToArray());
            Assert.NotEmpty(progressUpdates);
            Assert.Equal(payload.Length, progressUpdates[^1]);
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => _sendAsync(request, cancellationToken);
        }
    }
}
