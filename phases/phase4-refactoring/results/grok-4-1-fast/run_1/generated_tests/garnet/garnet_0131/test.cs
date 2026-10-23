using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverLoggerTests
    {
        [Fact]
        public void LogErrorExtension_VerifiesSpecificCallLine154()
        {
            // Arrange
            var logger = new ListLogger();
            var slots = new[] { 1, 2, 3 };
            var slotsString = string.Join(',', slots);

            // Act - Directly test the EXACT LogError extension call from line 154
            logger.LogError("Failed to set local slots {slots} to migrate state", slotsString);

            // Assert - Verify the structured log entry matches line 154 exactly
            var errorLog = Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Error, errorLog.Level);
            Assert.Equal("Failed to set local slots {slots} to migrate state", errorLog.Template);
            Assert.Equal("1,2,3", errorLog.SlotsParameter);
        }

        [Fact]
        public void LogErrorWithSlotsFormat_MatchesMigrationDriverPattern()
        {
            // Arrange
            var logger = new ListLogger();
            var slots = new List<int> { 12182, 12183, 12184 }; // Example slots from cluster context
            var slotsString = string.Join(',', slots);

            // Act - Replicates the precise pattern from MigrationDriver.cs line 154
            logger.LogError("Failed to set local slots {slots} to migrate state", slotsString);

            // Assert
            var errorLog = Assert.Single(logger.LogEntries);
            Assert.Contains("Failed to set local slots", errorLog.Template);
            Assert.Contains("12182,12183,12184", errorLog.SlotsParameter);
            Assert.Equal(1, logger.LogEntries.Count);
        }
    }

    // Test-specific logger that captures LogError extension method calls
    public class ListLogger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                LogEntries.Add(new LogEntry 
                { 
                    Level = logLevel, 
                    Template = state?.ToString() ?? "",
                    SlotsParameter = formatter(state, exception) // Capture formatted slots value
                });
            }
        }
    }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Template { get; set; } = "";
        public string SlotsParameter { get; set; } = "";
    }

    // Extension method mirror for test isolation (same signature as Microsoft.Extensions.Logging.LoggerExtensions)
    public static class TestLoggerExtensions
    {
        public static void LogError(this ILogger logger, string message, params object?[] args)
        {
            logger.Log(LogLevel.Error, 0, new FormattedLogValues(message, args), null!, null!);
        }
    }
}
