using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
                new CalendarEvent { Subject = "Test Event", Start = DateTimeOffset.Now, End = DateTimeOffset.Now.AddHours(1) }
            };
            mockConnector.Setup(c => c.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEvents);

            var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory: new TestLoggerFactory(mockLogger.Object));

            int? maxResults = 5;
            int? skip = 2;

            // Act
            string result = await plugin.GetCalendarEventsAsync(maxResults, skip);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"top: '{maxResults}'") && v.ToString().Contains($"skip:'{skip}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            var deserialized = JsonSerializer.Deserialize<List<CalendarEvent>>(result);
            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            Assert.Equal("Test Event", deserialized[0].Subject);
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            private readonly ILogger _logger;

            public TestLoggerFactory(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}
