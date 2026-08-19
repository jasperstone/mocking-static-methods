using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Streaming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var mediaSourceManager = new MediaSourceManager();
            var transcodeManager = new TranscodeManager();
            var streamState = new StreamState(mediaSourceManager, TranscodingJobType.Progressive, transcodeManager) { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient(new MockHttpMessageHandler());
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(streamState, httpClient, httpContext, cancellationToken);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream());
                return Task.FromResult(response);
            }
        }
    }
}
