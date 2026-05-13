using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Migrator;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private static readonly EventId BypassFiltersEventId = new EventId(0, "BypassFilters");

        [Fact]
        public void LogInformation_CallsLoggerLogInformationWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Test message {0}";
            object[] args = { 123 };
            string expectedMessage = string.Format(format, args);

            // Act
            dbUpLogger.LogInformation(format, args);

            // Assert
            mockLogger.Verify(logger => logger.LogInformation(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                It.Is<string>(s => s == "{InfoMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }

        [Fact]
        public void LogTrace_CallsLoggerLogTraceWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Trace {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            dbUpLogger.LogTrace(format, args);

            mockLogger.Verify(logger => logger.LogTrace(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                It.Is<string>(s => s == "{TraceMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }

        [Fact]
        public void LogDebug_CallsLoggerLogDebugWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Debug {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            dbUpLogger.LogDebug(format, args);

            mockLogger.Verify(logger => logger.LogDebug(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                It.Is<string>(s => s == "{DebugMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }

        [Fact]
        public void LogWarning_CallsLoggerLogWarningWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Warning {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            dbUpLogger.LogWarning(format, args);

            mockLogger.Verify(logger => logger.LogWarning(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                It.Is<string>(s => s == "{WarningMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }

        [Fact]
        public void LogError_StringOverload_CallsLoggerLogErrorWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Error {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            dbUpLogger.LogError(format, args);

            mockLogger.Verify(logger => logger.LogError(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                It.Is<string>(s => s == "{ErrorMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }

        [Fact]
        public void LogError_ExceptionOverload_CallsLoggerLogErrorWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            var ex = new Exception("Test exception");
            string format = "Error {0}";
            object[] args = { "arg" };
            string expectedMessage = string.Format(format, args);

            dbUpLogger.LogError(ex, format, args);

            mockLogger.Verify(logger => logger.LogError(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                ex,
                It.Is<string>(s => s == "{ErrorMessage}"),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == expectedMessage)
            ), Times.Once);
        }
    }
}
