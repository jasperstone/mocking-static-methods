using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine;

public class VarBlockTests
{
    [Fact]
    public void VarBlock_LogError_WhenContentIsEmpty()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var varBlock = new VarBlock(string.Empty, loggerFactoryMock.Object);

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void VarBlock_LogError_WhenContentDoesNotStartWithVarPrefix()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var varBlock = new VarBlock("test", loggerFactoryMock.Object);

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void VarBlock_LogError_WhenContentLengthIsLessThan2()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var varBlock = new VarBlock("$", loggerFactoryMock.Object);

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }
}
