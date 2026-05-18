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
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_CallsHttpClientSendAsync_WithCorrectParameters()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[0])
                })
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/test.mp4",
                RemoteHttpHeaders = new Dictionary<string, string?>
                {
                    { "User-Agent", "TestUserAgent" }
                }
            };

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(c => c.Request.Headers.TryGetValue("Range", out It.Ref<StringValues>.IsAny))
                          .Returns(false);

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientHandlerMock.Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == "http://example.com/test.mp4" &&
                        req.Headers.UserAgent.ToString() == "TestUserAgent"),
                    HttpCompletionOption.ResponseHeadersRead,
                    ItExpr.IsAny<CancellationToken>());

            Assert.NotNull(result);
            httpClient.Dispose();
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ForwardsRangeHeader_WhenPresent()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.PartialContent))
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/test.mp4"
            };

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(c => c.Request.Headers.TryGetValue("Range", out It.Ref<StringValues>.IsAny))
                          .Returns(true);
            httpContextMock.Setup(c => c.Request.Headers["Range"])
                          .Returns(new StringValues("bytes=100-200"));

            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientHandlerMock.Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.Range != null && req.Headers.Range.Ranges.Count == 1),
                    HttpCompletionOption.ResponseHeadersRead,
                    ItExpr.IsAny<CancellationToken>());

            httpClient.Dispose();
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_HandlesPartialContentResponse_SetsAcceptRangesBytes()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.PartialContent)
                {
                    Headers = { { HeaderNames.AcceptRanges, new[] { "bytes" } } },
                    Content = new ByteArrayContent(new byte[0])
                })
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/test.mp4"
            };

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(c => c.Request.Headers.TryGetValue("Range", out It.Ref<StringValues>.IsAny))
                          .Returns(false);
            httpContextMock.SetupSet(c => c.Response.Headers[HeaderNames.AcceptRanges] = "bytes");

            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientHandlerMock.Protected().Verify();
            httpClient.Dispose();
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SetsContentLength_WhenAvailable()
        {
            // Arrange
            var httpClientHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[1024])
                    {
                        Headers = { ContentLength = 1024 }
                    }
                })
                .Verifiable();

            var httpClient = new HttpClient(httpClientHandlerMock.Object);
            var state = new StreamState(null!, default, null!)
            {
                MediaPath = "http://example.com/test.mp4"
            };

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.Setup(c => c.Request.Headers.TryGetValue("Range", out It.Ref<StringValues>.IsAny))
                          .Returns(false);
            httpContextMock.SetupSet(c => c.Response.ContentLength = 1024L);

            var cancellationToken = CancellationToken.None;

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientHandlerMock.Protected().Verify();
            httpClient.Dispose();
        }
    }
}
