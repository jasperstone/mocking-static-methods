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
        public void LogWithLevel_ShouldCallLogCritical_WhenLogLevelIsCritical()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Test message";
            var exception = new Exception("Test exception");

            // Act
            AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message) && v.ToString().Contains(exception.Message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogException_ShouldCallLogCritical_WhenExceptionHasCriticalLogLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Mock<Exception>();
            exception.Setup(ex => ex.GetLogLevel()).Returns(LogLevel.Critical);
            exception.Setup(ex => ex.Message).Returns("Test exception message");

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(exception.Object.Message)),
                    exception.Object,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
