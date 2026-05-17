using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests.Brave;

public sealed class BraveConnectorTests
{
    private static readonly Uri DefaultUri = new("https://api.search.brave.com/res/v1/web/search?q");

    [Fact]
    public async Task SearchAsync_LogsTraceWithResponseContent_WhenValidResponseReceived()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"test\",\"description\":\"test desc\",\"url\":\"test.com\"}]}}")
        };
        
        mockHttpClient.Setup(c => c.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        
        var connector = new BraveConnector("fake-key", mockHttpClient.Object, DefaultUri, loggerFactory.Object);

        // Act
        await connector.SearchAsync<string>("test query", cancellationToken: default);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    s => s.ToString().Contains("Response content received:") &&
                         s.ToString().Contains("\"web\":{\"results\":[{\"title\":\"test\"}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_DoesNotLogTrace_WhenTraceLevelDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(c => c.SendWithSuccessCheckAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        var connector = new BraveConnector("fake-key", mockHttpClient.Object, DefaultUri, loggerFactory.Object);

        // Act
        await connector.SearchAsync<string>("test query", cancellationToken: default);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
