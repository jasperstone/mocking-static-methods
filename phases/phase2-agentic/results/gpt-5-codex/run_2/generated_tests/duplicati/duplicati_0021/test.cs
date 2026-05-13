using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Tests.Library.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WritesContentToProvidedStream()
        {
            var contentBytes = Encoding.UTF8.GetBytes("Download payload");
            var handler = new TestHttpMessageHandler((req, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };

                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler, disposeHandler: false);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");
            using var destination = new MemoryStream();

            await client.DownloadFile(request, destination, cancellationToken: CancellationToken.None);

            Assert.Equal(contentBytes, destination.ToArray());
            Assert.Equal(1, handler.CallCount);
            Assert.Same(request, handler.LastRequest);
        }

        [Fact]
        public async Task DownloadFile_ReportsProgressWhenActionProvided()
        {
            var contentBytes = Encoding.UTF8.GetBytes("Progress payload");
            var progressCalls = new List<long>();
            var handler = new TestHttpMessageHandler((req, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(contentBytes)
                };

                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler, disposeHandler: false);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/resource");
            using var destination = new MemoryStream();

            await client.DownloadFile(request, destination, progressCalls.Add, CancellationToken.None);

            Assert.Equal(contentBytes, destination.ToArray());
            Assert.NotEmpty(progressCalls);
            Assert.Equal(0, progressCalls[0]);
            Assert.Equal(contentBytes.Length, progressCalls[^1]);
            Assert.Equal(1, handler.CallCount);
            Assert.Same(request, handler.LastRequest);
        }

        [Fact]
        public async Task UploadStream_DelegatesToHttpClientSendAsync()
        {
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            try
            {
                var handler = new TestHttpMessageHandler((req, token) =>
                {
                    return Task.FromResult(expectedResponse);
                });

                using var client = new HttpClient(handler, disposeHandler: false);
                using var request = new HttpRequestMessage(HttpMethod.Put, "http://example.com/upload");

                var response = await client.UploadStream(request, CancellationToken.None);

                Assert.Same(expectedResponse, response);
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                Assert.Same(request, handler.LastRequest);
                Assert.Equal(1, handler.CallCount);
            }
            finally
            {
                expectedResponse.Dispose();
            }
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

            public int CallCount { get; private set; }

            public HttpRequestMessage? LastRequest { get; private set; }

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                return _handler(request, cancellationToken);
            }
        }
    }
}
