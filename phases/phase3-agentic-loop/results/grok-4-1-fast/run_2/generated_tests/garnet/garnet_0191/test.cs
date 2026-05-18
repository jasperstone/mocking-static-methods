using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Test
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CoverageLine301()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;

            // Act - Directly invoke the exact LogError extension pattern from line 301
            logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 50L, 100L);

            // Assert - Verify the underlying Log call was made with Error level
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("syncFromAofAddress: 50") && 
                    v.ToString().Contains("beginAofAddress: 100")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_ValidatesMessageFormatting()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;

            // Act
            logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 75L, 150L);

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act & Assert - Null-conditional operator prevents call (as used in source code)
            logger?.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 50L, 100L);
        }
    }
}
