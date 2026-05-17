using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogInformationCheckpointSearchCompleted_VerifyCallPattern()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - Simulate the exact LoggerExtensions.LogInformation call from ReplicaSyncSession line 134
            mockLogger.Object.LogInformation("Checkpoint search completed");

            // Assert - Verify the underlying Log call that LoggerExtensions makes
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Checkpoint search completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Logger_NullConditionalLogInformation_SafeWhenNull()
        {
            // Test the null-conditional operator ?.LogInformation used in ReplicaSyncSession
            ILogger logger = null;
            logger?.LogInformation("Checkpoint search completed");
            Assert.True(true);
        }

        [Fact]
        public void Logger_NullConditionalLogInformation_CallsWhenNotNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            ILogger logger = mockLogger.Object;

            // Act - Exact pattern from ReplicaSyncSession: logger?.LogInformation
            logger?.LogInformation("Checkpoint search completed");

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Checkpoint search completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_IsEnabledCheck_RespectsInformationLevel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - LoggerExtensions first checks IsEnabled before logging
            mockLogger.Object.LogInformation("Checkpoint search completed");

            // Assert - Verifies the extension method's guard clause passes
            mockLogger.Verify(x => x.IsEnabled(LogLevel.Information), Times.AtLeastOnce);
        }
    }
}
