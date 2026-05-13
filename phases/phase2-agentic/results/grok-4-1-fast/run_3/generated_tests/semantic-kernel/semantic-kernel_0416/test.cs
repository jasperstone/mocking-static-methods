using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.TemplateEngine.Tests;

public class VarBlockTests
{
    private readonly Mock<ILogger<VarBlock>> _mockLogger;

    public VarBlockTests()
    {
        _mockLogger = new Mock<ILogger<VarBlock>>();
        _mockLogger.SetupAllProperties();
    }

    [Fact]
    public void Constructor_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        string shortContent = "$";

        // Act
        var block = new VarBlock(shortContent, loggerFactory);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_SetsNameCorrectly_WhenContentValid()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        string validContent = "$validName";

        // Act
        var block = new VarBlock(validContent, loggerFactory);

        // Assert
        Assert.Equal("validName", block.Name);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyFormat>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentNullOrEmpty()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));

        // Act
        var block = new VarBlock(null, loggerFactory);
        bool isValid = block.IsValid(out _);

        // Assert
        Assert.False(isValid);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString()!.Contains("A variable must start with the symbol $")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentDoesNotStartWithVarPrefix()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        var block = new VarBlock("invalid", loggerFactory);

        // Act
        bool isValid = block.IsValid(out _);

        // Assert
        Assert.False(isValid);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString()!.Contains("A variable must start with the symbol $")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void IsValid_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        var block = new VarBlock("$", loggerFactory);

        // Act
        bool isValid = block.IsValid(out _);

        // Assert
        Assert.False(isValid);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenContentValid()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new MockLoggerProvider(_mockLogger.Object)));
        var block = new VarBlock("$valid", loggerFactory);

        // Act
        bool isValid = block.IsValid(out string errorMsg);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMsg);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyFormat>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()), Times.Never);
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
