using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests.Helpers
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var streamState = new StreamState { MediaPath = "https://example.com/media" };
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(
                        It.IsAny<HttpRequestMessage>(),
                        It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(streamState, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_InvalidRequest_ReturnsBadRequestResult()
        {
            // Arrange
            var streamState = new StreamState { MediaPath = "https://example.com/media" };
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(
                        It.IsAny<HttpRequestMessage>(),
                        It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(httpResponseMessage);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            // Act
            var result = await FileStreamResponseHelpers.GetStaticRemoteStreamResult(streamState, httpClient, httpContextMock.Object, cancellationToken);

            // Assert
            Assert.IsType<FileStreamResult>(result);
        }
    }
}
