using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Migrator;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly DbUpLogger _dbUpLogger;

        public DbUpLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _dbUpLogger = new DbUpLogger(_mockLogger.Object);
        }

        [Fact]
        public void LogInformation_CallsLoggerLogInformationWithCorrectParameters()
        {
            // Arrange
            string format = "Test message {0}";
            object[] args = { 123 };
            string expectedMessage = string.Format(format, args);

            // Act
            _dbUpLogger.LogInformation(format, args);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation(
                It.IsAny<EventId>(),
                "{InfoMessage}",
                expectedMessage), Times.Once);
        }

        [Fact]
        public void LogTrace_CallsLoggerLogTraceWithCorrectParameters()
        {
            string format = "Trace message {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            _dbUpLogger.LogTrace(format, args);

            _mockLogger.Verify(logger => logger.LogTrace(
                It.IsAny<EventId>(),
                "{TraceMessage}",
                expectedMessage), Times.Once);
        }

        [Fact]
        public void LogDebug_CallsLoggerLogDebugWithCorrectParameters()
        {
            string format = "Debug message {0}";
            object[] args = { 42 };
            string expectedMessage = string.Format(format, args);

            _dbUpLogger.LogDebug(format, args);

            _mockLogger.Verify(logger => logger.LogDebug(
                It.IsAny<EventId>(),
                "{DebugMessage}",
                expectedMessage), Times.Once);
        }

        [Fact]
        public void LogWarning_CallsLoggerLogWarningWithCorrectParameters()
        {
            string format = "Warning message {0}";
            object[] args = { "warn" };
            string expectedMessage = string.Format(format, args);

            _dbUpLogger.LogWarning(format, args);

            _mockLogger.Verify(logger => logger.LogWarning(
                It.IsAny<EventId>(),
                "{WarningMessage}",
                expectedMessage), Times.Once);
        }

        [Fact]
        public void LogError_CallsLoggerLogErrorWithCorrectParameters()
        {
            string format = "Error message {0}";
            object[] args = { "error" };
            string expectedMessage = string.Format(format, args);

            _dbUpLogger.LogError(format, args);

            _mockLogger.Verify(logger => logger.LogError(
                It.IsAny<EventId>(),
                "{ErrorMessage}",
                expectedMessage), Times.Once);
        }

        [Fact]
        public void LogError_WithException_CallsLoggerLogErrorWithException()
        {
            var ex = new Exception("Test exception");
            string format = "Error with exception {0}";
            object[] args = { "ex" };
            string expectedMessage = string.Format(format, args);

            _dbUpLogger.LogError(ex, format, args);

            _mockLogger.Verify(logger => logger.LogError(
                It.IsAny<EventId>(),
                ex,
                "{ErrorMessage}",
                expectedMessage), Times.Once);
        }
    }
}
