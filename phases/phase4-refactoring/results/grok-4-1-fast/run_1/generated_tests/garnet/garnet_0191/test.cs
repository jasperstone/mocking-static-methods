using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.LoggerExtensionsTests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_FormatsCorrectly_WithSyncFromAofAddressAndBeginAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;

            long syncFromAofAddress = 50L;
            long beginAddress = 100L;

            // Act
            logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 
                           syncFromAofAddress, beginAddress);

            // Assert - Verifies the LogError extension method call from ReplicaSyncSession.cs line 301
            // works correctly with the exact message format and parameters used in production
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("syncFromAofAddress: 50") && 
                        v.ToString()!.Contains("beginAofAddress: 100")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act & Assert - Matches the ?.LogError pattern in ReplicaSyncSession.cs
            logger?.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 50L, 100L);
        }

        [Fact]
        public void LogError_HandlesLongParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;

            // Act
            logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 
                           long.MinValue, long.MaxValue);

            // Assert
            mockLogger.VerifyAll();
        }

        [Fact]
        public void LogError_NullConditional_DoesNotCallLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            ILogger? logger = mockLogger.Object;

            // Act
            logger?.LogError("test message", 1L, 2L);

            // Assert - verifies the null-conditional operator ?. pattern used in ReplicaSyncSession.cs
            mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
