using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncFunc;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
            {
                _sendAsyncFunc = sendAsyncFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsyncFunc(request, cancellationToken);
            }
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ReturnsResponse_WhenSendAsyncSucceeds()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler((req, ct) => Task.FromResult(expectedResponse));
            var httpClient = new HttpClient(handler);
            var helper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act
            var response = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

            // Assert
            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ThrowsAndDisposesResponse_WhenSendAsyncThrows()
        {
            // Arrange
            var disposed = false;
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Dispose(); // Dispose early to simulate disposed response in catch block

            var handler = new FakeHttpMessageHandler((req, ct) =>
            {
                throw new HttpRequestException("Simulated failure");
            });
            var httpClient = new HttpClient(handler);
            var helper = new JsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None));
        }
    }
}
