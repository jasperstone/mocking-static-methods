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
    public async Task GetStaticRemoteStreamResult_ShouldForwardUserAgentHeader()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com/media" };
        state.RemoteHttpHeaders[HeaderNames.UserAgent] = "TestUserAgent";

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        httpContextMock.Setup(x => x.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(x => x.Response.Headers).Returns(responseHeadersMock.Object);

        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

        // Assert
        httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Headers.UserAgent.ToString() == "TestUserAgent"), HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldForwardRangeHeader()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com/media" };

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        requestHeadersMock.Setup(x => x.TryGetValue(HeaderNames.Range, out It.Ref<string>.IsAny)).Returns(true);
        requestHeadersMock.Setup(x => x[HeaderNames.Range]).Returns("bytes=0-100");

        httpContextMock.Setup(x => x.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(x => x.Response.Headers).Returns(responseHeadersMock.Object);

        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

        // Assert
        httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(m => m.Headers.Range.ToString() == "bytes=0-100"), HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetStaticRemoteStreamResult_ShouldSetAcceptRangesHeader()
    {
        // Arrange
        var state = new StreamState { MediaPath = "http://example.com/media" };

        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestHeadersMock = new Mock<IHeaderDictionary>();
        var responseHeadersMock = new Mock<IHeaderDictionary>();

        httpContextMock.Setup(x => x.Request.Headers).Returns(requestHeadersMock.Object);
        httpContextMock.Setup(x => x.Response.Headers).Returns(responseHeadersMock.Object);

        var responseMock = new Mock<HttpResponseMessage>();
        responseMock.Setup(x => x.StatusCode).Returns(System.Net.HttpStatusCode.PartialContent);
        responseMock.Setup(x => x.Headers.TryGetValues(HeaderNames.AcceptRanges, out It.Ref<IEnumerable<string>>.IsAny)).Returns(true);

        httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>())).ReturnsAsync(responseMock.Object);

        var cancellationToken = new CancellationToken();

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

        // Assert
        responseHeadersMock.VerifySet(x => x[HeaderNames.AcceptRanges] = "bytes");
    }
}
