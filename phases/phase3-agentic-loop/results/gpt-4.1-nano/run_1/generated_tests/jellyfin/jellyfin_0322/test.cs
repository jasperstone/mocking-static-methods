using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_Should_Call_SendAsync_And_Set_ResponseHeaders()
        {
            // Arrange
            var mediaPath = "http://example.com/media";
            var state = new StreamState
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new Dictionary<string, string>
                {
                    { HeaderNames.UserAgent, "TestAgent" }
                }
            };

            var mockHttpClient = new Mock<HttpClient>();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.PartialContent);
            var contentHeaders = new System.Net.Http.Headers.HttpContentHeaders();
            var content = new StringContent("test");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 100, 200);
            content.Headers.ContentLength = 100;
            mockResponse.Content = content;
            mockResponse.Headers.TryAddWithoutValidation(HeaderNames.AcceptRanges, "bytes");
            mockResponse.StatusCode = HttpStatusCode.PartialContent;

            var mockResponseTask = Task.FromResult(mockResponse);
            var mockSendAsync = new Mock<Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>>>();
            mockSendAsync.Setup(f => f(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
                .Returns(mockResponseTask);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    return await mockResponseTask;
                });
            var httpClient = new HttpClient(handlerMock.Object);

            var context = new DefaultHttpContext();
            var responseHeaders = context.Response.Headers;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                state,
                httpClient,
                context,
                CancellationToken.None);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            Assert.Equal(206, context.Response.StatusCode);
            Assert.Equal("bytes", context.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.Equal(content.Headers.ContentRange.ToString(), context.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(content.Headers.ContentLength, context.Response.Content.Headers.ContentLength);
        }
    }
}
