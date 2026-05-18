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
            var mockLogger = new Mock<ILogger>();
            var mockConnector = new Mock<ICalendarConnector>();
            var expectedEvents = new List<CalendarEvent>
            {
                new CalendarEvent { Subject = "Event1" },
                new CalendarEvent { Subject = "Event2" }
            };

            mockConnector.Setup(c => c.GetEventsAsync(
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEvents);

            var plugin = new CalendarPlugin(mockConnector.Object, new LoggerFactory().AddProvider(new TestLoggerProvider(mockLogger.Object)));

            int? maxResults = 5;
            int? skip = 2;

            // Act
            string result = await plugin.GetCalendarEventsAsync(maxResults, skip);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"top: '{maxResults}'") && v.ToString()!.Contains($"skip:'{skip}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Contains("Event1", result);
            Assert.Contains("Event2", result);
        }

        // Helper logger provider to inject mock ILogger into LoggerFactory
        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public TestLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}
