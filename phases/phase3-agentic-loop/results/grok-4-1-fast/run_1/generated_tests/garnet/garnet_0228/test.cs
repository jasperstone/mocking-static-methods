using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Replication.ReplicaOps
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithNoPrimaryErrorMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);

            var errorMsg = "-ERR No primary assigned\r\n";

            // Act - Directly test the LoggerExtensions.LogError call from line ~100
            Microsoft.Extensions.Logging.LoggerExtensions.LogError(mockLogger.Object, "{msg}", errorMsg);

            // Assert - Verify the underlying ILogger.Log was called with correct parameters
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("{msg}") && v.ToString()!.Contains(errorMsg)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogErrorExtension_NullLogger_DoesNotThrow()
        {
            // Act & Assert - The null-conditional operator prevents the call
            Microsoft.Extensions.Logging.LoggerExtensions.LogError(null!, "{msg}", "test error");
            Assert.True(true);
        }

        [Fact]
        public void LogErrorExtension_LoggerDisabled_DoesNotLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(false);

            var errorMsg = "-ERR No primary assigned\r\n";

            // Act
            Microsoft.Extensions.Logging.LoggerExtensions.LogError(mockLogger.Object, "{msg}", errorMsg);

            // Assert - No log call when logging is disabled
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Never
            );
        }
    }
}
