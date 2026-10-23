using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;

namespace CalendarPluginTests
{
    public class CalendarPluginTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ICalendarConnector> _connectorMock;

        public CalendarPluginTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            _connectorMock = new Mock<ICalendarConnector>();
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugAndReturnsSerializedEvents()
        {
            // Arrange
            var plugin = new CalendarPlugin(_connectorMock.Object, _loggerFactoryMock.Object);
            var sampleEvents = new[] { new CalendarEvent { Subject = "Test Event" } };
            _connectorMock.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleEvents);

            // Act
            var result = await plugin.GetCalendarEventsAsync(maxResults: 5, skip: 2);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 5, 2), Times.Once);
            Assert.Contains("Test Event", result);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_UsesDefaultParametersAndLogsDebug()
        {
            // Arrange
            var plugin = new CalendarPlugin(_connectorMock.Object, _loggerFactoryMock.Object);
            var sampleEvents = new[] { new CalendarEvent { Subject = "Default Event" } };
            _connectorMock.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleEvents);

            // Act
            var result = await plugin.GetCalendarEventsAsync();

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 10, 0), Times.Once);
            Assert.Contains("Default Event", result);
        }

        [Fact]
        public async Task AddEventAsync_LogsTraceAndCallsAddEventAsync()
        {
            // Arrange
            var plugin = new CalendarPlugin(_connectorMock.Object, _loggerFactoryMock.Object);
            var input = "Meeting";
            var start = DateTimeOffset.Now;
            var end = start.AddHours(1);
            _connectorMock.Setup(c => c.AddEventAsync(It.IsAny<CalendarEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CalendarEvent());

            // Act
            await plugin.AddEventAsync(input, start, end);

            // Assert
            _loggerMock.Verify(l => l.LogTrace("Adding calendar event '{0}'", input), Times.Once);
            _connectorMock.Verify(c => c.AddEventAsync(It.Is<CalendarEvent>(e => e.Subject == input), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddEventAsync_ThrowsArgumentException_WhenInputIsNullOrWhitespace()
        {
            // Arrange
            var plugin = new CalendarPlugin(_connectorMock.Object, _loggerFactoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.AddEventAsync(null, DateTimeOffset.Now, DateTimeOffset.Now));
            await Assert.ThrowsAsync<ArgumentException>(() => plugin.AddEventAsync("  ", DateTimeOffset.Now, DateTimeOffset.Now));
        }
    }
}
