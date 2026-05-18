using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave;

public class BraveConnectorTests
{
    [Fact]
    public async Task SearchAsync_LogsResponseContentAtTraceLevel()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BraveConnector>>();
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
                Content = new StringContent("{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test description\"}]}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new LoggerFactory().CreateLogger<BraveConnector>());

        // Act
        await braveConnector.SearchAsync<string>("test query");

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsLessThan1()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BraveConnector>>();
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
                Content = new StringContent("{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test description\"}]}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new LoggerFactory().CreateLogger<BraveConnector>());

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test query", 0));
    }

    [Fact]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenCountIsGreaterThanOrEqualTo21()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BraveConnector>>();
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
                Content = new StringContent("{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test description\"}]}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new LoggerFactory().CreateLogger<BraveConnector>());

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test query", 21));
    }

    [Fact]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsLessThan0()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BraveConnector>>();
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
                Content = new StringContent("{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test description\"}]}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new LoggerFactory().CreateLogger<BraveConnector>());

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test query", 1, -1));
    }

    [Fact]
    public async Task SearchAsync_ThrowsArgumentOutOfRangeException_WhenOffsetIsGreaterThan10()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BraveConnector>>();
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
                Content = new StringContent("{\"Web\":{\"Results\":[{\"Title\":\"Test\",\"Description\":\"Test description\"}]}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), new LoggerFactory().CreateLogger<BraveConnector>());

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => braveConnector.SearchAsync<string>("test query", 1, 11));
    }
}
