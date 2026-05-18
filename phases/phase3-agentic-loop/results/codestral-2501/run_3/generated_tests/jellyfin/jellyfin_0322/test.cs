using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

public class FileStreamResponseHelpersTests
{
    [Fact]
    public async Task GetStaticRemoteStreamResult_SendsRequestAndSetsHeaders()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();
        var httpContextMock = new Mock<HttpContext>();
        var requestMessageMock = new Mock<HttpRequestMessage>();
        var responseMessageMock = new Mock<HttpResponseMessage>();

        var cancellationToken = new CancellationToken();

        // Set up the mock response
        responseMessageMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.PartialContent);
        responseMessageMock.Setup(r => r.Headers.TryGetValues(HeaderNames.AcceptRanges, out It.Ref<IEnumerable<string>>.IsAny)).Returns(true);
        responseMessageMock.Setup(r => r.Content.Headers.ContentRange).Returns(new System.Net.Http.Headers.ContentRangeHeaderValue(0, 100, 200));
        responseMessageMock.Setup(r => r.Content.Headers.ContentLength).Returns(100);

        httpClientMock.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(responseMessageMock.Object);

        // Mock dependencies for StreamState
        var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
        var transcodeManagerMock = new Mock<ITranscodeManager>();

        // Mock StreamState
        var streamState = new StreamState(mediaSourceManagerMock.Object, TranscodingJobType.Progressive, transcodeManagerMock.Object)
        {
            MediaPath = "http://example.com/media",
            RemoteHttpHeaders = new Dictionary<string, string>()
        };

        // Act
        var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
            streamState,
            httpClientMock.Object,
            httpContextMock.Object,
            cancellationToken);

        // Assert
        httpClientMock.Verify(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
        httpContextMock.VerifySet(c => c.Response.Headers[HeaderNames.AcceptRanges] = "bytes");
        httpContextMock.VerifySet(c => c.Response.Headers[HeaderNames.ContentRange] = "bytes 0-100/200");
        httpContextMock.VerifySet(c => c.Response.ContentLength = 100);
    }
}
