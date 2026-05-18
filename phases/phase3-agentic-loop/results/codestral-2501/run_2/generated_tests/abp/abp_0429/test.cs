using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class AbpLoggerExtensionsTests
{
    [Fact]
    public void LogWithLevel_LogsCriticalWithException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var exception = new Exception("Test exception");
        var message = "Test message";

        // Act
        AbpLoggerExtensions.LogWithLevel(loggerMock.Object, LogLevel.Critical, message, exception);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogException_LogsCriticalWithException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var exception = new Exception("Test exception");

        // Act
        AbpLoggerExtensions.LogException(loggerMock.Object, exception, LogLevel.Critical);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == exception.Message),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
