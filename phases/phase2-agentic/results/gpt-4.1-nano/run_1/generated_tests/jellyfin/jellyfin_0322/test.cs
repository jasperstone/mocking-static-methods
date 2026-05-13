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
            var mediaPath = "http://example.com/media";
            var state = new StreamState { MediaPath = mediaPath, RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>() };
            var mockHttpClient = new Mock<HttpClient>();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.PartialContent);
            var mockContent = new Mock<HttpContent>();
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockContent.Setup(c => c.ReadAsStreamAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stream);
            mockResponse.Content = mockContent.Object;
            mockResponse.Headers.TryGetValues = (name, out var values) =>
            {
                values = new[] { "bytes" };
                return true;
            };
            mockResponse.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 2, 3);
            mockResponse.Content.Headers.ContentLength = 3;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async req =>
                {
                    var resp = mockResponse;
                    return resp;
                });
            var client = new HttpClient(mockHttpMessageHandler.Object);

            var context = new DefaultHttpContext();
            context.Response.Headers["X-Test"] = "value";

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(
                state,
                client,
                context,
                CancellationToken.None);

            // Assert
            Assert.IsType<FileStreamResult>(result);
            var fileResult = (FileStreamResult)result;
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.NotNull(fileResult.FileStream);
            Assert.Equal(3, context.Response.ContentLength);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("bytes", context.Response.Headers["Accept-Ranges"]);
        }
    }
}
