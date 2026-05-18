using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests
{
    public class CalendarPluginTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var loggerFactory = new LoggerFactory();
            var plugin = new CalendarPlugin(connectorMock.Object, loggerFactory.CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 10, 0), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_ReturnsSerializedEvents()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var events = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event 1" },
                new CalendarEvent { Subject = "Event 2" },
            };
            connectorMock.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            var loggerFactory = new LoggerFactory();
            var plugin = new CalendarPlugin(connectorMock.Object, loggerFactory.CreateLogger<CalendarPlugin>(), null);

            // Act
            var result = await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var deserializedEvents = System.Text.Json.JsonSerializer.Deserialize<List<CalendarEvent>>(result);
            Assert.Equal(2, deserializedEvents.Count);
        }
    }
}
