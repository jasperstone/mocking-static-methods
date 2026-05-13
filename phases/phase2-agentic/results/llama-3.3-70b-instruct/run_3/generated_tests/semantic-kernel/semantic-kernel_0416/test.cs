using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine;

public class VarBlockTests
{
    [Fact]
    public void VarBlock_InvalidContent_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VarBlock>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Act
        var varBlock = new VarBlock(string.Empty, loggerFactoryMock.Object);

        // Assert
        loggerMock.Verify(x => x.LogError("The variable name is empty"), Times.Once);
    }

    [Fact]
    public void VarBlock_InvalidContentLength_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VarBlock>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Act
        var varBlock = new VarBlock("a", loggerFactoryMock.Object);

        // Assert
        loggerMock.Verify(x => x.LogError("The variable name is empty"), Times.Once);
    }

    [Fact]
    public void VarBlock_InvalidContentPrefix_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VarBlock>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Act
        var varBlock = new VarBlock("ab", loggerFactoryMock.Object);
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void VarBlock_InvalidName_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VarBlock>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        // Act
        var varBlock = new VarBlock("$a!", loggerFactoryMock.Object);
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
    }
}
