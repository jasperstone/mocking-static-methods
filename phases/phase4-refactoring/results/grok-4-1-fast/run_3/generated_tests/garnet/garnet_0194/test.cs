using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogInformation_CalledWithStructuredParams()
        {
            // Arrange - Test the exact LoggerExtensions.LogInformation call from line 361
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Simulate the structured logging call: logger?.LogInformation("AcquireCheckpointEntry iteration {iteration}", iteration);
            ((LoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "AcquireCheckpointEntry iteration {iteration}", 0);

            // Assert - Verifies the LogInformation extension method formats the message correctly
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("AcquireCheckpointEntry iteration 0")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogInformation_CalledWithIteration1()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act - Simulate retry iteration logging
            ((LoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "AcquireCheckpointEntry iteration {iteration}", 1);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("AcquireCheckpointEntry iteration 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogInformation_NullLogger_Safe()
        {
            // Arrange & Act - Null-conditional operator makes this safe (logger?.LogInformation)
            ILogger nullLogger = null;
            Action act = () => ((LoggerExtensions)nullLogger)?.LogInformation(nullLogger, "AcquireCheckpointEntry iteration {iteration}", 0);

            // Assert - No exception thrown
            act();
        }

        [Fact]
        public void LoggerExtensions_LogInformation_DisabledLevel_NotCalled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);

            // Act
            ((LoggerExtensions)mockLogger.Object).LogInformation(mockLogger.Object, "AcquireCheckpointEntry iteration {iteration}", 0);

            // Assert - Log method not called when level disabled
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
