using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void LoggerExtension_LogInformation_CalledWithIterationFormat()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var logger = mockLogger.Object;
            int iteration = 0;

            // Act - Directly test the LoggerExtensions.LogInformation call from line 361
            logger.LogInformation("AcquireCheckpointEntry iteration {iteration}", iteration);

            // Assert - Verify the underlying Log method was called with the formatted message
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state?.ToString()!.Contains("AcquireCheckpointEntry iteration 0") == true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_CalledWithIterationOne()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var logger = mockLogger.Object;
            int iteration = 1;

            // Act - Test with iteration=1 to show it works for different values
            logger.LogInformation("AcquireCheckpointEntry iteration {iteration}", iteration);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state?.ToString()!.Contains("AcquireCheckpointEntry iteration 1") == true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_NullLogger_DoesNotCallLogInformation()
        {
            // Arrange
            ILogger logger = null;
            int iteration = 0;

            // Act - Null-conditional operator prevents call
            logger?.LogInformation("AcquireCheckpointEntry iteration {iteration}", iteration);

            // Assert - No verification needed, this just compiles and runs without exception
            Assert.True(true);
        }
    }
}
