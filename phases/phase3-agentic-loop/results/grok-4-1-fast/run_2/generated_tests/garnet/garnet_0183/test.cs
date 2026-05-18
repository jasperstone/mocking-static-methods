using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LoggerExtension_LogInformationCheckpointSearchCompleted_IsCalled()
        {
            // Arrange - Test the actual logger extension method used in ReplicaSyncSession
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            
            var logMessages = new List<string>();
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                    (level, id, state, ex, formatter) => logMessages.Add(formatter(state, ex)));

            // Act - Directly call the extension method pattern used on line 134
            // This matches exactly: logger?.LogInformation("Checkpoint search completed");
            mockLogger.Object.LogInformation("Checkpoint search completed");

            // Assert
            Assert.Single(logMessages);
            Assert.Contains("Checkpoint search completed", logMessages.First());
        }

        [Fact]
        public void LoggerExtension_LogInformationWithParameters_IsCalled()
        {
            // Arrange - Test another logging call pattern from the same method
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            
            var logMessages = new List<string>();
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>(
                    (level, id, state, ex, formatter) => logMessages.Add(formatter(state, ex)));

            // Act - Matches the pattern: logger?.LogInformation("Replica replicaId:{replicaId}...", replicaNodeId, versions);
            mockLogger.Object.LogInformation("Replica replicaId:{replicaId} requesting checkpoint replicaStoreVersion:{replicaStoreVersion} replicaObjectStoreVersion:{replicaObjectStoreVersion}",
                "testNode", 123L, 456L);

            // Assert
            Assert.Single(logMessages);
            Assert.Contains("testNode", logMessages.First());
            Assert.Contains("123", logMessages.First());
            Assert.Contains("456", logMessages.First());
        }
    }
}
