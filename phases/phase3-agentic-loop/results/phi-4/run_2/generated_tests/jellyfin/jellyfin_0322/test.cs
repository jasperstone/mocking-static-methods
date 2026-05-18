using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Jellyfin.Api.Models; // Ensure this is included for StreamState
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected; // Include this for Protected method
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_SuccessfulResponse_SetsCorrectHeaders()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(""),
                Content.Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") }
            });

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHttpMessageHandler.Object.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        var mockHttpContext = new Mock<HttpContext>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

        var state = new StreamState
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient.Object, mockHttpContext.Object);

        // Assert
        Assert.IsType<FileStreamResult>(result);
        mockResponse.VerifySet(r => r.Headers[HeaderNames.AcceptRanges] = "none", Times.Once);
        mockResponse.VerifySet(r => r.ContentType = "application/json", Times.Once);
        mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.OK, Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_PartialContentResponse_SetsRangeHeaders()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.PartialContent,
                Content = new StringContent(""),
                Content.Headers = {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4"),
                    ContentRange = System.Net.Http.Headers.ContentRangeHeaderValue.Parse("bytes 0-1023/2048")
                }
            });

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHttpMessageHandler.Object.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        var mockHttpContext = new Mock<HttpContext>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

        var state = new StreamState
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient.Object, mockHttpContext.Object);

        // Assert
        Assert.IsType<FileStreamResult>(result);
        mockResponse.VerifySet(r => r.Headers[HeaderNames.AcceptRanges] = "bytes", Times.Once);
        mockResponse.VerifySet(r => r.Headers[HeaderNames.ContentRange] = "bytes 0-1023/2048", Times.Once);
        mockResponse.VerifySet(r => r.ContentType = "video/mp4", Times.Once);
        mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.PartialContent, Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_NoRangeSupport_SetsCorrectHeaders()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.PartialContent,
                Content = new StringContent(""),
                Content.Headers = {
                    ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4")
                }
            });

        mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockHttpMessageHandler.Object.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

        var mockHttpContext = new Mock<HttpContext>();
        var mockResponse = new Mock<HttpResponse>();
        mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

        var state = new StreamState
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
            {
                { "User-Agent", "TestAgent" }
            }
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, mockHttpClient.Object, mockHttpContext.Object);

        // Assert
        Assert.IsType<FileStreamResult>(result);
        mockResponse.VerifySet(r => r.Headers[HeaderNames.AcceptRanges] = "bytes", Times.Once);
        mockResponse.VerifySet(r => r.ContentType = "video/mp4", Times.Once);
        mockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.PartialContent, Times.Once);
    }
}
