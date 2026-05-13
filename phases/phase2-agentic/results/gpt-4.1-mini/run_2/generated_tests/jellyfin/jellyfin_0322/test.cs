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
        public async Task GetStaticRemoteStreamResult_ForwardsHeadersAndReturnsFileStreamResult()
        {
            // Arrange
            var mediaPath = "http://example.com/media.mp4";
            var userAgentValue = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-1023";
            var contentRangeValue = "bytes 0-1023/2048";
            var contentLengthValue = 1024L;
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

            var httpClientMock = new Mock<HttpClient>(MockBehavior.Strict);
            // We cannot mock HttpClient.SendAsync directly because it's not virtual.
            // Instead, we mock HttpMessageHandler and create HttpClient with it.
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
            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;
            Assert.Equal(contentTypeValue, fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal(contentRangeValue, httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(contentLengthValue, httpContext.Response.ContentLength);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        // Minimal stub for StreamState to compile and run test
        private class StreamState
        {
            public string MediaPath { get; set; } = string.Empty;
            public Dictionary<string, string> RemoteHttpHeaders { get; set; } = new();
        }
    }
}
