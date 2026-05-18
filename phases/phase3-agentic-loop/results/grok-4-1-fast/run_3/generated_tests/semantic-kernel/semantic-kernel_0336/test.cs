using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests.Brave;

public sealed class BraveConnectorTests
{
    private static readonly Uri s_defaultUri = new("https://api.search.brave.com/res/v1/web/search?q");

    [Fact]
    public async Task SearchAsync_LogsTraceWithResponseContent_WhenValidResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"web\":{\"results\":[{\"title\":\"test\",\"description\":\"test desc\",\"url\":\"test.com\"}]}}")
            });
        
        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var connector = new BraveConnector("test-key", mockHttpClient, s_defaultUri, loggerFactory.Object);

        // Act
        _ = await connector.SearchAsync<string>("test query");

        // Assert
        mockLogger.Verify(
            l => l.LogTrace("Response content received: {Data}", It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_DoesNotLogTrace_WhenTraceLevelDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        
        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var connector = new BraveConnector("test-key", mockHttpClient, s_defaultUri, loggerFactory.Object);

        // Act
        _ = await connector.SearchAsync<string>("test query");

        // Assert
        mockLogger.Verify(
            l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryNull()
    {
        // Arrange & Act
        var connector = new BraveConnector("test-key");

        // Assert
        var loggerField = typeof(BraveConnector).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var logger = loggerField?.GetValue(connector) as ILogger;
        Assert.IsType<NullLogger>(logger);
    }
}
