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
            var state = new StreamState
            {
                MediaPath = "http://example.com/media",
                RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
                {
                    { HeaderNames.UserAgent, "TestUserAgent" }
                }
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(msg => msg.Headers.UserAgent.ToString() == "TestUserAgent"), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldForwardRangeHeader_WhenPresent()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "http://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var requestHeaders = new HeaderDictionary
            {
                { HeaderNames.Range, "bytes=0-100" }
            };
            httpContextMock.Setup(context => context.Request.Headers).Returns(requestHeaders);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            httpClientMock.Verify(client => client.SendAsync(It.Is<HttpRequestMessage>(msg => msg.Headers.Range.ToString() == "bytes=0-100"), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetAcceptRangesHeader_WhenUpstreamSupportsRange()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "http://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
            response.Headers.Add(HeaderNames.AcceptRanges, "bytes");

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var responseHeaders = new HeaderDictionary();
            httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeaders);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            Assert.Equal("bytes", responseHeaders[HeaderNames.AcceptRanges]);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetContentRangeHeader_WhenUpstreamProvidesIt()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "http://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
            response.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 100, 200);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var responseHeaders = new HeaderDictionary();
            httpContextMock.Setup(context => context.Response.Headers).Returns(responseHeaders);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            Assert.Equal("bytes 0-100/200", responseHeaders[HeaderNames.ContentRange]);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_ShouldSetContentLengthHeader_WhenUpstreamProvidesIt()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "http://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var response = new HttpResponseMessage(HttpStatusCode.PartialContent);
            response.Content.Headers.ContentLength = 100;

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var responseHeaders = new HeaderDictionary();
            httpContextMock.Setup(context => context.Response.ContentLength).Returns(100);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            Assert.Equal(100, httpContextMock.Object.Response.ContentLength);
        }
    }
}
