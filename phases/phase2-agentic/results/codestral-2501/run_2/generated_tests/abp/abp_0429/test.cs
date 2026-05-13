using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.ExceptionHandling;
using Xunit;

namespace Volo.Abp.Core.Tests.Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_ShouldLogCriticalMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test critical message";

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogWithLevel_ShouldLogCriticalMessageWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test critical message";
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Critical),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                It.Is<Exception>(ex => ex == exception),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogExceptionWithDefaultLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == exception.Message),
                It.Is<Exception>(ex => ex == exception),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogExceptionWithSpecifiedLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var logLevel = LogLevel.Warning;

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, logLevel);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == exception.Message),
                It.Is<Exception>(ex => ex == exception),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogKnownProperties_ShouldLogErrorCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Mock<Exception>();
            var errorCodeException = new Mock<IHasErrorCode>();
            errorCodeException.Setup(e => e.Code).Returns("TestCode");
            exception.As<IHasErrorCode>().Setup(e => e.Code).Returns(errorCodeException.Object.Code);
            var logLevel = LogLevel.Error;

            // Act
            AbpLoggerExtensions.LogKnownProperties(loggerMock.Object, exception.Object, logLevel);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Code:TestCode"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogKnownProperties_ShouldLogErrorDetails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Mock<Exception>();
            var errorDetailsException = new Mock<IHasErrorDetails>();
            errorDetailsException.Setup(e => e.Details).Returns("TestDetails");
            exception.As<IHasErrorDetails>().Setup(e => e.Details).Returns(errorDetailsException.Object.Details);
            var logLevel = LogLevel.Error;

            // Act
            AbpLoggerExtensions.LogKnownProperties(loggerMock.Object, exception.Object, logLevel);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Details:TestDetails"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void LogData_ShouldLogExceptionData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            exception.Data["Key1"] = "Value1";
            exception.Data["Key2"] = "Value2";
            var logLevel = LogLevel.Error;

            // Act
            AbpLoggerExtensions.LogData(loggerMock.Object, exception, logLevel);

            // Assert
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Key1 = Value1") && v.ToString().Contains("Key2 = Value2")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
