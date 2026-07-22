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
using MediaBrowser.Controller.Streaming;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeadersAndSetsResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";
            var userAgentValue = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-99";

            var state = new StreamState(null!, default, null!)
            {
                MediaPath = mediaPath,
                UserAgent = userAgentValue,
                RemoteHttpHeaders = new Dictionary<string, string>
                {
                    { HeaderNames.UserAgent, userAgentValue }
                }
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Range] = rangeHeaderValue;

            var content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            content.Headers.ContentLength = 4;
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 3, 10);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = content
            };
            responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");

            var httpClient = new HttpClient(new FakeHttpMessageHandler(responseMessage));

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;

            Assert.Equal("video/mp4", fileStreamResult.ContentType);
            Assert.Equal(206, httpContext.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal("bytes 0-3/10", httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(4, httpContext.Response.ContentLength);

            // Check the stream is readable
            using var stream = fileStreamResult.FileStream;
            Assert.True(stream.CanRead);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithoutUserAgentOrRangeHeaders_SetsDefaultHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";

            var state = new StreamState(null!, default, null!)
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new Dictionary<string, string>()
            };

            var httpContext = new DefaultHttpContext();

            var content = new ByteArrayContent(new byte[] { 5, 6, 7, 8 });
            content.Headers.ContentLength = 4;
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            var httpClient = new HttpClient(new FakeHttpMessageHandler(responseMessage));

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;

            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal(200, httpContext.Response.StatusCode);
            Assert.Equal("none", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.False(httpContext.Response.Headers.ContainsKey(HeaderNames.ContentRange));
            Assert.Equal(4, httpContext.Response.ContentLength);

            using var stream = fileStreamResult.FileStream;
            Assert.True(stream.CanRead);
        }
    }
}
