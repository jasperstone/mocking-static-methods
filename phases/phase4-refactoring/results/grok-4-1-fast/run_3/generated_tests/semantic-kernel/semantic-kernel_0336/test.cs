using System;
using System.Collections.Generic;
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

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.UnitTests;

public sealed class BraveConnectorTests
{
    private static readonly Uri TestUri = new("https://api.search.brave.com/res/v1/web/search?q");

    [Fact]
    public async Task SearchAsync_LogsTraceWithResponseContent_WhenTraceEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var mockResponseContent = new Mock<HttpContent>();
        var jsonContent = "{\"web\":{\"results\":[{\"title\":\"Test\",\"description\":\"Test desc\",\"url\":\"http://test.com\"}]}}";
        mockResponseContent.Setup(c => c.ReadAsStringAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonContent);
        
        var mockResponse = new Mock<HttpResponseMessage>(HttpStatusCode.OK) { CallBase = true };
        mockResponse.Setup(r => r.Content).Returns(mockResponseContent.Object);
        
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        var connector = new BraveConnector("fake-key", mockHttpClient.Object, TestUri, loggerFactory);

        // Act
        _ = await connector.SearchAsync<string>("test query");

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response content received:") && v.ToString()!.Contains(jsonContent)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BraveConnector>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var mockResponseContent = new Mock<HttpContent>();
        mockResponseContent.Setup(c => c.ReadAsStringAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");
        
        var mockResponse = new Mock<HttpResponseMessage>(HttpStatusCode.OK) { CallBase = true };
        mockResponse.Setup(r => r.Content).Returns(mockResponseContent.Object);
        
        var mockHttpClient = new Mock<HttpClient>();
        mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        var connector = new BraveConnector("fake-key", mockHttpClient.Object, TestUri, loggerFactory);

        // Act
        _ = await connector.SearchAsync<string>("test query");

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

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryIsNull()
    {
        // Arrange & Act
        var connector = new BraveConnector("fake-key", TestUri);

        // Assert
        var loggerField = typeof(BraveConnector).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(loggerField);
        var logger = (ILogger)loggerField!.GetValue(connector)!;
        Assert.IsType<NullLogger>(logger);
    }

    private sealed class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public TestLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
