using Xunit;
using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Duplicati.Library;

namespace Duplicati.Library.Backend.OAuthHelper.Tests;

public class JsonWebHelperHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _mockHttpClient;
    private readonly JsonWebHelperHttpClient _helper;

    public JsonWebHelperHttpClientTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHandler.Object);
        _helper = new JsonWebHelperHttpClient(_mockHttpClient);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_CallsSendAsyncWithCorrectParameters()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                httpCompletionOption,
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse)
            .Verifiable();

        // Act
        var result = await _helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken);

        // Assert
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            httpCompletionOption,
            ItExpr.IsAny<CancellationToken>());
        
        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_ThrowsWhenSendAsyncThrows()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();
        var expectedException = new HttpRequestException("Test exception");

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                httpCompletionOption,
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _helper.GetResponseUncheckedAsync(request, httpCompletionOption, cancellationToken));
        
        Assert.NotNull(exception);
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_CancelsWhenCancellationTokenTriggered()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                httpCompletionOption,
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _helper.GetResponseUncheckedAsync(request, httpCompletionOption, cts.Token));
    }

    [Fact]
    public async Task GetResponseUncheckedAsync_NullRequest_ThrowsArgumentNull()
    {
        // Arrange
        var httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
        var cancellationToken = new CancellationToken();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _helper.GetResponseUncheckedAsync(null!, httpCompletionOption, cancellationToken));
        
        Assert.Equal("request", exception.ParamName);
    }
}
