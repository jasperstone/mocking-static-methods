using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.ExceptionHandling;
using Xunit;

namespace Volo.Abp.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock;

    public AbpLoggerExtensionsTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogCritical_WithException_When_LogLevel_Is_Critical()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Critical error occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Critical, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Critical,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Critical error occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogError_WithException_When_LogLevel_Is_Error()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Error occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Error, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogWarning_WithException_When_LogLevel_Is_Warning()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Warning occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Warning, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Warning occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogInformation_WithException_When_LogLevel_Is_Information()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Info occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Information, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Info occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_WithException_When_LogLevel_Is_Debug()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Debug occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Debug, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Debug occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogDebug_WithException_When_LogLevel_Is_None()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "None level occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.None, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("None level occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_LogTrace_WithException_When_LogLevel_Is_Trace()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var message = "Trace occurred";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Trace, message, exception);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Trace occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_Should_Call_CorrectMethods_WithoutException()
    {
        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Critical, "Critical message");
        _loggerMock.Object.LogWithLevel(LogLevel.Error, "Error message");
        _loggerMock.Object.LogWithLevel(LogLevel.Warning, "Warning message");
        _loggerMock.Object.LogWithLevel(LogLevel.Information, "Info message");
        _loggerMock.Object.LogWithLevel(LogLevel.Debug, "Debug message");
        _loggerMock.Object.LogWithLevel(LogLevel.None, "None message");
        _loggerMock.Object.LogWithLevel(LogLevel.Trace, "Trace message");

        // Assert
        _loggerMock.Verify(l => l.Log(LogLevel.Critical, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(l => l.Log(LogLevel.Warning, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _loggerMock.Verify(l => l.Log(LogLevel.Debug, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));
        _loggerMock.Verify(l => l.Log(LogLevel.Trace, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
