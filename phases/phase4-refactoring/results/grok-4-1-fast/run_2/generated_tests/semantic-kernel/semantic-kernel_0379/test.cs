using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Functions.Tests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogWarningExtension_CanBeCalledOnILogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();

        // Act
        loggerMock.Object.LogWarning("Test warning message");

        // Assert - No exception thrown
        Assert.True(true);
    }

    [Fact]
    public void LogWarningExtension_CapturesCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAny<string>, object?[]?>(state => state.ToString()!.Contains("Unable to get token details")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAny<string>, Exception?, string>>()))
            .Verifiable();

        // Act
        loggerMock.Object.LogWarning("Unable to get token details from model result.");

        // Assert
        loggerMock.VerifyAll();
    }

    [Fact]
    public void LogWarningExtension_WithEventIdAndException_Works()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var exception = new Exception("Test exception");

        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.Is<EventId>(e => e.Id == 0),
            It.IsAny<It.IsAny<string>, object?[]?>(),
            exception,
            It.IsAny<Func<It.IsAny<string>, Exception?, string>>()))
            .Verifiable();

        // Act
        loggerMock.Object.LogWarning(0, exception, "Test warning");

        // Assert
        loggerMock.VerifyAll();
    }

    [Fact]
    public void LogWarningExtension_WithFormattedMessage_Works()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAny<string>, object?[]?>(state => state.ToString()!.Contains("param")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAny<string>, Exception?, string>>()))
            .Verifiable();

        // Act
        loggerMock.Object.LogWarning("Warning with {Param}", "param");

        // Assert
        loggerMock.VerifyAll();
    }
}
