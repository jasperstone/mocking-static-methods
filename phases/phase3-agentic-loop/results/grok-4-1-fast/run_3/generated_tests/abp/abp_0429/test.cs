using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using Xunit;

namespace Volo.Abp.Logging.Tests;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _mockLogger;

    public AbpLoggerExtensionsTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Critical;
        var message = "Critical error occurred";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Critical,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogError_When_LogLevel_Is_Error_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Error;
        var message = "Error message";
        var exception = new ArgumentException("Test arg exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogWarning_When_LogLevel_Is_Warning_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Warning;
        var message = "Warning message";
        var exception = new NotImplementedException();

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Warning,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogInformation_When_LogLevel_Is_Information_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Information;
        var message = "Info message";
        var exception = new Exception("Info exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_When_LogLevel_Is_Trace_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Trace;
        var message = "Trace message";
        var exception = new Exception("Trace exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_When_LogLevel_Is_Debug_WithException()
    {
        // Arrange
        var logLevel = LogLevel.Debug;
        var message = "Debug message";
        var exception = new Exception("Debug exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_When_LogLevel_Is_None_WithException()
    {
        // Arrange
        var logLevel = LogLevel.None;
        var message = "None message";
        var exception = new Exception("None exception");

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message, exception);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_When_LogLevel_Is_Critical_WithoutException()
    {
        // Arrange
        var logLevel = LogLevel.Critical;
        var message = "Critical message without exception";

        // Act
        _mockLogger.Object.LogWithLevel(logLevel, message);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Critical,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
