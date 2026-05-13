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
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_SendsRequestAndSetsHeadersCorrectly()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpClient.Handler = mockHttpMessageHandler.Object;

        var mockHttpContext = new Mock<HttpContext>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

        var mockRequestMessage = new Mock<HttpRequestMessage>();
        var mockResponseMessage = new Mock<HttpResponseMessage>();
        mockResponseMessage.Setup(r => r.StatusCode).Returns(HttpStatusCode.PartialContent);
        mockResponseMessage.Setup(r => r.Content.Headers.ContentRange).Returns(new System.Net.Http.Headers.ContentRangeHeaderValue(0, 100, 200));
        mockResponseMessage.Setup(r => r.Content.Headers.ContentLength).Returns(100);
        mockResponseMessage.Setup(r => r.Content.Headers.ContentType).Returns(new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"));

        mockHttpMessageHandler
            .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponseMessage.Object);

        var state = new StreamState
        {
            MediaPath = "http://example.com/media.mp4",
            RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient.Object, mockHttpContext.Object);

        // Assert
        mockHttpMessageHandler.Verify(handler => handler.SendAsync(mockRequestMessage.Object, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);

        mockHttpContext.VerifySet(ctx => ctx.Response.Headers[HeaderNames.AcceptRanges] = "bytes", Times.Once);
        mockHttpContext.VerifySet(ctx => ctx.Response.Headers[HeaderNames.ContentRange] = "bytes 0-100/200", Times.Once);
        mockHttpContext.VerifySet(ctx => ctx.Response.ContentLength = 100, Times.Once);
        mockHttpContext.VerifySet(ctx => ctx.Response.StatusCode = (int)HttpStatusCode.PartialContent, Times.Once);
    }
}
