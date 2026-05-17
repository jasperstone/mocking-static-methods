using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Logging;
using Xunit;

namespace Volo.Abp.Core.Tests.Extensions.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _mockLogger;

    public AbpLoggerExtensionsTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(m => m.Log(
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
        var message = "Critical error occurred";
        var exception = new InvalidOperationException("Test exception");
        var logger = _mockLogger.Object;

        // Act
        logger.LogWithLevel(LogLevel.Critical, message, exception);

        // Assert
        _mockLogger.Verify(m => m.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithoutException()
    {
        // Arrange
        var message = "Critical error occurred";
        var logger = _mockLogger.Object;

        // Act
        logger.LogWithLevel(LogLevel.Critical, message);

        // Assert
        _mockLogger.Verify(m => m.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_Correct_LogMethods_For_Different_LogLevels_WithException()
    {
        // Arrange
        var message = "Test message";
        var exception = new InvalidOperationException("Test");

        // Act & Assert
        _mockLogger.Object.LogWithLevel(LogLevel.Error, message, exception);
        _mockLogger.Verify(m => m.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Object.LogWithLevel(LogLevel.Warning, message, exception);
        _mockLogger.Verify(m => m.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Object.LogWithLevel(LogLevel.Information, message, exception);
        _mockLogger.Verify(m => m.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Object.LogWithLevel(LogLevel.Trace, message, exception);
        _mockLogger.Verify(m => m.Log(LogLevel.Trace, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Object.LogWithLevel(LogLevel.Debug, message, exception);
        _mockLogger.Verify(m => m.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_For_Default_LogLevels()
    {
        // Arrange
        var message = "Debug message";
        var logger = _mockLogger.Object;

        // Act
        logger.LogWithLevel(LogLevel.None, message);

        // Assert
        _mockLogger.Verify(m => m.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogException_Should_Call_LogWithLevel_When_Level_Not_Specified()
    {
        // Arrange
        var exception = new InvalidOperationException("Test");
        var logger = _mockLogger.Object;

        // Act
        logger.LogException(exception);

        // Assert
        _mockLogger.Verify(m => m.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void LogException_Should_Use_Specified_LogLevel()
    {
        // Arrange
        var exception = new ArgumentException("Test");
        var logger = _mockLogger.Object;

        // Act
        logger.LogException(exception, LogLevel.Critical);

        // Assert
        _mockLogger.Verify(m => m.Log(
            LogLevel.Critical,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }
}
