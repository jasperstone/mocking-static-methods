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
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardUserAgentHeader()
        {
            // Arrange
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/video.mp4",
                RemoteHttpHeaders = new Dictionary<string, string>
                {
                    { "User-Agent", "TestUserAgent/1.0" }
                }
            };

            var httpContext = new DefaultHttpContext();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Array.Empty<byte>())
                })
                .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.UserAgent.ToString() == "TestUserAgent/1.0"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardRangeHeader()
        {
            // Arrange
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/video.mp4"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Range"] = "bytes=100-200";

            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.PartialContent)
                {
                    Content = new ByteArrayContent(Array.Empty<byte>())
                })
                .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            mockHandler.Protected().Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Headers.Range != null 
                    && req.Headers.Range.Ranges.First().From == 100L 
                    && req.Headers.Range.Ranges.First().To == 200L),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamSupportsRange_PartialContent()
        {
            // Arrange
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/video.mp4"
            };

            var httpContext = new DefaultHttpContext();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Headers = 
                {
                    { "Accept-Ranges", new[] { "bytes" } }
                },
                Content = new ByteArrayContent(Array.Empty<byte>())
                {
                    Headers = 
                    {
                        ContentLength = 500,
                        ContentRange = new ContentRangeHeaderValue(100, 599, 1000)
                    }
                }
            };
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.Equal("bytes", httpContext.Response.Headers["Accept-Ranges"].First());
            Assert.Equal("bytes 100-599/1000", httpContext.Response.Headers["Content-Range"].First());
            Assert.Equal("500", httpContext.Response.Headers["Content-Length"].First());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_NoAcceptRangesHeader_ButPartialContent()
        {
            // Arrange
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/video.mp4"
            };

            var httpContext = new DefaultHttpContext();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
                {
                    Headers = { ContentLength = 500 }
                }
            };
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.Equal("bytes", httpContext.Response.Headers["Accept-Ranges"].First());
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_NoRangeSupport()
        {
            // Arrange
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/video.mp4"
            };

            var httpContext = new DefaultHttpContext();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Headers = { { "Accept-Ranges", new[] { "none" } } },
                Content = new ByteArrayContent(Array.Empty<byte>())
                {
                    Headers = { ContentLength = 1000 }
                }
            };
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext);

            // Assert
            Assert.Equal("none", httpContext.Response.Headers["Accept-Ranges"].First());
        }
    }
}
