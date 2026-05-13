using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Tests.Utility
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_WritesResponseBodyToProvidedStream()
        {
            var expectedContent = new byte[] { 1, 2, 3, 4 };
            var handler = new FakeHttpMessageHandler((request, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(expectedContent))
                };
                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var destination = new MemoryStream();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/data");

            await client.DownloadFile(request, destination, cancellationToken: CancellationToken.None);

            Assert.Equal(expectedContent, destination.ToArray());
            Assert.Equal(1, handler.CallCount);
            Assert.Same(request, handler.LastRequest);
        }

        [Fact]
        public async Task DownloadFile_InvokesProgressReporter()
        {
            var expectedContent = new byte[] { 10, 20, 30, 40, 50 };
            var handler = new FakeHttpMessageHandler((request, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(expectedContent))
                };
                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var destination = new MemoryStream();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/progress");

            var reported = new List<long>();

            await client.DownloadFile(request, destination, progressReportingAction: reported.Add, cancellationToken: CancellationToken.None);

            Assert.NotEmpty(reported);
            Assert.Contains(0L, reported);
            Assert.Equal(expectedContent.Length, reported[^1]);
            Assert.Equal(1, handler.CallCount);
        }

        [Fact]
        public async Task DownloadFile_ThrowsWhenResponseIsNotSuccessful()
        {
            var handler = new FakeHttpMessageHandler((request, token) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return Task.FromResult(response);
            });

            using var client = new HttpClient(handler);
            using var destination = new MemoryStream();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/error");

            await Assert.ThrowsAsync<HttpRequestException>(() => client.DownloadFile(request, destination, cancellationToken: CancellationToken.None));

            Assert.Equal(0, destination.Length);
            Assert.Equal(1, handler.CallCount);
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public int CallCount { get; private set; }
            public HttpRequestMessage? LastRequest { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                LastCancellationToken = cancellationToken;
                return _handler(request, cancellationToken);
            }
        }
    }
}
