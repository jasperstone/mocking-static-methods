using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class HttpClientWrapperTests
{
    [Fact]
    public async Task GetStreamAsync_ReturnsStreamFromHttpClient()
    {
        // Arrange
        var expectedContent = "Hello, world!";
        var expectedUrl = "http://example.com/test";

        var handler = new TestHttpMessageHandler(expectedContent, expectedUrl);
        var httpClient = new HttpClient(handler);
        var wrapper = new Microsoft.DotNet.Openapi.Tools.HttpClientWrapper(httpClient);

        // Act
        using var stream = await wrapper.GetStreamAsync(expectedUrl);
        using var reader = new StreamReader(stream);
        var actualContent = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal(expectedContent, actualContent);
        Assert.Equal(expectedUrl, handler.RequestUri.ToString());
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly string _expectedUrl;

        public Uri RequestUri { get; private set; }

        public TestHttpMessageHandler(string content, string expectedUrl)
        {
            _content = content;
            _expectedUrl = expectedUrl;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            if (request.RequestUri.ToString() != _expectedUrl)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_content)
            };

            return Task.FromResult(response);
        }
    }
}
