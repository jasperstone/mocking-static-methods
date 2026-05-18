using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Jellyfin.Api.Helpers;
using Microsoft.Net.Http.Headers;

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
                RemoteHttpHeaders = new System.Collections.Generic.Dictionary<string, string>
                {
                    { HeaderNames.UserAgent, "TestAgent" }
                }
            };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.PartialContent);
            var contentStream = new MemoryStream(new byte[] { 1, 2, 3 });
            mockResponse.Content = new StreamContent(contentStream);
            mockResponse.Content.Headers.ContentLength = 3;
            mockResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            mockResponse.Headers.TryAddWithoutValidation(HeaderNames.AcceptRanges, "bytes");
            mockResponse.Headers.TryAddWithoutValidation(HeaderNames.ContentRange, "bytes 0-2/3");
            mockResponse.StatusCode = HttpStatusCode.PartialContent;

            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(mockResponse));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var context = new DefaultHttpContext();

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
            Assert.Equal("bytes 0-2/3", context.Response.Headers[HeaderNames.ContentRange]);
            Assert.Equal(3, context.Response.ContentLength);
        }
    }
}
