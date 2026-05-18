using Xunit;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Duplicati.Library;

namespace Duplicati.Library.Backend.OAuthHelper.Tests;

public class JsonWebHelperHttpClientTests
{
    [Fact]
    public async Task GetResponseUncheckedAsync_CallsSendAsyncWithCorrectParameters()
    {
        // Arrange
        var mockHttpClient = new MockHttpClient();
        var helper = new JsonWebHelperHttpClient(mockHttpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        mockHttpClient.SetupNextResponse(expectedResponse);

        // Act
        var result = await helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken);

        // Assert
        Assert.Same(expectedResponse, result);
        Assert.True(mockHttpClient.SendAsyncWasCalled);
        Assert.Equal(httpCompletionOption, mockHttpClient.LastCompletionOption);
        Assert.Equal(cancellationToken, mockHttpClient.LastCancellationToken);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_ThrowsOriginalExceptionWhenSendAsyncThrows()
    {
        // Arrange
        var mockHttpClient = new MockHttpClient();
        var helper = new JsonWebHelperHttpClient(mockHttpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var expectedException = new HttpRequestException("Network error");

        mockHttpClient.SetupNextThrow(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken));
        
        Assert.Equal("Network error", exception.Message);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_HandlesTaskCanceledException()
    {
        // Arrange
        var mockHttpClient = new MockHttpClient();
        var helper = new JsonWebHelperHttpClient(mockHttpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;
        var expectedException = new OperationCanceledException(cancellationToken);

        mockHttpClient.SetupNextThrow(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken));
        
        Assert.Same(cancellationToken, ((OperationCanceledException)exception).CancellationToken);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var mockHttpClient = new MockHttpClient();
        var helper = new JsonWebHelperHttpClient(mockHttpClient);
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => helper.GetResponseUncheckedAsync(null!, httpCompletionOption, cancellationToken));
        
        Assert.Equal("req", exception.ParamName);
    }
}

public class MockHttpClient : HttpClient
{
    public bool SendAsyncWasCalled { get; private set; }
    public HttpCompletionOption? LastCompletionOption { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }
    private Exception? _nextException;
    private HttpResponseMessage? _nextResponse;

    public void SetupNextResponse(HttpResponseMessage response)
    {
        _nextException = null;
        _nextResponse = response;
    }

    public void SetupNextThrow(Exception exception)
    {
        _nextException = exception;
        _nextResponse = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // This overload is called internally by HttpClient, but we override the protected one
        throw new NotImplementedException("Use the 3-parameter overload for testing");
    }

    public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        SendAsyncWasCalled = true;
        LastCompletionOption = completionOption;
        LastCancellationToken = cancellationToken;

        if (_nextException != null)
            throw _nextException;

        return Task.FromResult(_nextResponse!);
    }
}
