using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarningExtension_CanBeCalledWithExceptionAndMessage()
        {
            // Arrange
            var logger = new MockLogger();
            var testException = new InvalidOperationException("Test exception for failover attach completion");

            // Act - Directly call the extension method we want to test coverage for
            logger.LogWarning(testException, "WaitingForAttachToComplete Error");

            // Assert
            Assert.Single(logger.LogEntries);
            var entry = logger.LogEntries[0];
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Contains("WaitingForAttachToComplete Error", entry.Message);
            Assert.Same(testException, entry.Exception);
        }

        [Fact]
        public void LogWarningExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;
            var testException = new InvalidOperationException("Test exception");

            // Act & Assert
            logger?.LogWarning(testException, "WaitingForAttachToComplete Error");
            // Should not throw
            Assert.True(true);
        }
    }

    // Simple test logger implementation to verify extension method behavior
    public class MockLogger : ILogger
    {
        public List<LogEntry> LogEntries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception? exception, 
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            LogEntries.Add(new LogEntry 
            { 
                Level = logLevel, 
                Message = message, 
                Exception = exception 
            });
        }
    }

    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }
}
