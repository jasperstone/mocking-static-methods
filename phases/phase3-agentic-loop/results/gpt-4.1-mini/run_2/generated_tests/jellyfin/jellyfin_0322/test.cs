using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeadersAndSetsResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";
            var userAgentValue = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-99";

            var state = new StreamState
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new Dictionary<string, string>
                {
                    { HeaderNames.UserAgent, userAgentValue }
                }
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Range] = rangeHeaderValue;

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
            };
            responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");
            responseMessage.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 99, 200);
            responseMessage.Content.Headers.ContentLength = 100;
            responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri(mediaPath) &&
                        req.Headers.UserAgent.ToString().Contains(userAgentValue) &&
                        req.Headers.Range != null &&
                        req.Headers.Range.Ranges.Count == 1 &&
                        req.Headers.Range.Ranges.First().From == 0 &&
                        req.Headers.Range.Ranges.First().To == 99),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;
            Assert.Equal("video/mp4", fileStreamResult.ContentType);
            Assert.Equal(100, httpContext.Response.ContentLength);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal("bytes 0-99/200", httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        // Minimal stub for StreamState to compile and match the expected type
        private class StreamState : MediaBrowser.Controller.Streaming.StreamState
        {
            public StreamState()
            {
                RemoteHttpHeaders = new Dictionary<string, string>();
            }
        }
    }
}
