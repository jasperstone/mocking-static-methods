using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests;

public class VarBlockTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger<VarBlock>> _mockLogger;

    public VarBlockTests()
    {
        _mockLogger = new Mock<ILogger<VarBlock>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(f => f.CreateLogger<VarBlock>()).Returns(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        string shortContent = "$";

        // Act
        _ = new VarBlock(shortContent, _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SetsNameEmpty_WhenContentLengthLessThan2()
    {
        // Arrange
        string shortContent = "$";

        // Act
        var block = new VarBlock(shortContent, _mockLoggerFactory.Object);

        // Assert
        Assert.Equal(string.Empty, block.Name);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentEmpty()
    {
        // Arrange
        var block = new VarBlock(null, _mockLoggerFactory.Object);

        // Act
        _ = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("A variable must start with the symbol $ and have a name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentDoesNotStartWithDollar()
    {
        // Arrange
        var block = new VarBlock("abc", _mockLoggerFactory.Object);

        // Act
        _ = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("A variable must start with the symbol $")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        var block = new VarBlock("$", _mockLoggerFactory.Object);

        // Act
        _ = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
