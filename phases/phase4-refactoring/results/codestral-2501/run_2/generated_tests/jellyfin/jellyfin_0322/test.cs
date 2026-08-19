using System;
using System.Net;
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
    public async Task GetStaticRemoteStreamResult_ShouldForwardUserAgentHeader()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");
        state.Setup(s => s.RemoteHttpHeaders).Returns(new HeaderDictionary { { HeaderNames.UserAgent, "TestAgent" } });

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.Object.MediaPath));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeadersMock.Object);

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClientMock.Object, httpContextMock.Object);

        // Assert
        httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(msg => msg.Headers.UserAgent.ToString() == "TestAgent"), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldForwardRangeHeader()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.Object.MediaPath));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeadersMock.Object);

        requestHeadersMock.Setup(headers => headers.TryGetValue(HeaderNames.Range, out It.Ref<string>.IsAny)).Returns(true);

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClientMock.Object, httpContextMock.Object);

        // Assert
        httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(msg => msg.Headers.Range != null), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetAcceptRangesHeader()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.Object.MediaPath));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent);
        responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeadersMock.Object);

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClientMock.Object, httpContextMock.Object);

        // Assert
        responseHeadersMock.VerifySet(headers => headers[HeaderNames.AcceptRanges] = "bytes", Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetContentRangeHeader()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.Object.MediaPath));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent);
        responseMessage.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(100, 199, 500);

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeadersMock.Object);

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClientMock.Object, httpContextMock.Object);

        // Assert
        responseHeadersMock.VerifySet(headers => headers[HeaderNames.ContentRange] = "bytes 100-199/500", Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetContentLengthHeader()
    {
        // Arrange
        var state = new Mock<StreamState>();
        state.Setup(s => s.MediaPath).Returns("http://example.com/media");

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.Object.MediaPath));
        var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent);
        responseMessage.Content.Headers.ContentLength = 100;

        httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessage);

        httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeadersMock.Object);

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClientMock.Object, httpContextMock.Object);

        // Assert
        httpContextMock.VerifySet(context => context.Response.ContentLength = 100, Times.Once);
    }
}
