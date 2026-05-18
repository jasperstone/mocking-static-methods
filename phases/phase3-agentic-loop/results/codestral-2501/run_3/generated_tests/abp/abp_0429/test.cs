using System;
using Microsoft.Extensions.Logging;
using Moq;
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
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
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
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
