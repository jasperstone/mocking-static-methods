using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Tests.Library
{
    public class JsonWebHelperHttpClientTests
    {
        [Fact]
        public async Task GetResponseUncheckedAsync_ReturnsHttpClientResponse()
        {
            using var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("payload")
            };
            HttpRequestMessage? capturedRequest = null;

            var handler = new StubHttpMessageHandler((request, token) =>
            {
                capturedRequest = request;
                return Task.FromResult(expectedResponse);
            });

            using var httpClient = new HttpClient(handler);
            var sut = new JsonWebHelperHttpClient(httpClient);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");

            var response = await sut.GetResponseUncheckedAsync(
                request,
                HttpCompletionOption.ResponseContentRead,
                CancellationToken.None);

            Assert.Same(expectedResponse, response);
            Assert.Same(request, capturedRequest);
            Assert.Equal("payload", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_WhenSendAsyncThrows_RethrowsOriginalException()
        {
            var expectedException = new InvalidOperationException("boom");
            var handler = new StubHttpMessageHandler((request, token) =>
                Task.FromException<HttpResponseMessage>(expectedException));

            using var httpClient = new HttpClient(handler);
            var sut = new JsonWebHelperHttpClient(httpClient);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");

            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await sut.GetResponseUncheckedAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    CancellationToken.None);
            });

            Assert.Same(expectedException, actualException);
        }

        [Fact]
        public async Task GetResponseAsync_OnFailureDisposesResponse()
        {
            var trackingContent = new TrackingStringContent("failure");
            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = trackingContent
            };

            var handler = new StubHttpMessageHandler((request, token) => Task.FromResult(response));

            using var httpClient = new HttpClient(handler);
            var sut = new JsonWebHelperHttpClient(httpClient);
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com/");

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await sut.GetResponseAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    CancellationToken.None);
            });

            Assert.True(trackingContent.Disposed);
        }

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

            public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
            {
                _send = send;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _send(request, cancellationToken);
            }
        }

        private sealed class TrackingStringContent : StringContent
        {
            public bool Disposed { get; private set; }

            public TrackingStringContent(string content)
                : base(content)
            {
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Disposed = true;
                }

                base.Dispose(disposing);
            }
        }
    }
}
