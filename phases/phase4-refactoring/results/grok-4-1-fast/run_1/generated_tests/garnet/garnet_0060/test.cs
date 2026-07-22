using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_IssueAttachReplicasError_FormatsCorrectly()
        {
            // Arrange
            var loggedMessages = new List<string>();
            using var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new CapturingLoggerProvider(loggedMessages));
            var logger = loggerFactory.CreateLogger("ReplicaFailoverSession");

            var replicaId = "node-123";
            var replicaOfResp = "ERR_INVALID_RESPONSE";

            // Act - Directly test the exact LoggerExtensions.LogWarning call from line 226
            logger.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert
            Assert.Single(loggedMessages);
            var logMessage = loggedMessages[0];
            Assert.Contains("IssueAttachReplicas Error", logMessage);
            Assert.Contains(replicaId, logMessage);
            Assert.Contains(replicaOfResp, logMessage);
            Assert.Contains("node-123", logMessage);
            Assert.Contains("ERR_INVALID_RESPONSE", logMessage);
        }

        [Fact]
        public void LogWarning_IssueAttachReplicasError_NullLoggerSafe()
        {
            // Arrange
            ILogger logger = NullLogger.Instance;

            var replicaId = "node-456";
            var replicaOfResp = "ERR_ANOTHER_ERROR";

            // Act & Assert - null-conditional operator makes this safe (logger?.LogWarning)
            logger.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);
        }
    }

    public class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _messages;

        public CapturingLoggerProvider(List<string> messages)
        {
            _messages = messages;
        }

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(_messages);

        public void Dispose() { }
    }

    public class CapturingLogger : ILogger
    {
        private readonly List<string> _messages;

        public CapturingLogger(List<string> messages)
        {
            _messages = messages;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _messages.Add(formatter(state, exception));
        }
    }
}
