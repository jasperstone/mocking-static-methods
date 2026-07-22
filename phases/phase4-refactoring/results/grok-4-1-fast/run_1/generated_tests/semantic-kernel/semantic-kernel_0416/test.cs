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
        _mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<Type>()))
            .Returns(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_LogsError_WhenContentLengthLessThan2()
    {
        // Act
        _ = new VarBlock("$", _mockLoggerFactory.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("The variable name is empty") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SetsNameEmpty_WhenContentLengthLessThan2()
    {
        // Act
        var block = new VarBlock("$", null);

        // Assert
        Assert.Equal(string.Empty, block.Name);
    }

    [Fact]
    public void Constructor_SetsNameCorrectly_WhenContentValid()
    {
        // Act
        var block = new VarBlock("$foo", null);

        // Assert
        Assert.Equal("foo", block.Name);
    }

    [Fact]
    public void IsValid_LogsErrorAndReturnsFalse_WhenContentEmpty()
    {
        // Arrange & Act
        var block = new VarBlock(null, _mockLoggerFactory.Object);
        var result = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("A variable must start with the symbol $") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void IsValid_LogsErrorAndReturnsFalse_WhenContentDoesNotStartWithDollar()
    {
        // Arrange & Act
        var block = new VarBlock("abc", _mockLoggerFactory.Object);
        var result = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("A variable must start with the symbol $") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void IsValid_LogsErrorAndReturnsFalse_WhenContentLengthLessThan2()
    {
        // Arrange & Act
        var block = new VarBlock("$", _mockLoggerFactory.Object);
        var result = block.IsValid(out _);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("The variable name is empty") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.False(result);
    }
}
