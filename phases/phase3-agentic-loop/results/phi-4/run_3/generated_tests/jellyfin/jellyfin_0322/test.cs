using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

// Assuming StreamState and HeaderNames are part of the Jellyfin.Api namespace
using Jellyfin.Api;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeaders()
    {
        // Arrange
        var state = new StreamState
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream())
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);
        mockRequest.Setup(req => req.Headers).Returns(new HeaderDictionary
        {
            { "Range", "bytes=0-100" }
        });

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient, mockHttpContext.Object);

        // Assert
        var requestMessage = mockHttpMessageHandler.Protected()
            .Invoke<HttpRequestMessage>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
            .First();

        Assert.Equal("TestAgent", requestMessage.Headers.UserAgent.ToString());
        Assert.Equal("bytes=0-100", requestMessage.Headers.Range.ToString());
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_SetsAcceptRangesHeader()
    {
        // Arrange
        var state = new StreamState
        {
            MediaPath = "http://example.com/media"
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.PartialContent,
                Content = new StreamContent(new MemoryStream())
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        var mockHttpContext = new Mock<HttpContext>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

        // Act
        await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient, mockHttpContext.Object);

        // Assert
        Assert.Equal("bytes", mockResponse.Object.Headers[HeaderNames.AcceptRanges]);
    }
}
