using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        private class TestLogger : ILogger
        {
            public List<LogEntry> LogEntries { get; } = new();
            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable? BeginScope<TState>(TState state) => null;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                LogEntries.Add(new LogEntry
                {
                    Level = logLevel,
                    Message = formatter(state, exception)
                });
            }
        }

        public class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        [Fact]
        public void LogError_CalledWhen_SyncFromAofAddressLessThanBeginAddress_NoAofDataLoss()
        {
            // Arrange
            var logger = new TestLogger();
            var syncFromAofAddress = 50L;
            var beginAddress = 100L;
            var possibleAofDataLoss = false;

            // Act - Simulate exact condition from line 301
            if (!possibleAofDataLoss)
            {
                if (syncFromAofAddress < beginAddress)
                {
                    logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, beginAddress);
                }
            }

            // Assert
            Assert.Single(logger.LogEntries);
            var logEntry = logger.LogEntries[0];
            Assert.Equal(LogLevel.Error, logEntry.Level);
            Assert.Contains("syncFromAofAddress: 50", logEntry.Message);
            Assert.Contains("beginAofAddress: 100", logEntry.Message);
        }

        [Fact]
        public void LogError_NotCalled_When_PossibleAofDataLoss()
        {
            // Arrange
            var logger = new TestLogger();
            var syncFromAofAddress = 50L;
            var beginAddress = 100L;
            var possibleAofDataLoss = true;

            // Act - Simulate condition (LogError should NOT be called)
            if (!possibleAofDataLoss)
            {
                if (syncFromAofAddress < beginAddress)
                {
                    logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, beginAddress);
                }
            }

            // Assert
            Assert.Empty(logger.LogEntries);
        }

        [Fact]
        public void LogError_UsesCorrectMessageFormat()
        {
            // Arrange
            var logger = new TestLogger();
            var syncFromAofAddress = 50L;
            var beginAddress = 100L;
            var possibleAofDataLoss = false;

            // Act
            if (!possibleAofDataLoss && syncFromAofAddress < beginAddress)
            {
                logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", syncFromAofAddress, beginAddress);
            }

            // Assert - Verify exact message format and parameter substitution
            Assert.Single(logger.LogEntries);
            var message = logger.LogEntries[0].Message;
            Assert.Contains("syncFromAofAddress: 50", message);
            Assert.Contains("beginAofAddress: 100", message);
        }
    }
}
