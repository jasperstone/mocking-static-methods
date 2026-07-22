using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Tests
{
    public class CalendarPluginTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<CalendarPlugin>();
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, logger, null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 10, 0), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_ReturnsSerializedEvents()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<CalendarPlugin>();
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var events = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event 1" },
                new CalendarEvent { Subject = "Event 2" },
            };
            connectorMock.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            var plugin = new CalendarPlugin(connectorMock.Object, logger, null);

            // Act
            var result = await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var deserializedEvents = JsonSerializer.Deserialize<IEnumerable<CalendarEvent>>(result);
            Assert.NotNull(deserializedEvents);
            Assert.Equal(2, deserializedEvents.Count());
        }
    }
}
