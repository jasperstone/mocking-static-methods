using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Xunit;

public class CalendarPluginTests
{
    private readonly Mock<ICalendarConnector> _mockConnector;
    private readonly Mock<ILogger> _mockLogger;
    private readonly CalendarPlugin _plugin;

    public CalendarPluginTests()
    {
        _mockConnector = new Mock<ICalendarConnector>();
        _mockLogger = new Mock<ILogger>();

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(CalendarPlugin).FullName!)).Returns(_mockLogger.Object);

        _plugin = new CalendarPlugin(
            _mockConnector.Object,
            loggerFactory.Object);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParameters()
    {
        // Arrange
        _mockConnector
            .Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting calendar events with query options top: '10', skip:'0'.")),
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
        _mockConnector
            .Setup(c => c.GetEventsAsync(maxResults, skip, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults, skip);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Getting calendar events with query options top: '{maxResults}', skip:'{skip}'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithNullParameters()
    {
        // Arrange
        _mockConnector
            .Setup(c => c.GetEventsAsync(null, null, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(null, null);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting calendar events with query options top: 'null', skip:'null'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
