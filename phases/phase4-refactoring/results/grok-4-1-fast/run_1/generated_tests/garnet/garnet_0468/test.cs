using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class VectorManagerLoggerTests
    {
        [Fact]
        public void LogsError_WhenTryDeleteVectorSetThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var logger = loggerMock.Object;
            
            var key = "testkey";
            var exception = new InvalidOperationException("Test exception");

            // Act
            logger.LogError(exception, "Attempt at normal cleanup of {key} failed", key);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((Microsoft.Extensions.Logging.FormattedLogValues)v).ToString().Contains("Attempt at normal cleanup of testkey failed")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogsInformation_WhenDeleteSucceedsNormally()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var logger = loggerMock.Object;
            
            var key = "testkey";
            var ctx = 123u;

            // Act
            logger.LogInformation("Vector Set under {key} (context: {ctx}) deleted normally", key, ctx);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((Microsoft.Extensions.Logging.FormattedLogValues)v).ToString().Contains("Vector Set under testkey")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogsCritical_WhenCleanupDeleteFailsCompletely()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var logger = loggerMock.Object;
            
            var key = "testkey";
            var ctx = 123u;

            // Act
            logger.LogCritical("Failed to cleanup delete dropped Vector Set {key} (context: {ctx}), Vector Set will remain corrupted", key, ctx);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((Microsoft.Extensions.Logging.FormattedLogValues)v).ToString().Contains("Failed to cleanup delete dropped Vector Set testkey")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogsInformation_WhenCleaningUpInProgressDelete()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var logger = loggerMock.Object;
            
            var key = "testkey";
            var ctx = 123u;

            // Act
            logger.LogInformation("Cleaning up in progress Vector Set delete of {key} (context: {ctx})", key, ctx);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((Microsoft.Extensions.Logging.FormattedLogValues)v).ToString().Contains("Cleaning up in progress Vector Set delete of testkey")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
