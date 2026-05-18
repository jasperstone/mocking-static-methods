using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using System;

namespace Microsoft.SemanticKernel.TemplateEngine.Blocks;

public class VarBlockTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger> _loggerMock;

    public VarBlockTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_LogsError_WhenContentLengthLessThan2()
    {
        // Arrange
        string shortContent = "$";

        // Act
        _ = new VarBlock(shortContent, _loggerFactoryMock.Object);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v!).Contains("The variable name is empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotLogError_WhenContentLength2OrMore()
    {
        // Arrange
        string validContent = "$var";

        // Act
        _ = new VarBlock(validContent, _loggerFactoryMock.Object);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
