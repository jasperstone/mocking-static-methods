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
                new CalendarEvent { Subject = "Test Event 1" },
                new CalendarEvent { Subject = "Test Event 2" }
            };
            _mockConnector.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);

            // Act
            var result = await _plugin.GetCalendarEventsAsync(maxResults: 5, skip: 2);

            // Assert
            _mockLogger.Verify(
                l => l.LogDebug("Getting calendar events with query options top: '{0}', skip:'{1}'.", 5, 2),
                Times.Once);
            Assert.Contains("Test Event 1", result);
            Assert.Contains("Test Event 2", result);
        }
    }
}
