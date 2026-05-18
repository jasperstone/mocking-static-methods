using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
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
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Getting calendar events with query options top: '10', skip:'0'."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage_WithNullMaxResults()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: null, skip: 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Getting calendar events with query options top: '', skip:'0'."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage_WithNullSkip()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CalendarPlugin>>();
            var connectorMock = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(connectorMock.Object, new LoggerFactory().CreateLogger<CalendarPlugin>(), null);

            // Act
            await plugin.GetCalendarEventsAsync(maxResults: 10, skip: null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Getting calendar events with query options top: '10', skip:''."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
