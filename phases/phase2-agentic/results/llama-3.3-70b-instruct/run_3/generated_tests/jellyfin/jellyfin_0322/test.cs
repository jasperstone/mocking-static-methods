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
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StreamContent(new MemoryStream());

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientMock
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
        public async Task GetStaticRemoteStreamResult_SendAsyncCalledWithCorrectRequest()
        {
            // Arrange
            var state = new StreamState { MediaPath = "https://example.com/media" };
            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationToken = new CancellationToken();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StreamContent(new MemoryStream());

            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationToken);

            // Assert
            httpClientMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == state.MediaPath),
                    ItExpr.IsAny<HttpCompletionOption>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
