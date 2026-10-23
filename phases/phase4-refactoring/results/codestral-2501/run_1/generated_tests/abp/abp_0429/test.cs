using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.ExceptionHandling;

namespace Volo.Abp.Core.Tests.Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_ShouldLogCriticalWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "Test message";

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(
                logger => logger.Log<It.IsAnyType>(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message) && v.ToString().Contains(exception.Message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogCriticalWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, LogLevel.Critical);

            // Assert
            loggerMock.Verify(
                logger => logger.Log<It.IsAnyType>(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(exception.Message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogKnownProperties()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new TestExceptionWithErrorCodeAndDetails
            {
                Code = "TestCode",
                Details = "TestDetails"
            };

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, LogLevel.Critical);

            // Assert
            loggerMock.Verify(
                logger => logger.Log<It.IsAnyType>(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Code:TestCode")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                logger => logger.Log<It.IsAnyType>(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Details:TestDetails")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class TestExceptionWithErrorCodeAndDetails : Exception, IHasErrorCode, IHasErrorDetails
    {
        public string? Code { get; set; }
        public string? Details { get; set; }
    }
}
