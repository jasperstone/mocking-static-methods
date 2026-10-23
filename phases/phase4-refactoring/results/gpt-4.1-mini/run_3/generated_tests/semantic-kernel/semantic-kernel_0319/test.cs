using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public async Task GetCalendarEventsAsync_LogsDebugAndReturnsSerializedEvents()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockConnector = new Mock<ICalendarConnector>();

            var sampleEvents = new List<CalendarEvent>
            {
                new CalendarEvent
                {
                    Subject = "Test Event",
                    Start = DateTimeOffset.UtcNow,
                    End = DateTimeOffset.UtcNow.AddHours(1),
                    Location = "Test Location",
                    Content = "Test Content",
                    Attendees = new[] { "attendee1@example.com" }
                }
            };

            mockConnector.Setup(c => c.GetEventsAsync(
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(sampleEvents);

            var plugin = new CalendarPlugin(mockConnector.Object, loggerFactory: null);

            // Use reflection to replace the private _logger field with the mock logger
            var loggerField = typeof(CalendarPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(plugin, mockLogger.Object);

            int? maxResults = 5;
            int? skip = 2;

            // Act
            string result = await plugin.GetCalendarEventsAsync(maxResults, skip);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting calendar events with query options top")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            var deserialized = JsonSerializer.Deserialize<List<CalendarEvent>>(result);
            Assert.NotNull(deserialized);
            Assert.Single(deserialized);
            Assert.Equal("Test Event", deserialized[0].Subject);
        }
    }
}
