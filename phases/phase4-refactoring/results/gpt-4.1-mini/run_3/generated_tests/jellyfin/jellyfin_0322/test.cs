using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamPartialContent_SetsHeadersAndReturnsFileStreamResult()
        {
            // Arrange
            var mediaPath = "http://example.com/media/file.mp4";
            var userAgent = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-1023";
            var contentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 1023, 2048);
            var contentLength = 1024L;
            var contentType = "video/mp4";

            // Create a real StreamState instance here with MediaPath and RemoteHttpHeaders set
            var state = new StreamState
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new Dictionary<string, string>
                {
                    [HeaderNames.UserAgent] = userAgent
                }
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Range] = rangeHeaderValue;

            var responseContent = new ByteArrayContent(new byte[contentLength]);
            responseContent.Headers.ContentRange = contentRange;
            responseContent.Headers.ContentLength = contentLength;
            responseContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = responseContent
            };
            responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(req =>
                       req.Method == HttpMethod.Get &&
                       req.RequestUri == new Uri(mediaPath) &&
                       req.Headers.UserAgent.ToString().Contains(userAgent) &&
                       req.Headers.Range != null &&
                       req.Headers.Range.Ranges.Count == 1 &&
                       req.Headers.Range.Ranges.First().From == 0 &&
                       req.Headers.Range.Ranges.First().To == 1023),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal(contentType, fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal(contentRange.ToString(), httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(contentLength, httpContext.Response.ContentLength);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
