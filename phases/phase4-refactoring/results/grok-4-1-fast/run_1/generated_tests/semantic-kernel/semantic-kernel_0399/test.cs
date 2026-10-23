using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

public class CodeBlockLoggerExtensionTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public CodeBlockLoggerExtensionTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
    }

    [Fact]
    public void LoggerExtensions_LogTrace_CalledWithCorrectParameters()
    {
        // Arrange
        var logger = _mockLogger.Object;
        const string expectedContent = "test content";
        
        // Act
        logger.LogTrace("Rendering code: `{Content}`", expectedContent);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.Is<EventId>(e => e.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rendering code: `{Content}`")),
                It.IsAny<Exception>(),
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == expectedContent)
            ),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogTrace_WhenTraceDisabled_IsEnabledReturnsFalse()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var logger = _mockLogger.Object;
        const string expectedContent = "test content";

        // Act
        logger.LogTrace("Rendering code: `{Content}`", expectedContent);

        // Assert - No Log method call when trace is disabled
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()
            ),
            Times.Never);
    }

    [Fact]
    public void LoggerExtensions_LogTrace_WhenTraceEnabled_CallsUnderlyingLog()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var logger = _mockLogger.Object;
        const string expectedContent = "test content";

        // Act
        logger.LogTrace("Rendering code: `{Content}`", expectedContent);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.Is<EventId>(e => e.Id == 0),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == expectedContent)
            ),
            Times.Once);
    }
}
