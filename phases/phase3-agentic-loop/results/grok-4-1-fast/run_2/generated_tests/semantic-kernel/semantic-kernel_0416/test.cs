using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using System;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.UnitTests;

public class VarBlockTests
{
    [Fact]
    public void Constructor_LogsError_WhenContentTrimsToEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Since VarBlock is internal, test the base Block behavior and verify logger setup
        // The LogError call happens when Content.Length < 2 after trim
        Action act = () => _ = new VarBlock(" ", loggerFactoryMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidContent_DoesNotLogError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Act
        _ = new VarBlock("$valid", loggerFactoryMock.Object);

        // Assert - No error logged
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenNoLoggerFactory()
    {
        // Act & Assert - Should not throw
        var ex = Record.Exception(() => _ = new VarBlock(null));
        Assert.Null(ex);
    }
}
