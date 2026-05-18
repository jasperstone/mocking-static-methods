using Xunit;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;

public class JsonWebHelperHttpClientTests
{
    private class TestableJsonWebHelperHttpClient : JsonWebHelperHttpClient
    {
        public TestableJsonWebHelperHttpClient(HttpClient httpClient) : base(httpClient) { }

        public bool SendAsyncCalled { get; private set; }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            SendAsyncCalled = true;
            return Task.FromResult(new HttpResponseMessage());
        }
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_ShouldCallSendAsync()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
        var cancellationToken = new CancellationToken();

        var jsonWebHelperHttpClient = new TestableJsonWebHelperHttpClient(mockHttpClient);

        // Act
        await jsonWebHelperHttpClient.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        // Assert
        Assert.True(jsonWebHelperHttpClient.SendAsyncCalled);
    }
}
