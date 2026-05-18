using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock;

    public AbpLoggerExtensionsTests()
    {
        _loggerMock = new Mock<ILogger>();
        // Setup the generic Log method that all extension methods call internally
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithException()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Critical error occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

        // Assert - verify the generic Log was called with LogLevel.Critical
        _loggerMock.Verify(x => x.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithoutException()
    {
        // Arrange
        var message = "Critical error occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

        // Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_Correct_LogLevels_For_Different_Levels_WithException()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Test message";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Error, message, exception);
        _loggerMock.Object.LogWithLevel(LogLevel.Warning, message, exception);
        _loggerMock.Object.LogWithLevel(LogLevel.Information, message, exception);
        _loggerMock.Object.LogWithLevel(LogLevel.Trace, message, exception);
        _loggerMock.Object.LogWithLevel(LogLevel.Debug, message, exception);

        // Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(x => x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogException_Should_Call_LogWithLevel_With_CriticalLevel()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _loggerMock.Object.LogException(exception, LogLevel.Critical);

        // Assert - verifies it calls LogWithLevel(Critical, message, exception) which triggers LogCritical internally
        _loggerMock.Verify(x => x.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_For_Default_LogLevel()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Test message";

        // Act
        _loggerMock.Object.LogWithLevel((LogLevel)999, message, exception); // Invalid level falls to default

        // Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
