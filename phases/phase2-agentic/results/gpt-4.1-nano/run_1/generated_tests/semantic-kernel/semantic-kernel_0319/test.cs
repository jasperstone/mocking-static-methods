using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests
{
    public class CalendarPluginTests
    {
        private readonly Mock<ICalendarConnector> _mockConnector;
        private readonly Mock<ILogger> _mockLogger;
        private readonly CalendarPlugin _plugin;

        public CalendarPluginTests()
        {
            _mockConnector = new Mock<ICalendarConnector>();
            var loggerFactory = new Mock<ILoggerFactory>();
            _mockLogger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);
            _plugin = new CalendarPlugin(_mockConnector.Object, loggerFactory.Object);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugAndReturnsSerializedEvents()
        {
            // Arrange
            var events = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Test Event" }
            };
            _mockConnector.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            // Act
            var result = await _plugin.GetCalendarEventsAsync(maxResults: 5, skip: 2);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top: '5', skip:'2'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            Assert.Contains("start", result);
        }

        [Fact]
        public async Task AddEventAsync_LogsTraceAndCallsAddEventAsync()
        {
            // Arrange
            var subject = "Meeting";
            var start = DateTimeOffset.Now;
            var end = DateTimeOffset.Now.AddHours(1);
            var location = "Room 1";
            var content = "Discuss project";
            var attendees = "alice,bob";

            // Act
            await _plugin.AddEventAsync(subject, start, end, location, content, attendees);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Adding calendar event '{subject}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _mockConnector.Verify(c => c.AddEventAsync(It.Is<CalendarEvent>(e =>
                e.Subject == subject &&
                e.Start == start &&
                e.End == end &&
                e.Location == location &&
                e.Content == content &&
                e.Attendees != null && e.Attendees.Contains("alice") && e.Attendees.Contains("bob")
            )), Times.Once);
        }

        [Fact]
        public async Task AddEventAsync_ThrowsArgumentException_WhenInputIsNullOrWhitespace()
        {
            // Arrange
            string input = "   ";
            var start = DateTimeOffset.Now;
            var end = DateTimeOffset.Now.AddHours(1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _plugin.AddEventAsync(input, start, end));
        }
    }
}
