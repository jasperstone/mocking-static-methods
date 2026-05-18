using System;
using System.Collections.Generic;
using System.Linq;
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
        _mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(typeof(CalendarPlugin))).Returns(_mockLogger.Object);
        _plugin = new CalendarPlugin(_mockConnector.Object, loggerFactory.Object);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParameters()
    {
        // Arrange
        _mockConnector.Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func(null!, null!) == "Getting calendar events with query options top: '10', skip:'0'.")),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCustomParameters()
    {
        // Arrange
        _mockConnector.Setup(c => c.GetEventsAsync(5, 3, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: 5, skip: 3);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func(null!, null!) == "Getting calendar events with query options top: '5', skip:'3'.")),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithNullParameters()
    {
        // Arrange
        _mockConnector.Setup(c => c.GetEventsAsync(null, null, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: null, skip: null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    func(null!, null!) == "Getting calendar events with query options top: '', skip:''.")),
            Times.Once);
    }
}
