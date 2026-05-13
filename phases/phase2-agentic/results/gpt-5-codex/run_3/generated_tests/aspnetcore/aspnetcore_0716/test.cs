using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_DelegatesToHttpClientWithProvidedUrl()
        {
            var responseContent = "Hello from handler";
            var requestCount = 0;
            string? requestedUrl = null;
            HttpMethod? requestedMethod = null;

            var handler = new TestHttpMessageHandler(request =>
            {
                requestCount++;
                requestedUrl = request.RequestUri?.ToString();
                requestedMethod = request.Method;

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseContent)
                };

                return Task.FromResult(response);
            });

            using var wrapper = new HttpClientWrapper(new HttpClient(handler));

            using var stream = await wrapper.GetStreamAsync("https://example.com/test");
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            Assert.Equal(1, requestCount);
            Assert.Equal("https://example.com/test", requestedUrl);
            Assert.Equal(HttpMethod.Get, requestedMethod);
            Assert.Equal(responseContent, content);
        }

        [Fact]
        public async Task GetResponseAsync_ReturnsWrapperWithExpectedProperties()
        {
            var responseContent = "OpenAPI payload";
            var handler = new TestHttpMessageHandler(request =>
            {
                var httpResponse = new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent(responseContent)
                };

                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "openapi.json"
                };
                httpResponse.Headers.TryAddWithoutValidation("Content-Disposition", contentDisposition.ToString());

                return Task.FromResult(httpResponse);
            });

            using var wrapper = new HttpClientWrapper(new HttpClient(handler));

            using var responseWrapper = await wrapper.GetResponseAsync("https://example.com/openapi");

            using var stream = await responseWrapper.Stream;
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var contentDispositionHeader = responseWrapper.ContentDisposition();

            Assert.IsType<HttpResponseMessageWrapper>(responseWrapper);
            Assert.Equal(HttpStatusCode.Accepted, responseWrapper.StatusCode);
            Assert.True(responseWrapper.IsSuccessCode());
            Assert.Equal(responseContent, content);
            Assert.NotNull(contentDispositionHeader);
            Assert.Equal("attachment", contentDispositionHeader!.DispositionType);
            Assert.Equal("openapi.json", contentDispositionHeader.FileName);
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responder;

            public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _responder(request);
            }
        }
    }
}
