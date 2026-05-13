using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Stream _streamToReturn;

            public TestHttpMessageHandler(Stream streamToReturn)
            {
                _streamToReturn = streamToReturn;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(_streamToReturn)
                };
                return Task.FromResult(response);
            }
        }

        [Fact]
        public async Task GetStreamAsync_CallsHttpClientGetStreamAsync_ReturnsExpectedStream()
        {
            // Arrange
            var expectedContent = "Hello, world!";
            var expectedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(expectedContent));
            var handler = new TestHttpMessageHandler(expectedStream);
            var httpClient = new HttpClient(handler);
            var wrapper = new HttpClientWrapper(httpClient);

            // Act
            var actualStream = await wrapper.GetStreamAsync("http://example.com");

            // Assert
            Assert.NotNull(actualStream);
            using var reader = new StreamReader(actualStream);
            var actualContent = await reader.ReadToEndAsync();
            Assert.Equal(expectedContent, actualContent);
        }
    }
}
