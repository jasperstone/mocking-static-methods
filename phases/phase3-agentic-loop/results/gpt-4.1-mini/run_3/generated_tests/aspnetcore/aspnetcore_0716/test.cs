using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Openapi.Tools;
using Xunit;

namespace Microsoft.DotNet.Openapi.Tools.Tests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task GetStreamAsync_InvokesHttpClientGetStreamAsync_ReturnsStream()
        {
            // Arrange
            var expectedContent = "Hello, world!";
            var httpClient = new HttpClient(new TestHttpMessageHandler(expectedContent));
            var wrapper = new HttpClientWrapper(httpClient);

            // Act
            using var resultStream = await wrapper.GetStreamAsync("http://example.com");

            // Assert
            Assert.NotNull(resultStream);
            using var reader = new StreamReader(resultStream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal(expectedContent, content);
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly byte[] _contentBytes;

            public TestHttpMessageHandler(string content)
            {
                _contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var contentStream = new MemoryStream(_contentBytes);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(contentStream)
                };
                return Task.FromResult(response);
            }
        }
    }
}
