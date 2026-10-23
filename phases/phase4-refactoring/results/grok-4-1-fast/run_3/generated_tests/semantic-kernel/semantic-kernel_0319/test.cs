using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.UnitTests;

public class CalendarPluginTests
{
    private readonly Mock<ICalendarConnector> _mockConnector;
    private readonly Mock<ILogger> _mockLogger;
    private readonly CalendarPlugin _plugin;

    public CalendarPluginTests()
    {
        _mockConnector = new Mock<ICalendarConnector>();
        _mockLogger = new Mock<ILogger>();

        _plugin = new CalendarPlugin(
            _mockConnector.Object,
            new TestLoggerFactory(_mockLogger.Object),
            null);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParameters()
    {
        // Arrange
        _mockConnector.Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "Getting calendar events with query options top: '10', skip:'0'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCustomParameters()
    {
        // Arrange
        var maxResults = 5;
        var skip = 3;
        _mockConnector.Setup(c => c.GetEventsAsync(maxResults, skip, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new CalendarEvent() });

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults, skip);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, $"Getting calendar events with query options top: '{maxResults}', skip:'{skip}'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithNullParameters()
    {
        // Arrange
        _mockConnector.Setup(c => c.GetEventsAsync(null, null, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(null, null);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "Getting calendar events with query options top: 'null', skip:'null'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static bool ContainsLogMessage<TState>(TState state, string expectedMessage)
    {
        return state?.ToString()?.Contains(expectedMessage) == true;
    }

    private class TestLoggerFactory : ILoggerFactory
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
