using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsResponseContent()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\",\"description\":\"This is a test\"}]}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act
            await braveConnector.SearchAsync<string>(string.Empty);

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
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsLessThan1()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\",\"description\":\"This is a test\"}]}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(string.Empty, 0));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsGreaterThan20()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\",\"description\":\"This is a test\"}]}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(string.Empty, 21));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsLessThan0()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\",\"description\":\"This is a test\"}]}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(string.Empty, 1, -1));
        }

        [Fact]
        public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsGreaterThan10()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"Test\",\"url\":\"https://www.test.com\",\"description\":\"This is a test\"}]}}")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var braveConnector = new BraveConnector(string.Empty, httpClient, null, new LoggerFactory().CreateLogger<BraveConnector>());

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>(string.Empty, 1, 11));
        }
    }
}
