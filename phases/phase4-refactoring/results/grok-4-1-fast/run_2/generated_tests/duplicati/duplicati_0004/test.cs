using Xunit;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;

namespace Duplicati.Library.Tests;

public class JsonWebHelperHttpClientTests
{
    private sealed class TestableJsonWebHelperHttpClient : JsonWebHelperHttpClient
    {
        public Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>>? SendAsyncFunc { get; set; }

        public TestableJsonWebHelperHttpClient(HttpClient httpClient) : base(httpClient) { }

        public override Task<HttpResponseMessage> GetResponseUncheckedAsync(HttpRequestMessage req, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            if (SendAsyncFunc != null)
            {
                return SendAsyncFunc(req, httpCompletionOption, cancellationToken);
            }
            return base.GetResponseUncheckedAsync(req, httpCompletionOption, cancellationToken);
        }
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var helper = new TestableJsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        
        helper.SendAsyncFunc = (_, _, _) => Task.FromResult(expectedResponse);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_HttpClientThrows_PropagatesException()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var helper = new TestableJsonWebHelperHttpClient(httpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var expectedException = new HttpRequestException("Network error");
        
        helper.SendAsyncFunc = (_, _, _) => throw expectedException;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken));
        
        Assert.Equal("Network error", exception.Message);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var helper = new TestableJsonWebHelperHttpClient(httpClient);
        var cts = new CancellationTokenSource();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        
        helper.SendAsyncFunc = (_, _, token) => 
        {
            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Should not reach here");
        };

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token));
    }
}
