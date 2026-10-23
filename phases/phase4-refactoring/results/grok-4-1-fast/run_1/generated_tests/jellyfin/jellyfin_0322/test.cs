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
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_NoUserAgent_NoRange_SetsHeadersCorrectly()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>(), new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal(200, httpContext.Object.Response.StatusCode);
            Assert.Equal("none", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithUserAgent_ForwardsUserAgent()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "http://example.com/video.mp4",
                RemoteHttpHeaders = new Dictionary<string, string> { { HeaderNames.UserAgent, "TestAgent/1.0" } }
            };
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler.Object);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>(), new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext.Object, cancellationToken);

            // Assert
            handler.Protected().Verify("SendAsync", Times.Once(), 
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.UserAgent.ToString() == "TestAgent/1.0"));
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithRangeHeader_ForwardsRange()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.PartialContent);
            var rangeHeader = "bytes=0-999";
            var httpContext = CreateMockHttpContext(
                new Dictionary<string, StringValues> { { HeaderNames.Range, new StringValues(rangeHeader) } },
                new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal(206, httpContext.Object.Response.StatusCode);
            Assert.Equal("bytes", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamAcceptsRanges_SetsAcceptRangesHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, acceptRanges: new[] { "bytes" });
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>(), new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal("bytes", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithContentRange_SetsContentRangeHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var contentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 999, 1000);
            var httpClient = CreateMockHttpClient(HttpStatusCode.PartialContent, contentRange: contentRange);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>(), new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal(contentRange.ToString(), httpContext.Object.Response.Headers[HeaderNames.ContentRange].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithContentLength_SetsContentLength()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, contentLength: 12345);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>(), new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal(12345, httpContext.Object.Response.ContentLength);
        }

        private static Mock<HttpClient> CreateMockHttpClient(
            HttpStatusCode statusCode,
            string[]? acceptRanges = null,
            System.Net.Http.Headers.ContentRangeHeaderValue? contentRange = null,
            long? contentLength = null)
        {
            var handler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(statusCode);
            
            if (acceptRanges != null)
            {
                response.Headers.Add(HeaderNames.AcceptRanges, acceptRanges);
            }
            
            if (contentRange != null)
            {
                response.Content.Headers.ContentRange = contentRange;
            }
            
            if (contentLength.HasValue)
            {
                response.Content.Headers.ContentLength = contentLength.Value;
            }
            
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            
            return new Mock<HttpClient>(handler.Object) { CallBase = true };
        }

        private static Mock<HttpContext> CreateMockHttpContext(
            Dictionary<string, StringValues> requestHeaders,
            Dictionary<string, StringValues> responseHeaders)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Headers).Returns(new HeaderDictionary(requestHeaders));

            var response = new Mock<HttpResponse>();
            var responseHeadersDict = new HeaderDictionary();
            foreach (var kvp in responseHeaders)
            {
                responseHeadersDict.Append(kvp.Key, kvp.Value);
            }
            response.Setup(r => r.Headers).Returns(responseHeadersDict);
            response.SetupProperty(r => r.StatusCode);
            response.SetupProperty(r => r.ContentLength);

            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            return context;
        }
    }
}
