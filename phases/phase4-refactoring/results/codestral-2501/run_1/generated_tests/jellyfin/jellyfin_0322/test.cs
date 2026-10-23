using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldForwardUserAgent_WhenProvided()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com", RemoteHttpHeaders = new HeaderDictionary { { HeaderNames.UserAgent, "TestAgent" } } };
        var httpClient = new HttpClient();
        var httpContext = new DefaultHttpContext();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestAgent", httpContext.Request.Headers[HeaderNames.UserAgent]);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldForwardRangeHeader_WhenPresent()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com" };
        var httpClient = new HttpClient();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[HeaderNames.Range] = "bytes=0-100";
        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bytes=0-100", httpContext.Request.Headers[HeaderNames.Range]);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetAcceptRangesHeader_WhenUpstreamSupportsRange()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com" };
        var httpClient = new HttpClient();
        var httpContext = new DefaultHttpContext();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("none", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetContentRangeHeader_WhenUpstreamProvidesIt()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com" };
        var httpClient = new HttpClient();
        var httpContext = new DefaultHttpContext();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(httpContext.Response.Headers[HeaderNames.ContentRange]);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetContentLengthHeader_WhenUpstreamProvidesIt()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com" };
        var httpClient = new HttpClient();
        var httpContext = new DefaultHttpContext();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(httpContext.Response.ContentLength);
    }
}
