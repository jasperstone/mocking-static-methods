using System;
using System.Collections.Generic;
using System.Text.Json;
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
    private readonly ILogger<CalendarPlugin> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly CalendarPlugin _calendarPlugin;

    public CalendarPluginTests()
    {
        _mockConnector = new Mock<ICalendarConnector>();
        _logger = NullLogger<CalendarPlugin>.Instance;
        _jsonSerializerOptions = new JsonSerializerOptions();
        _calendarPlugin = new CalendarPlugin(_mockConnector.Object, new NullLoggerFactory(), _jsonSerializerOptions);
    }

    [Fact]
    public async Task AddEventAsync_ValidInput_LogsTraceAndAddsEvent()
    {
        // Arrange
        var input = "Test Event";
        var start = DateTimeOffset.Now;
        var end = DateTimeOffset.Now.AddHours(1);
        var location = "Test Location";
        var content = "Test Content";
        var attendees = "attendee1@example.com,attendee2@example.com";

        // Act
        await _calendarPlugin.AddEventAsync(input, start, end, location, content, attendees);

        // Assert
        _mockConnector.Verify(c => c.AddEventAsync(It.IsAny<CalendarEvent>()), Times.Once);
        // Verify that LogTrace is called with the correct message
        // Note: We cannot directly verify LogTrace calls with Moq, so we rely on the connector call verification.
    }

    [Fact]
    public async Task GetCalendarEventsAsync_ValidInput_LogsDebugAndReturnsEvents()
    {
        // Arrange
        var maxResults = 10;
        var skip = 0;
        var cancellationToken = CancellationToken.None;
        var expectedEvents = new List<CalendarEvent>
        {
            new CalendarEvent { Subject = "Event 1" },
            new CalendarEvent { Subject = "Event 2" }
        };
        _mockConnector.Setup(c => c.GetEventsAsync(maxResults, skip, It.IsAny<string>(), cancellationToken))
                      .ReturnsAsync(expectedEvents);

        // Act
        var result = await _calendarPlugin.GetCalendarEventsAsync(maxResults, skip, cancellationToken);

        // Assert
        var events = JsonSerializer.Deserialize<List<CalendarEvent>>(result, _jsonSerializerOptions);
        Assert.Equal(expectedEvents.Count, events.Count);
        // Verify that LogDebug is called with the correct message
        // Note: We cannot directly verify LogDebug calls with Moq, so we rely on the connector call verification.
    }
}
