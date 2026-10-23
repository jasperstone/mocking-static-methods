using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Moq.Language.Flow;
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
            new Mock<ILoggerFactory>().Object,
            null);
        
        // Replace logger via reflection since it's private readonly
        typeof(CalendarPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_plugin, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCorrectParameters()
    {
        // Arrange
        var maxResults = 5;
        var skip = 2;
        var mockEvents = new List<CalendarEvent>
        {
            new CalendarEvent { Subject = "Test Event" }
        };
        _mockConnector
            .Setup(c => c.GetEventsAsync(maxResults, skip, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockEvents);

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults, skip);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Getting calendar events with query options top: '5', skip:'2'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_DefaultParameters_LogsDebugMessageWithNulls()
    {
        // Arrange
        _mockConnector
            .Setup(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<CalendarEvent>?)null);

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Getting calendar events with query options top: '10', skip:'0'.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static bool ContainsMessage<TState>(TState state, string expectedMessage)
    {
        return state?.ToString()?.Contains(expectedMessage) == true;
    }
}
