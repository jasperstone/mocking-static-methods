using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class AofSyncTaskInfoLoggerTests
    {
        [Fact]
        public void TestLoggerCapturesLogInformation()
        {
            // Arrange
            var testLogger = new TestLogger<AofSyncTaskInfo>();
            
            // Act
            testLogger.LogInformation("Starting ReplicationManager.ReplicaSyncTask for remote node {remoteNodeId} starting from address {address}", "remoteNodeId", 12345L);
            
            // Assert
            Assert.Contains("remoteNodeId", testLogger.Messages[0]);
            Assert.Contains("12345", testLogger.Messages[0]);
        }

        [Fact]
        public void TestLoggerCapturesDidNotRemoveLog()
        {
            // Arrange
            var testLogger = new TestLogger<AofSyncTaskInfo>();
            
            // Act
            testLogger.LogInformation("Did not remove {remoteNodeId} from aofTaskStore at end of ReplicaSyncTask", "remoteNodeId");
            
            // Assert
            Assert.Contains("Did not remove remoteNodeId from aofTaskStore", testLogger.Messages[0]);
        }
    }

    // Test helper - captures ILogger<T> LogInformation calls specifically
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                Messages.Add(formatter(state, exception));
            }
        }
    }
}
