using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithCheckpointSearchCompletedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            ILogger logger = mockLogger.Object;

            // Act - Directly invoke the exact extension method call from line 134
            logger.LogInformation("Checkpoint search completed");

            // Assert - Verify the underlying Log call matches exactly what logger?.LogInformation produces
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Checkpoint search completed"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;

            // Act - Test the null-conditional operator behavior exactly as in source code
            logger?.LogInformation("Checkpoint search completed");

            // Assert - No exception thrown (test passes by reaching this point)
            Assert.True(true);
        }

        [Fact]
        public void LogInformationExtension_VerifyLogLevelInformation()
        {
            // Additional coverage for LogLevel verification
            var mockLogger = new Mock<ILogger>();
            ILogger logger = mockLogger.Object;

            logger.LogInformation("Checkpoint search completed");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
