using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.ExceptionHandling;
using Xunit;

namespace Microsoft.Extensions.Logging;

public class AbpLoggerExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock;

    public AbpLoggerExtensionsTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public void LogWithLevel_CriticalWithException_CallsLogCritical()
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_ErrorWithException_CallsLogError()
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_WarningWithException_CallsLogWarning()
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_InformationWithException_CallsLogInformation()
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_DebugWithException_CallsLogDebug()
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWithLevel_CriticalWithoutException_CallsLogCritical()
    {
        // Arrange
        var message = "Critical message";

        // Act
        _loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Critical,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
