using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests
{
    public class CalendarPluginTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugWithCorrectParameters()
        {
            // Arrange
            var mockConnector = new Mock<ICalendarConnector>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(CalendarPlugin))).Returns(mockLogger.Object);

            var sampleEvents = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event1", Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1) }
            };

            mockConnector.Setup(c => c.GetEventsAsync(
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleEvents);

            var plugin = new CalendarPlugin(mockConnector.Object, mockLoggerFactory.Object);

            int? maxResults = 5;
            int? skip = 2;

            // Act
            await plugin.GetCalendarEventsAsync(maxResults, skip);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"top: '{maxResults}'") && v.ToString()!.Contains($"skip:'{skip}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
