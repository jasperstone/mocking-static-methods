using System;
using System.IO;
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
            using var httpClient = new HttpClient(new TestHttpMessageHandler(expectedContent));
            var wrapper = new HttpClientWrapper(httpClient);

            // Act
            using var stream = await wrapper.GetStreamAsync("http://test");
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal(expectedContent, content);
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _responseContent;

            public TestHttpMessageHandler(string responseContent)
            {
                _responseContent = responseContent;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(_responseContent)
                };
                return Task.FromResult(response);
            }
        }
    }
}
