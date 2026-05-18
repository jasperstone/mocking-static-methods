using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public AbpLoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogWithLevel_Critical_CallsLogCritical()
        {
            // Arrange
            var message = "Critical message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            _loggerMock.Verify(x => x.LogCritical(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Error_CallsLogError()
        {
            // Arrange
            var message = "Error message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Error, message);

            // Assert
            _loggerMock.Verify(x => x.LogError(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Warning_CallsLogWarning()
        {
            // Arrange
            var message = "Warning message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Warning, message);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Information_CallsLogInformation()
        {
            // Arrange
            var message = "Information message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Information, message);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Trace_CallsLogTrace()
        {
            // Arrange
            var message = "Trace message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Trace, message);

            // Assert
            _loggerMock.Verify(x => x.LogTrace(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Default_CallsLogDebug()
        {
            // Arrange
            var message = "Debug message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.None, message);

            // Assert
            _loggerMock.Verify(x => x.LogDebug(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Critical_CallsLogCriticalWithException()
        {
            // Arrange
            var message = "Critical exception";
            var exception = new Exception("Critical error");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Error_CallsLogErrorWithException()
        {
            // Arrange
            var message = "Error exception";
            var exception = new Exception("Error occurred");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Error, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogError(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Warning_CallsLogWarningWithException()
        {
            // Arrange
            var message = "Warning exception";
            var exception = new Exception("Warning occurred");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Warning, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogWarning(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Information_CallsLogInformationWithException()
        {
            // Arrange
            var message = "Information exception";
            var exception = new Exception("Info occurred");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Information, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Trace_CallsLogTraceWithException()
        {
            // Arrange
            var message = "Trace exception";
            var exception = new Exception("Trace info");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Trace, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogTrace(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_WithException_Default_CallsLogDebugWithException()
        {
            // Arrange
            var message = "Debug exception";
            var exception = new Exception("Debug info");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.None, message, exception);

            // Assert
            _loggerMock.Verify(x => x.LogDebug(exception, message), Times.Once);
        }
    }
}
