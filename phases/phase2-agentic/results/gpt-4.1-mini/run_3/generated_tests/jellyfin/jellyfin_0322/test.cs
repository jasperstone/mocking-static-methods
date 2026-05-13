using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using Jellyfin.Api.Helpers;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        private class DummyStreamState : StreamState
        {
            public DummyStreamState(string mediaPath, IDictionary<string, string> headers)
            {
                MediaPath = mediaPath;
                RemoteHttpHeaders = headers;
            }
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeaders_SetsResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media.mp4";
            var userAgentValue = "TestAgent/1.0";
            var rangeHeaderValue = "bytes=0-1023";

            var remoteHeaders = new Dictionary<string, string>
            {
                { HeaderNames.UserAgent, userAgentValue }
            };

            var state = new DummyStreamState(mediaPath, remoteHeaders);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Range] = rangeHeaderValue;

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4 }))
            };
            responseMessage.Content.Headers.ContentLength = 4;
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            responseMessage.Content.Headers.ContentRange = new ContentRangeHeaderValue(0, 3, 10);
            responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");

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
                        req.Headers.Range.Ranges.First().To == 1023),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;
            Assert.Equal("video/mp4", fileStreamResult.ContentType);

            Assert.True(httpContext.Response.Headers.ContainsKey(HeaderNames.AcceptRanges));
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);

            Assert.True(httpContext.Response.Headers.ContainsKey(HeaderNames.ContentRange));
            Assert.Equal("bytes 0-3/10", httpContext.Response.Headers[HeaderNames.ContentRange]);

            Assert.Equal(4, httpContext.Response.ContentLength);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_NoUserAgentOrRangeHeaders_SetsDefaults()
        {
            // Arrange
            var mediaPath = "http://example.com/media.mp4";
            var remoteHeaders = new Dictionary<string, string>();

            var state = new DummyStreamState(mediaPath, remoteHeaders);

            var httpContext = new DefaultHttpContext();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 5, 6, 7 }))
            };
            responseMessage.Content.Headers.ContentLength = 3;
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            // No Accept-Ranges header

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri(mediaPath) &&
                        req.Headers.UserAgent.Count == 0 &&
                        req.Headers.Range == null),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;
            Assert.Equal("video/mp4", fileStreamResult.ContentType);

            Assert.True(httpContext.Response.Headers.ContainsKey(HeaderNames.AcceptRanges));
            Assert.Equal("none", httpContext.Response.Headers[HeaderNames.AcceptRanges]);

            Assert.False(httpContext.Response.Headers.ContainsKey(HeaderNames.ContentRange));

            Assert.Equal(3, httpContext.Response.ContentLength);
            Assert.Equal((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
        }
    }
}
