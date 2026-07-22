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
                logger => logger.LogCritical(
                    It.Is<EventId>(e => e.Id == 0),
                    exception,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogException_ShouldLogExceptionWithCriticalLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");
            var message = "Test message";

            // Act
            AbpLoggerExtensions.LogException(loggerMock.Object, exception, LogLevel.Critical);

            // Assert
            loggerMock.Verify(
                logger => logger.LogCritical(
                    It.Is<EventId>(e => e.Id == 0),
                    exception,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
