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

namespace Jellyfin.Api.Tests
{
    public class FileStreamResponseHelpersTests
    {
        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalled()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "https://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationTokenSource = new CancellationTokenSource();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent("Hello World");

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
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationTokenSource.Token);

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
            var state = new StreamState
            {
                MediaPath = "https://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationTokenSource = new CancellationTokenSource();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent("Hello World");

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
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationTokenSource.Token);

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

        [Fact]
        public async Task GetStaticRemoteStreamResult_SendAsyncCalledWithCorrectHttpCompletionOption()
        {
            // Arrange
            var state = new StreamState
            {
                MediaPath = "https://example.com/media"
            };

            var httpClientMock = new Mock<HttpClient>();
            var httpContextMock = new Mock<HttpContext>();
            var cancellationTokenSource = new CancellationTokenSource();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent("Hello World");

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
            await FileStreamResponseHelpers.GetStaticRemoteStreamResult(state, httpClientMock.Object, httpContextMock.Object, cancellationTokenSource.Token);

            // Assert
            httpClientMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.Is<HttpCompletionOption>(option => option == HttpCompletionOption.ResponseHeadersRead),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
