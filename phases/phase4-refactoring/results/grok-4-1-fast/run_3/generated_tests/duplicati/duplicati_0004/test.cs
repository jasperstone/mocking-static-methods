using Xunit;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Duplicati.Library;

namespace Duplicati.Library.Tests;

public class JsonWebHelperHttpClientTests
{
    private class FakeHttpClient : HttpClient
    {
        public Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> SendAsyncFunc { get; set; } = null!;

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return SendAsyncFunc(request, completionOption, cancellationToken);
        }
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var fakeClient = new FakeHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        fakeClient.SendAsyncFunc = (_1, _2, _3) => Task.FromResult(expectedResponse);

        var helper = new JsonWebHelperHttpClient(fakeClient);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_HttpClientThrows_PropagatesExceptionAfterCleanup()
    {
        // Arrange
        var fakeClient = new FakeHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var expectedException = new HttpRequestException("Network error");
        fakeClient.SendAsyncFunc = (_1, _2, _3) => throw expectedException;

        var helper = new JsonWebHelperHttpClient(fakeClient);
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
        var fakeClient = new FakeHttpClient();
        var cts = new CancellationTokenSource();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        fakeClient.SendAsyncFunc = (_1, _2, _3) => throw new OperationCanceledException(cts.Token);

        var helper = new JsonWebHelperHttpClient(fakeClient);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, cts.Token));
    }
}
