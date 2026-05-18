using System;
using System.Collections.Generic;
using System.IO;
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
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsUserAgentAndRangeHeaders_SetsResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";
            var userAgentValue = "TestUserAgent";
            var rangeHeaderValue = "bytes=0-99";

            var remoteHttpHeaders = new Dictionary<string, string>
            {
                { HeaderNames.UserAgent, userAgentValue }
            };

            var stateMock = new Mock<StreamState>(MockBehavior.Strict, null, default(TranscodingJobType), null);
            stateMock.SetupGet(s => s.MediaPath).Returns(mediaPath);
            stateMock.SetupGet(s => s.RemoteHttpHeaders).Returns(remoteHttpHeaders);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Range] = rangeHeaderValue;

            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }))
            };
            responseMessage.Headers.Add(HeaderNames.AcceptRanges, "bytes");
            responseMessage.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 99, 100);
            responseMessage.Content.Headers.ContentLength = 100;
            responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

            var handler = new MockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handler);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(stateMock.Object, httpClient, httpContext);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            var fileStreamResult = (FileStreamResult)result;
            Assert.Equal("video/mp4", fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.PartialContent, httpContext.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal("bytes 0-99/100", httpContext.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(100, httpContext.Response.ContentLength);
        }

        // Helper class to mock HttpMessageHandler for HttpClient
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public MockHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
