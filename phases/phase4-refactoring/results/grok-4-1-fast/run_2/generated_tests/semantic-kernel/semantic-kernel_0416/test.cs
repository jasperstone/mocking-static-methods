using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests;

public class VarBlockLoggingTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger> _mockLogger;

    public VarBlockLoggingTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_NullContent_LogsErrorMessage()
    {
        // Act
        var block = new Microsoft.SemanticKernel.TemplateEngine.Blocks.VarBlock(null, _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "The variable name is empty"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_EmptyContent_LogsErrorMessage()
    {
        // Act
        var block = new Microsoft.SemanticKernel.TemplateEngine.Blocks.VarBlock("", _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "The variable name is empty"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SingleDollarContent_LogsErrorMessage()
    {
        // Act
        var block = new Microsoft.SemanticKernel.TemplateEngine.Blocks.VarBlock("$", _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "The variable name is empty"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ValidContent_DoesNotLogError()
    {
        // Act
        var block = new Microsoft.SemanticKernel.TemplateEngine.Blocks.VarBlock("$valid", _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
