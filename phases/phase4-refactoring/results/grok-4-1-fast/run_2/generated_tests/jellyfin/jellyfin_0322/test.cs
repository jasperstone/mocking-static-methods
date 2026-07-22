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
using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_NoUserAgent_NoRange_SetsHeadersCorrectly()
        {
            // Arrange
            var state = CreateMinimalStreamState("http://example.com/video.mp4");
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, new Dictionary<string, string[]>
            {
                [HeaderNames.AcceptRanges] = new[] { "bytes" }
            });
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            Assert.Equal("bytes", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithUserAgent_ForwardsUserAgent()
        {
            // Arrange
            var state = CreateMinimalStreamState("http://example.com/video.mp4");
            state.Setup(s => s.RemoteHttpHeaders).Returns(new Dictionary<string, string>
            {
                [HeaderNames.UserAgent] = "TestAgent/1.0"
            });
            
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            
            var httpClient = new HttpClient(handlerMock.Object);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient, httpContext.Object, cancellationToken);

            // Assert
            handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.UserAgent.ToString() == "TestAgent/1.0"),
                    ItExpr.Is<HttpCompletionOption>(opt => opt == HttpCompletionOption.ResponseHeadersRead),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithRangeHeader_206PartialContent_HandlesRangeCorrectly()
        {
            // Arrange
            var state = CreateMinimalStreamState("http://example.com/video.mp4");
            var httpClient = CreateMockHttpClient(HttpStatusCode.PartialContent, new Dictionary<string, string[]>
            {
                [HeaderNames.AcceptRanges] = new[] { "bytes" }
            }, contentLength: 1000L, contentRangeString: "bytes 0-999/10000");
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>
            {
                [HeaderNames.Range] = "bytes=0-999"
            });
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal(1000, httpContext.Object.Response.ContentLength);
            Assert.Equal("bytes", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
            Assert.Equal("bytes 0-999/10000", httpContext.Object.Response.Headers[HeaderNames.ContentRange].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_NoAcceptRangesHeader_200OK_SetsAcceptRangesNone()
        {
            // Arrange
            var state = CreateMinimalStreamState("http://example.com/video.mp4");
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal("none", httpContext.Object.Response.Headers[HeaderNames.AcceptRanges].ToString());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_WithContentLength_SetsContentLength()
        {
            // Arrange
            var state = CreateMinimalStreamState("http://example.com/video.mp4");
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, contentLength: 12345L);
            var httpContext = CreateMockHttpContext(new Dictionary<string, StringValues>());
            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state.Object, httpClient.Object, httpContext.Object, cancellationToken);

            // Assert
            Assert.Equal(12345, httpContext.Object.Response.ContentLength);
        }

        private static Mock<StreamState> CreateMinimalStreamState(string mediaPath)
        {
            var state = new Mock<StreamState>();
            state.Setup(s => s.MediaPath).Returns(mediaPath);
            state.Setup(s => s.RemoteHttpHeaders).Returns(new Dictionary<string, string>());
            return state;
        }

        private static Mock<HttpClient> CreateMockHttpClient(
            HttpStatusCode statusCode,
            Dictionary<string, string[]> responseHeaders = null,
            long? contentLength = null,
            string contentRangeString = null)
        {
            responseHeaders ??= new Dictionary<string, string[]>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    var response = new HttpResponseMessage(statusCode);
                    foreach (var kvp in responseHeaders)
                    {
                        response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                    }
                    
                    var content = new ByteArrayContent(Array.Empty<byte>());
                    if (contentLength.HasValue)
                    {
                        content.Headers.ContentLength = contentLength.Value;
                    }
                    if (!string.IsNullOrEmpty(contentRangeString))
                    {
                        content.Headers.ContentRange = Microsoft.Net.Http.Headers.ContentRangeHeaderValue.Parse(contentRangeString);
                    }
                    response.Content = content;
                    
                    return response;
                });

            var httpClient = new HttpClient(handlerMock.Object);
            return new Mock<HttpClient>(() => httpClient) { CallBase = true };
        }

        private static Mock<HttpContext> CreateMockHttpContext(Dictionary<string, StringValues> requestHeaders)
        {
            var requestHeadersDict = new HeaderDictionary(requestHeaders);
            var responseHeadersDict = new HeaderDictionary();

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(requestHeadersDict);

            var mockResponse = new Mock<HttpResponse>();
            mockResponse.SetupProperty(r => r.ContentLength);
            mockResponse.Setup(r => r.Headers).Returns(responseHeadersDict);
            mockResponse.SetupProperty(r => r.StatusCode);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            return mockContext;
        }
    }
}
