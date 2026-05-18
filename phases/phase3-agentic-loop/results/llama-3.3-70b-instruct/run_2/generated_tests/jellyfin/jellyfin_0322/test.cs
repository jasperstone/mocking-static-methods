using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Helpers;
using Jellyfin.Api.Models;
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
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, new HttpClient(handlerMock.Object), httpContext, cancellationToken);

            // Assert
            handlerMock
                .Verify(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()),
                    Times.Once()
                );
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalledWithCorrectRequest()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, new HttpClient(handlerMock.Object), httpContext, cancellationToken);

            // Assert
            handlerMock
                .Verify(
                    h => h.SendAsync(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == state.MediaPath), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()),
                    Times.Once()
                );
        }

        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalledWithCorrectHttpCompletionOption()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClient = new HttpClient();
            var httpContext = new DefaultHttpContext();
            var cancellationToken = CancellationToken.None;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, new HttpClient(handlerMock.Object), httpContext, cancellationToken);

            // Assert
            handlerMock
                .Verify(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.Is<HttpCompletionOption>(option => option == HttpCompletionOption.ResponseHeadersRead), It.IsAny<CancellationToken>()),
                    Times.Once()
                );
        }
    }
}
