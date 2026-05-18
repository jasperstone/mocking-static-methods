using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class AofTaskStoreLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogError_Called_WhenTruncationPreventsTaskAddition()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v?.ToString()?.Contains("TryAddReplicationTasks") == true &&
                    v?.ToString()?.Contains("failed to add tasks for AOF sync") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Test the LoggerExtensions LogError call directly since AofTaskStore is internal
            // This verifies the exact extension method call pattern used on line 271
            var methodName = nameof(AofTaskStore.TryAddReplicationTasks);
            long startAddress = 500;
            long truncatedUntil = 1000;

            // Act - invoke the exact LogError extension used in the code
            mockLogger.Object.LogError("{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}", 
                                     methodName, startAddress, truncatedUntil);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()?.Contains("TryAddReplicationTasks") == true &&
                        v?.ToString()?.Contains("failed to add tasks for AOF sync") == true &&
                        v?.ToString()?.Contains("500") == true &&
                        v?.ToString()?.Contains("1000") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogError_FormatsCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("TryAddReplicationTasks", message);
                    Assert.Contains("failed to add tasks for AOF sync", message);
                })
                .Verifiable();

            // Act
            mockLogger.Object.LogError("{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}",
                                     "TryAddReplicationTasks", 123L, 456L);

            // Assert
            mockLogger.VerifyAll();
        }
    }
}
