using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsResponseContentAsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act
            await braveConnector.SearchAsync<string>("test", 1, 0, default);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsLessThanOrEqualToZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test", 0, 0, default));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsGreaterThanTwenty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test", 21, 0, default));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsLessThanZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test", 1, -1, default));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsGreaterThanTen()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BraveConnector>>();
            var httpClientMock = new Mock<HttpClient>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"title\":\"Test\"}]}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test", 1, 11, default));
        }
    }
}
