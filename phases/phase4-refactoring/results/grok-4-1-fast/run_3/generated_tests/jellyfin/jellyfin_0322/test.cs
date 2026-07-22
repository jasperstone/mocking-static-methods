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
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, null, null, 1024);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            httpContext.Verify(x => x.Response.Headers[HeaderNames.AcceptRanges] = "none", Times.Once);
            httpContext.Verify(x => x.Response.ContentLength = 1024, Times.Once);
            httpContext.Verify(x => x.Response.StatusCode = 200, Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithUserAgent_ForwardsUserAgent()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            state.RemoteHttpHeaders = new Dictionary<string, string> { { HeaderNames.UserAgent, "TestAgent/1.0" } };
            
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Array.Empty<byte>())
                })
                .Verifiable();
            var httpClient = new HttpClient(handler.Object);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext.Object, cancellationToken);

            // Assert
            handler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.UserAgent.ToString() == "TestAgent/1.0"),
                ItExpr.IsAny<HttpCompletionOption>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithRangeHeader_206PartialContent_SetsAcceptRangesBytes()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.PartialContent, null, null, 500);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues> { { HeaderNames.Range, "bytes=0-499" } });
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            httpContext.Verify(x => x.Response.Headers[HeaderNames.AcceptRanges] = "bytes", Times.Once);
            httpContext.Verify(x => x.Response.ContentLength = 500, Times.Once);
            httpContext.Verify(x => x.Response.StatusCode = 206, Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithAcceptRangesHeader_UsesHeaderValue()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new[] { "bytes", "none" }, null, 1024);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            httpContext.Verify(x => x.Response.Headers[HeaderNames.AcceptRanges] = "bytes, none", Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithContentRange_SetsContentRangeHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/video.mp4" };
            var contentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 499, 1024);
            var httpClient = CreateMockHttpClient(HttpStatusCode.PartialContent, null, null, 500, contentRange);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            httpContext.Verify(x => x.Response.Headers[HeaderNames.ContentRange] = contentRange.ToString(), Times.Once);
        }

        private static Mock<HttpClient> CreateMockHttpClient(
            HttpStatusCode statusCode,
            IEnumerable<string>? acceptRanges = null,
            string? acceptRangesValue = null,
            long? contentLength = null,
            System.Net.Http.Headers.ContentRangeHeaderValue? contentRange = null)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };

            if (contentLength.HasValue)
            {
                response.Content.Headers.ContentLength = contentLength.Value;
            }

            if (contentRange != null)
            {
                response.Content.Headers.ContentRange = contentRange;
            }

            if (acceptRanges != null)
            {
                response.Headers.Add(HeaderNames.AcceptRanges, acceptRanges);
            }
            else if (!string.IsNullOrEmpty(acceptRangesValue))
            {
                response.Headers.Add(HeaderNames.AcceptRanges, acceptRangesValue);
            }

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<HttpCompletionOption>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new Mock<HttpClient>(handler.Object) { CallBase = true };
            return httpClient;
        }

        private static Mock<HttpContext> CreateMockHttpContext(Dictionary<string, StringValues> requestHeaders)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(x => x.Headers).Returns(new HeaderDictionary(requestHeaders));

            var response = new Mock<HttpResponse>();
            response.SetupSet(x => x.Headers[It.IsAny<string>()] = It.IsAny<StringValues>()).Verifiable();
            response.SetupSet(x => x.ContentLength = It.IsAny<long?>()).Verifiable();
            response.SetupSet(x => x.StatusCode = It.IsAny<int>()).Verifiable();

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.Request).Returns(request.Object);
            httpContext.Setup(x => x.Response).Returns(response.Object);
            return httpContext;
        }
    }
}
