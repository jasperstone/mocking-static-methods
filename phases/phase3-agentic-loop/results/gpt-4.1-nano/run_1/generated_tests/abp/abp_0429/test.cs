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
            var mockLogger = new Mock<ILogger>();
            var message = "Error message";

            mockLogger.Object.LogWithLevel(LogLevel.Error, message);

            mockLogger.Verify(x => x.LogError(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Warning_CallsLogWarning()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Warning message";

            mockLogger.Object.LogWithLevel(LogLevel.Warning, message);

            mockLogger.Verify(x => x.LogWarning(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Information_CallsLogInformation()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Information message";

            mockLogger.Object.LogWithLevel(LogLevel.Information, message);

            mockLogger.Verify(x => x.LogInformation(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Trace_CallsLogTrace()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Trace message";

            mockLogger.Object.LogWithLevel(LogLevel.Trace, message);

            mockLogger.Verify(x => x.LogTrace(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Default_CallsLogDebug()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Debug message";

            mockLogger.Object.LogWithLevel(LogLevel.Debug, message);

            mockLogger.Verify(x => x.LogDebug(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Critical_CallsLogCriticalWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Critical exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Critical, message, exception);

            mockLogger.Verify(x => x.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Error_CallsLogErrorWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Error exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Error, message, exception);

            mockLogger.Verify(x => x.LogError(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Warning_CallsLogWarningWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Warning exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Warning, message, exception);

            mockLogger.Verify(x => x.LogWarning(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Information_CallsLogInformationWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Information exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Information, message, exception);

            mockLogger.Verify(x => x.LogInformation(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Trace_CallsLogTraceWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Trace exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Trace, message, exception);

            mockLogger.Verify(x => x.LogTrace(exception, message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_Exception_Default_CallsLogDebugWithException()
        {
            var mockLogger = new Mock<ILogger>();
            var message = "Debug exception";
            var exception = new InvalidOperationException();

            mockLogger.Object.LogWithLevel(LogLevel.Debug, message, exception);

            mockLogger.Verify(x => x.LogDebug(exception, message), Times.Once);
        }
    }
}
