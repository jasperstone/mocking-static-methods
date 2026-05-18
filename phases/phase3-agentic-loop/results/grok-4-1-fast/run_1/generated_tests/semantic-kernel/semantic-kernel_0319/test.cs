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

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests;

public class CalendarPluginTests
{
    private readonly Mock<ICalendarConnector> _mockConnector;
    private readonly Mock<ILogger> _mockLogger;
    private readonly CalendarPlugin _plugin;

    public CalendarPluginTests()
    {
        _mockConnector = new Mock<ICalendarConnector>();
        _mockLogger = new Mock<ILogger>();
        var loggerFactory = Mock.Of<ILoggerFactory>(f => f.CreateLogger(typeof(CalendarPlugin).FullName!) == _mockLogger.Object);
        _plugin = new CalendarPlugin(_mockConnector.Object, loggerFactory);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Getting calendar events with query options top: '10', skip:'0'.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCustomParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(5, 3, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: 5, skip: 3);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Getting calendar events with query options top: '5', skip:'3'.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithNullParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(null, null, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CalendarEvent>());

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: null, skip: null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                0,
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static bool ContainsMessage<TState>(TState state, string expectedMessage)
    {
        var stateStr = state?.ToString() ?? "";
        return stateStr.Contains(expectedMessage);
    }
}
