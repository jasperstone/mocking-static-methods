using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.ExceptionHandling;
using Xunit;

namespace Volo.Abp.Core.Tests.Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public AbpLoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogWithLevel_ShouldLogCritical()
        {
            // Arrange
            var message = "Test message";

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            _loggerMock.Verify(logger => logger.LogCritical(message), Times.Once);
        }

        [Fact]
        public void LogWithLevel_ShouldLogCriticalWithException()
        {
            // Arrange
            var message = "Test message";
            var exception = new Exception("Test exception");

            // Act
            _loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

            // Assert
            _loggerMock.Verify(logger => logger.LogCritical(exception, message), Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogWithLevel()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var logLevel = LogLevel.Error;

            // Act
            _loggerMock.Object.LogException(exception, logLevel);

            // Assert
            _loggerMock.Verify(logger => logger.LogWithLevel(logLevel, exception.Message, exception), Times.Once);
        }

        [Fact]
        public void LogKnownProperties_ShouldLogErrorCode()
        {
            // Arrange
            var exception = new Mock<Exception>();
            var hasErrorCode = new Mock<IHasErrorCode>();
            hasErrorCode.Setup(e => e.Code).Returns("TestCode");
            exception.As<IHasErrorCode>().Setup(e => e.Code).Returns(hasErrorCode.Object.Code);

            // Act
            AbpLoggerExtensions.LogKnownProperties(_loggerMock.Object, exception.Object, LogLevel.Error);

            // Assert
            _loggerMock.Verify(logger => logger.LogWithLevel(LogLevel.Error, "Code:TestCode"), Times.Once);
        }

        [Fact]
        public void LogKnownProperties_ShouldLogErrorDetails()
        {
            // Arrange
            var exception = new Mock<Exception>();
            var hasErrorDetails = new Mock<IHasErrorDetails>();
            hasErrorDetails.Setup(e => e.Details).Returns("TestDetails");
            exception.As<IHasErrorDetails>().Setup(e => e.Details).Returns(hasErrorDetails.Object.Details);

            // Act
            AbpLoggerExtensions.LogKnownProperties(_loggerMock.Object, exception.Object, LogLevel.Error);

            // Assert
            _loggerMock.Verify(logger => logger.LogWithLevel(LogLevel.Error, "Details:TestDetails"), Times.Once);
        }

        [Fact]
        public void LogData_ShouldLogExceptionData()
        {
            // Arrange
            var exception = new Exception("Test exception");
            exception.Data["Key1"] = "Value1";
            exception.Data["Key2"] = "Value2";

            // Act
            AbpLoggerExtensions.LogData(_loggerMock.Object, exception, LogLevel.Error);

            // Assert
            _loggerMock.Verify(logger => logger.LogWithLevel(LogLevel.Error, It.IsAny<string>()), Times.Once);
        }
    }
}
