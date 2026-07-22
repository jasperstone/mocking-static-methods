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
        private class TestLogger : ILogger
        {
            public LogLevel? LoggedLevel { get; private set; }
            public string? LoggedMessage { get; private set; }
            public object?[]? LoggedArgs { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => NullLogger.Instance.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                LoggedLevel = logLevel;
                LoggedMessage = formatter(state, exception);
                if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                {
                    var argsList = new List<object>();
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key == "{OriginalFormat}") continue;
                        argsList.Add(kvp.Value);
                    }
                    LoggedArgs = argsList.ToArray();
                }
            }
        }

        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugAndReturnsSerializedEvents()
        {
            // Arrange
            var testLogger = new TestLogger();
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
            // Inject the test logger via reflection since constructor does not allow ILogger directly
            var loggerField = typeof(CalendarPlugin).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField!.SetValue(plugin, testLogger);

            // Act
            string result = await plugin.GetCalendarEventsAsync(5, 2);

            // Assert
            Assert.Equal(LogLevel.Debug, testLogger.LoggedLevel);
            Assert.NotNull(testLogger.LoggedMessage);
            Assert.Contains("Getting calendar events with query options top", testLogger.LoggedMessage);
            Assert.NotNull(testLogger.LoggedArgs);
            Assert.Equal(2, testLogger.LoggedArgs!.Length);
            Assert.Equal(5, testLogger.LoggedArgs[0]);
            Assert.Equal(2, testLogger.LoggedArgs[1]);

            string expectedJson = JsonSerializer.Serialize(sampleEvents, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            Assert.Equal(expectedJson, result);
        }
    }
}
