using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Api.Tests
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ValidRequest_ReturnsFileStreamResult()
        {
            // Arrange
            var streamState = new 
            {
                MediaPath = "https://example.com/media",
                RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "User-Agent", "Test User Agent" }
                }
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream())
            };

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(httpResponseMessage);

            var httpContextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();

            httpContextMock
                .SetupGet(c => c.Request)
                .Returns(httpRequestMock.Object);

            httpContextMock
                .SetupGet(c => c.Response)
                .Returns(httpResponseMock.Object);

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                new MediaBrowser.Controller.Streaming.StreamState 
                { 
                    MediaPath = streamState.MediaPath, 
                    RemoteHttpHeaders = streamState.RemoteHttpHeaders 
                },
                httpClientMock.Object,
                httpContextMock.Object);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }
    }
}
