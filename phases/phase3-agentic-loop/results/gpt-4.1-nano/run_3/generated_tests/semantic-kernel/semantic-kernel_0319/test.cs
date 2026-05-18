using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using System.Text.Json;

namespace CalendarPluginTests
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
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            _plugin = new CalendarPlugin(_mockConnector.Object, loggerFactory.Object);
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugCall()
        {
            // Arrange
            var expectedEvents = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Test Event" }
            };
            _mockConnector.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEvents);

            // Act
            var result = await _plugin.GetCalendarEventsAsync();

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top: '10', skip:'0'.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            Assert.NotNull(result);
            var deserialized = JsonSerializer.Deserialize<IEnumerable<CalendarEvent>>(result);
            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            Assert.Equal("Test Event", deserialized.First().Subject);
        }
    }
}
