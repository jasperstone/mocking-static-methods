using System;
using System.Net;
using System.Net.Http;
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
        public async Task GetStaticRemoteStreamResult_ShouldForwardUserAgent_WhenProvided()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };
            state.RemoteHttpHeaders.Add(HeaderNames.UserAgent, "TestUserAgent");

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(req => req.Headers.UserAgent.ToString() == "TestUserAgent"), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldForwardRangeHeader_WhenPresent()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.OK);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            var requestHeaders = new HeaderDictionary();
            requestHeaders.Add(HeaderNames.Range, "bytes=0-100");
            httpContextMock.Setup(ctx => ctx.Request.Headers).Returns(requestHeaders);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(req => req.Headers.Range.ToString() == "bytes=0-100"), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetAcceptRangesHeader_WhenUpstreamSupportsRange()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.PartialContent);
            responseMock.Setup(r => r.Headers.TryGetValues(HeaderNames.AcceptRanges, out It.Ref<string>.IsAny)).Returns(true);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            var responseHeaders = new HeaderDictionary();
            responseHeaders.Add(HeaderNames.AcceptRanges, "bytes");
            responseMock.Setup(r => r.Headers).Returns(responseHeaders);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpContextMock.Verify(ctx => ctx.Response.Headers[HeaderNames.AcceptRanges], Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetContentRangeHeader_WhenUpstreamProvidesIt()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.PartialContent);
            responseMock.Setup(r => r.Content.Headers.ContentRange).Returns(new System.Net.Http.Headers.ContentRangeHeaderValue(0, 100, 200));

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpContextMock.Verify(ctx => ctx.Response.Headers[HeaderNames.ContentRange], Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetContentLengthHeader_WhenUpstreamProvidesIt()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponseMessage>();
            responseMock.Setup(r => r.StatusCode).Returns(HttpStatusCode.PartialContent);
            responseMock.Setup(r => r.Content.Headers.ContentLength).Returns(100);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpContextMock.VerifySet(ctx => ctx.Response.ContentLength = 100, Times.Once);
        }
    }
}
