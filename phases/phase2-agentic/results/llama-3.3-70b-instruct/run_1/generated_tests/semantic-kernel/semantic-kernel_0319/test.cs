using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class CalendarPluginTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 10, 0), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_CallsGetEventsAsync()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            connectorMock.Verify(c => c.GetEventsAsync(top: 10, skip: 0, select: "start,subject,organizer,location", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_SerializesEvents()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var connectorMock = new Mock<ICalendarConnector>();
            var events = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event 1" },
                new CalendarEvent { Subject = "Event 2" },
            };
            connectorMock.Setup(c => c.GetEventsAsync(top: 10, skip: 0, select: "start,subject,organizer,location", It.IsAny<CancellationToken>())).ReturnsAsync(events);
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            var result = await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var deserializedEvents = System.Text.Json.JsonSerializer.Deserialize<List<CalendarEvent>>(result);
            Assert.NotNull(deserializedEvents);
            Assert.Equal(2, deserializedEvents.Count);
        }
    }
}
