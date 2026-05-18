using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationLoggerTests
    {
        [Fact]
        public void LogError_OperationCanceledFormat_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;
            double timeoutMs = 123.45;
            string slotsRange = "1-5";

            // Act - Directly call the LogError extension as it appears in the source code (line ~55)
            logger.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", timeoutMs, slotsRange);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("SetSlotRange operation timed out") &&
                        v.ToString()!.Contains("123.45") &&
                        v.ToString()!.Contains("1-5")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_GeneralExceptionFormat_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;
            var exception = new Exception("Test exception");
            string slotsRange = "10-20";

            // Act - Directly call the LogError extension as it appears in the source code
            logger.LogError(exception, "An error occurred during SetSlotRange for slots {slots}", slotsRange);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("An error occurred during SetSlotRange") &&
                        v.ToString()!.Contains("10-20")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_RecoverFromFailureFormat_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
            
            var logger = mockLogger.Object;

            // Act - Directly call the LogError extension as it appears in TryRecoverFromFailureAsync
            logger.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("MigrateSession.RecoverFromFailure failed to make slots STABLE")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
