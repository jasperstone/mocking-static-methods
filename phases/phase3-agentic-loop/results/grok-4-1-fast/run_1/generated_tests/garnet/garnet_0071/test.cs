using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Cluster.Tests.Gossip
{
    public class LoggerExtensionsTests
    {
        public class TestLogger : ILogger
        {
            public List<(LogLevel level, Exception? ex, string msg)> Logs { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Logs.Add((logLevel, exception, formatter(state, exception)));
            }
        }

        [Fact]
        public void LogWarning_WhenCalledWithExceptionAndMessage_LogsWarningWithCorrectDetails()
        {
            // Arrange
            var logger = new TestLogger();
            var exception = new InvalidOperationException("Gossip task fault");

            // Act - Exactly matches line 252 usage: logger?.LogWarning(task.Exception, "GOSSIP round faulted")
            logger.LogWarning(exception, "GOSSIP round faulted");

            // Assert
            Assert.Single(logger.Logs);
            var logEntry = logger.Logs[0];
            Assert.Equal(LogLevel.Warning, logEntry.level);
            Assert.Equal("Gossip task fault", logEntry.ex!.Message);
            Assert.Contains("GOSSIP round faulted", logEntry.msg);
        }

        [Fact]
        public void LogWarning_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;
            var exception = new InvalidOperationException("test");

            // Act - Tests the null-conditional operator usage as in GarnetServerNode line 252
            logger?.LogWarning(exception, "GOSSIP round faulted");

            // Assert - No exception thrown
            Assert.True(true);
        }
    }
}
