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
    public async Task SearchAsync_LogTrace_Called()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"title\",\"description\":\"description\",\"url\":\"url\"}]}}"),
            });
        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), loggerFactoryMock.Object);

        // Act
        await braveConnector.SearchAsync<string>("query", 1, 0, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_LogTrace_WithResponseContent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"title\",\"description\":\"description\",\"url\":\"url\"}]}}"),
            });
        var httpClient = new HttpClient(handlerMock.Object);
        var braveConnector = new BraveConnector("apiKey", httpClient, new Uri("https://api.search.brave.com/res/v1/web/search?q"), loggerFactoryMock.Object);
        var responseContent = "{\"web\":{\"results\":[{\"title\":\"title\",\"description\":\"description\",\"url\":\"url\"}]}}";

        // Act
        await braveConnector.SearchAsync<string>("query", 1, 0, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.Is<object>(o => o.ToString() == responseContent)), Times.Once);
    }
}
