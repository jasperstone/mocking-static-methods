using Xunit;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Duplicati.Library;

namespace Duplicati.Library.Backend.OAuthHelper.Tests;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_CallsSendAsyncWithCorrectParameters()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.EnqueueResponse(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        var httpClient = new HttpClient(mockHandler);
        var helper = new JsonWebHelperHttpClient(httpClient);

        // Act
        var result = await helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Single(mockHandler.Requests);
        var call = mockHandler.Requests[0];
        Assert.Same(request, call.request);
        Assert.Equal(httpCompletionOption, call.completionOption);
        Assert.Equal(cancellationToken, call.cancellationToken);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_ThrowsWhenSendAsyncThrows()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.EnqueueThrow(new HttpRequestException("Test exception"));
        var httpClient = new HttpClient(mockHandler);
        var helper = new JsonWebHelperHttpClient(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken));
        
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_CancelsWhenCancellationTokenTriggered()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var helper = new JsonWebHelperHttpClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => helper.GetResponseUncheckedAsync(request, httpCompletionOption, cts.Token));
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_NullRequest_ThrowsArgumentNull()
    {
        // Arrange
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var mockHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var helper = new JsonWebHelperHttpClient(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => helper.GetResponseUncheckedAsync(null!, httpCompletionOption, cancellationToken));
        
        Assert.Equal("request", exception.ParamName);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)> _requests = new();
    private readonly Queue<Task<HttpResponseMessage>> _responses = new();
    private readonly Queue<Exception> _exceptions = new();

    public List<(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)> Requests => 
        new(_requests);

    public void EnqueueResponse(HttpResponseMessage response)
    {
        _responses.Enqueue(Task.FromResult(response));
    }

    public void EnqueueThrow(Exception ex)
    {
        _exceptions.Enqueue(ex);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use the 3-parameter overload via MockHttpMessageHandler");
    }

    public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        _requests.Enqueue((request, completionOption, cancellationToken));

        if (_exceptions.Count > 0)
            throw _exceptions.Dequeue()!;

        if (_responses.Count == 0)
            throw new InvalidOperationException("No response enqueued");

        return _responses.Dequeue();
    }
}
