using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalled()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientMock.Verify(h => h.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamSupportsRange_SetAcceptRangesHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var responseMessage = new HttpResponseMessage(HttpStatusCode.PartialContent);
            responseMessage.Headers.AcceptRanges.Add("bytes");

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

            // Assert
            httpContextMock.Verify(h => h.Response.Headers.Add(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamDoesNotSupportRange_SetAcceptRangesHeaderToNone()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(state.MediaPath));
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

            // Assert
            httpContextMock.Verify(h => h.Response.Headers.Add(It.IsAny<string>(), "none"), Times.Once);
        }
    }
}
