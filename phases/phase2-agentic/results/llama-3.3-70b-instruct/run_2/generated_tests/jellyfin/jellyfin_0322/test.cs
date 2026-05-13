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

namespace Jellyfin.Api.Tests
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalled()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamSupportsRange_SetAcceptRangesHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.PartialContent)
                {
                    Headers =
                    {
                        AcceptRanges = { "bytes" }
                    }
                });

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

            // Assert
            Assert.Equal("bytes", httpContext.Response.Headers["Accept-Ranges"]);
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_UpstreamDoesNotSupportRange_SetAcceptRangesHeader()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClient, httpContext, cancellationToken);

            // Assert
            Assert.Equal("none", httpContext.Response.Headers["Accept-Ranges"]);
        }
    }
}
