using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Library.Tests
{
    public class JsonWebHelperHttpClientTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncFunc;

            public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
            {
                _sendAsyncFunc = sendAsyncFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsyncFunc(request, cancellationToken);
            }
        }

        private class TestJsonWebHelperHttpClient : JsonWebHelperHttpClient
        {
            public bool AttemptParseAndThrowExceptionAsyncCalled { get; private set; } = false;
            public Exception? PassedException { get; private set; }
            public HttpResponseMessage? PassedResponse { get; private set; }
            public CancellationToken PassedCancellationToken { get; private set; }

            public TestJsonWebHelperHttpClient(HttpClient httpClient) : base(httpClient)
            {
            }

            public override async Task AttemptParseAndThrowExceptionAsync(Exception ex, HttpResponseMessage? response, CancellationToken cancellationToken)
            {
                AttemptParseAndThrowExceptionAsyncCalled = true;
                PassedException = ex;
                PassedResponse = response;
                PassedCancellationToken = cancellationToken;
                await Task.CompletedTask;
            }
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ReturnsResponse_WhenSendAsyncSucceeds()
        {
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new TestHttpMessageHandler((req, ct) => Task.FromResult(expectedResponse));
            var httpClient = new HttpClient(handler);
            var helper = new TestJsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            var response = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

            Assert.Same(expectedResponse, response);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_CallsAttemptParseAndThrowExceptionAsync_WhenSendAsyncThrows()
        {
            var handler = new TestHttpMessageHandler((req, ct) => throw new HttpRequestException("fail"));
            var httpClient = new HttpClient(handler);
            var helper = new TestJsonWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            });

            Assert.True(helper.AttemptParseAndThrowExceptionAsyncCalled);
            Assert.Same(ex, helper.PassedException);
            Assert.Null(helper.PassedResponse);
            Assert.Equal(CancellationToken.None, helper.PassedCancellationToken);
        }
    }
}
