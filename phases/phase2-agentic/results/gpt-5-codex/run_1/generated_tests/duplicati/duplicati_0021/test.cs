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
        public async Task UploadStream_DelegatesToHttpClientAndReturnsHandlerResponse()
        {
            using var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            var handler = new RecordingHttpMessageHandler((request, token) => Task.FromResult(expectedResponse));

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/upload");

            var response = await client.UploadStream(request).ConfigureAwait(false);

            Assert.Equal(1, handler.CallCount);
            Assert.Same(request, handler.LastRequest);
            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task UploadStream_ForwardsProvidedCancellationToken()
        {
            using var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new RecordingHttpMessageHandler((request, token) => Task.FromResult(expectedResponse));

            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Put, "https://example.com/upload");
            using var cts = new CancellationTokenSource();

            var response = await client.UploadStream(request, cts.Token).ConfigureAwait(false);

            Assert.Equal(1, handler.CallCount);
            Assert.Equal(cts.Token, handler.LastCancellationToken);
            Assert.Same(expectedResponse, response);
        }

        private sealed class RecordingHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public int CallCount { get; private set; }
            public HttpRequestMessage? LastRequest { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            public RecordingHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                LastCancellationToken = cancellationToken;
                return _sendAsync(request, cancellationToken);
            }
        }
    }
}
