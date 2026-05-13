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

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests
{
    public class CalendarPluginTests
    {
        private readonly Mock<ICalendarConnector> _mockConnector;
        private readonly Mock<ILogger<CalendarPlugin>> _mockLogger;
        private readonly CalendarPlugin _calendarPlugin;

        public CalendarPluginTests()
        {
            _mockConnector = new Mock<ICalendarConnector>();
            _mockLogger = new Mock<ILogger<CalendarPlugin>>();
            _calendarPlugin = new CalendarPlugin(_mockConnector.Object, new NullLoggerFactory(), new JsonSerializerOptions());
        }

        [Fact]
        public async Task AddEventAsync_ValidInput_LogsTrace()
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
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding calendar event 'Test Event'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_ValidInput_LogsDebug()
        {
            // Arrange
            var maxResults = 10;
            var skip = 0;
            var cancellationToken = CancellationToken.None;
            var events = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event 1" },
                new CalendarEvent { Subject = "Event 2" }
            };
            _mockConnector.Setup(x => x.GetEventsAsync(maxResults, skip, It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(events);

            // Act
            var result = await _calendarPlugin.GetCalendarEventsAsync(maxResults, skip, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top: '10', skip:'0'.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);

            var deserializedEvents = JsonSerializer.Deserialize<List<CalendarEvent>>(result);
            Assert.NotNull(deserializedEvents);
            Assert.Equal(2, deserializedEvents.Count);
            Assert.Equal("Event 1", deserializedEvents[0].Subject);
            Assert.Equal("Event 2", deserializedEvents[1].Subject);
        }
    }
}
