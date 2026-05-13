using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_CallsSendAsync_WithExpectedParameters()
        {
            // Arrange
            var mediaPath = "http://example.com/video.mp4";
            var state = new StreamState
            {
                MediaPath = mediaPath,
                RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "User-Agent", "TestAgent" }
                }
            };

            var mockHttpClient = new Mock<HttpClient>();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.PartialContent);
            var mockContent = new Mock<HttpContent>();
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockContent.Setup(c => c.ReadAsStreamAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(stream);
            mockResponse.Content = mockContent.Object;
            mockResponse.Headers.TryAddWithoutValidation(HeaderNames.AcceptRanges, "bytes");
            mockResponse.Headers.TryAddWithoutValidation(HeaderNames.ContentRange, "bytes 0-1023/1024");
            mockResponse.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 1023, 1024);
            mockResponse.Content.Headers.ContentLength = 1024;
            mockResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            mockResponse.StatusCode = HttpStatusCode.PartialContent;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Headers.Clear();

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                state,
                httpClient,
                context,
                CancellationToken.None);

            // Assert
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("video/mp4", fileStreamResult.ContentType);
            Assert.Equal((int)HttpStatusCode.PartialContent, context.Response.StatusCode);
            Assert.True(context.Response.Headers.ContainsKey(HeaderNames.AcceptRanges));
            Assert.Equal("bytes", context.Response.Headers[HeaderNames.AcceptRanges]);
            Assert.True(context.Response.Headers.ContainsKey(HeaderNames.ContentRange));
            Assert.Equal("bytes 0-1023/1024", context.Response.Headers[HeaderNames.ContentRange]);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri(mediaPath)),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
