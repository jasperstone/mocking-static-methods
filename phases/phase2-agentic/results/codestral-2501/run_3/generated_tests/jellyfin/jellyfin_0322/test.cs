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
        public async Task GetStaticRemoteStreamResult_SendAsync_Success()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            };

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)httpContextMock.Object.Response.StatusCode);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsync_WithRangeHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "http://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent)
            {
                Content = new StringContent("Test content")
            };

            httpContextMock.Setup(context => context.Request.Headers.TryGetValue(HeaderNames.Range, out It.Ref<string>.IsAny))
                .Returns(true);

            httpClientMock.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal(HttpStatusCode.PartialContent, (HttpStatusCode)httpContextMock.Object.Response.StatusCode);
        }
    }
}
