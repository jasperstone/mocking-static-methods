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

namespace Microsoft.SemanticKernel.Plugins.MsGraph.UnitTests;

public sealed class CalendarPluginTests
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCorrectParametersAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CalendarPlugin>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var loggerFactory = Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(typeof(CalendarPlugin)) == mockLogger.Object);

        var mockConnector = new Mock<ICalendarConnector>();
        mockConnector.Setup(c => c.GetEventsAsync(5, 3, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory, s_jsonSerializerOptions);

        // Act
        await plugin.GetCalendarEventsAsync(maxResults: 5, skip: 3);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s.Contains("Getting calendar events with query options top: '{0}', skip:'{1}'.")),
                It.Is<int?>(top => top == 5),
                It.Is<int?>(skip => skip == 3)),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParametersAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CalendarPlugin>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var loggerFactory = Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(typeof(CalendarPlugin)) == mockLogger.Object);

        var mockConnector = new Mock<ICalendarConnector>();
        mockConnector.Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory, s_jsonSerializerOptions);

        // Act
        await plugin.GetCalendarEventsAsync();

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s.Contains("Getting calendar events with query options top: '{0}', skip:'{1}'.")),
                It.Is<int?>(top => top == 10),
                It.Is<int?>(skip => skip == 0)),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_DoesNotLog_WhenDebugNotEnabledAsync()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CalendarPlugin>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        var loggerFactory = Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(typeof(CalendarPlugin)) == mockLogger.Object);

        var mockConnector = new Mock<ICalendarConnector>();
        mockConnector.Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory, s_jsonSerializerOptions);

        // Act
        await plugin.GetCalendarEventsAsync();

        // Assert
        mockLogger.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_WithNullLogger_DoesNotThrowAsync()
    {
        // Arrange
        var mockConnector = new Mock<ICalendarConnector>();
        mockConnector.Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", default))
            .ReturnsAsync(new List<CalendarEvent>());

        var plugin = new CalendarPlugin(mockConnector.Object);

        // Act
        await plugin.GetCalendarEventsAsync();

        // Assert
        mockConnector.Verify(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", default), Times.Once);
    }
}
