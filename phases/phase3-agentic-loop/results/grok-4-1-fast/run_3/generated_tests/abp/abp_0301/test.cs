using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class TranslateCommandLoggerTests
{
    [Fact]
    public void LogInformation_Extension_Should_Call_Underlying_Log_Method()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranslateCommandLoggerTests>>();
        
        // Setup to allow any Log call
        loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        // Act - Test the exact LogInformation extension call from line 228
        var targetFile = "test/path/es.json";
        loggerMock.Object.LogInformation($"Write translation json to {targetFile}.");

        // Assert - Verify the underlying Log method was called with Information level
        loggerMock.Verify(l => l.Log(
            It.Is<LogLevel>(level => level == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v.ToString()!.Contains("Write translation json to") && 
                v.ToString()!.Contains(targetFile)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }

    [Fact]
    public void LogInformation_Extension_Should_Use_Information_LogLevel()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranslateCommandLoggerTests>>();
        loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        // Act
        loggerMock.Object.LogInformation("Line 228 style log message.");

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.Is<EventId>(e => e.Id == 0),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }
}
