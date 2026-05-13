using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeaders_AndSetsResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";
            var userAgentValue = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-99";
            var contentRangeValue = "bytes 0-99/1000";
            var contentLengthValue = 100L;
            var contentTypeValue = "video/mp4";

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
            responseMessage.Content.Headers.ContentRange = ContentRangeHeaderValue.Parse(contentRangeValue);
            responseMessage.Content.Headers.ContentLength = contentLengthValue;
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentTypeValue);

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
                        req.Headers.Range.Ranges[0].From == 0 &&
                        req.Headers.Range.Ranges[0].To == 99),
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

            Assert.Equal(contentTypeValue, fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal(contentRangeValue, httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(contentLengthValue, httpContext.Response.ContentLength);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_NoUserAgentOrRangeHeaders_SetsDefaults()
        {
            // Arrange
            var mediaPath = "http://example.com/media";

            var state = new StreamState
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new Dictionary<string, string>()
            };

            var httpContext = new DefaultHttpContext();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
            };
            // No Accept-Ranges header
            responseMessage.Content.Headers.ContentLength = 3;
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

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

            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.OK, httpContext.Response.StatusCode);
            Assert.Equal("none", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.False(httpContext.Response.Headers.ContainsKey(HeaderNames.ContentRange));
            Assert.Equal(3, httpContext.Response.ContentLength);
        }
    }

    // Minimal stub for StreamState to support tests
    public class StreamState
    {
        public string MediaPath { get; set; } = string.Empty;
        public Dictionary<string, string> RemoteHttpHeaders { get; set; } = new();
    }
}
