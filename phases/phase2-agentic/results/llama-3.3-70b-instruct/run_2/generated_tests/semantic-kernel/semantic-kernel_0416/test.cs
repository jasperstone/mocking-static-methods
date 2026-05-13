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
        var loggerMock = new Mock<ILogger>();
        var varBlock = new VarBlock(string.Empty, new LoggerFactory().AddMock(loggerMock.Object));

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError("A variable must start with the symbol {0} and have a name"), Times.Once);
    }

    [Fact]
    public void VarBlock_EmptyName_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var varBlock = new VarBlock(Symbols.VarPrefix.ToString(), new LoggerFactory().AddMock(loggerMock.Object));

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError("The variable name is empty"), Times.Once);
    }

    [Fact]
    public void VarBlock_InvalidName_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var varBlock = new VarBlock(Symbols.VarPrefix + "InvalidName!", new LoggerFactory().AddMock(loggerMock.Object));

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void VarBlock_ValidName_DoesNotLogError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var varBlock = new VarBlock(Symbols.VarPrefix + "ValidName", new LoggerFactory().AddMock(loggerMock.Object));

        // Act
        varBlock.IsValid(out _);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
    }
}

public static class LoggerFactoryExtensions
{
    public static ILoggerFactory AddMock(this ILoggerFactory loggerFactory, ILogger logger)
    {
        loggerFactory.AddProvider(new MockLoggerProvider(logger));
        return loggerFactory;
    }
}

public class MockLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public MockLoggerProvider(ILogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
    }
}
