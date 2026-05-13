using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Critical_CallsLogCritical()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Critical message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            mockLogger.Verify(x => x.LogCritical(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Error_CallsLogError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Error message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Error, message);

            // Assert
            mockLogger.Verify(x => x.LogError(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Warning_CallsLogWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Warning message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Warning, message);

            // Assert
            mockLogger.Verify(x => x.LogWarning(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Information_CallsLogInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Information message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Information, message);

            // Assert
            mockLogger.Verify(x => x.LogInformation(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Trace_CallsLogTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Trace message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Trace, message);

            // Assert
            mockLogger.Verify(x => x.LogTrace(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Default_CallsLogDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Default message";

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.None, message);

            // Assert
            mockLogger.Verify(x => x.LogDebug(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Critical_CallsLogCriticalWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Critical exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Error_CallsLogErrorWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Error exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Error, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogError(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Warning_CallsLogWarningWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Warning exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Warning, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogWarning(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Information_CallsLogInformationWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Information exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Information, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogInformation(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Trace_CallsLogTraceWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Trace exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.Trace, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogTrace(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Default_CallsLogDebugWithException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var message = "Default exception message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWithLevel(LogLevel.None, message, exception);

            // Assert
            mockLogger.Verify(x => x.LogDebug(exception, message), Times.Once);
        }
    }
}
