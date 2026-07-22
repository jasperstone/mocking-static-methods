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
    [Fact]
    public async Task AddEventAsync_ValidInput_LogsTrace()
    {
        // Arrange
        var mockConnector = new Mock<ICalendarConnector>();
        var mockLogger = new Mock<ILogger<CalendarPlugin>>();
        var plugin = new CalendarPlugin(mockConnector.Object, new NullLoggerFactory(), null);

        var input = "Test Event";
        var start = DateTimeOffset.Now;
        var end = DateTimeOffset.Now.AddHours(1);

        // Act
        await plugin.AddEventAsync(input, start, end);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding calendar event 'Test Event'")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebug()
    {
        // Arrange
        var mockConnector = new Mock<ICalendarConnector>();
        var mockLogger = new Mock<ILogger<CalendarPlugin>>();
        var plugin = new CalendarPlugin(mockConnector.Object, new NullLoggerFactory(), null);

        // Act
        await plugin.GetCalendarEventsAsync();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top: '10', skip:'0'.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
