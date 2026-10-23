using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;

namespace CalendarPluginTests
{
    public class CalendarPluginLoggingTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_Should_LogDebugCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CalendarPlugin>>();
            var mockConnector = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory: null);
            // Inject the mocked logger
            typeof(CalendarPlugin)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(plugin, mockLogger.Object);

            var dummyEvents = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Test Event" }
            };

            mockConnector.Setup(c => c.GetEventsAsync(
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dummyEvents);

            // Act
            var result = await plugin.GetCalendarEventsAsync(maxResults: 5, skip: 2);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top: '5', skip:'2'.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
