using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using System;
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
            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, loggerFactory.CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 10, 0), Times.Once);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_CallsGetEventsAsync()
        {
            // Arrange
            var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            connectorMock.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CalendarEvent>());
            var plugin = new CalendarPlugin(connectorMock.Object, loggerFactory.CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            connectorMock.Verify(c => c.GetEventsAsync(10, 0, "start,subject,organizer,location", CancellationToken.None), Times.Once);
        }
    }
}
