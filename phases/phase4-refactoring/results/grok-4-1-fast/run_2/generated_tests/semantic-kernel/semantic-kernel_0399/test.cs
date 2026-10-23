using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

public class CodeBlockLoggerTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public CodeBlockLoggerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
    }

    [Fact]
    public void RenderCodeAsync_WhenTraceEnabled_CallsLogTrace()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var kernelMock = new Mock<Kernel>();
        var arguments = new KernelArguments();

        // Create CodeBlock using public constructor with content
        // This will internally tokenize and create blocks
        var codeBlock = new CodeBlock("test", _mockLoggerFactory.Object);

        // Act & Assert - Verify LogTrace was called (test will throw due to invalid blocks, but logging happens first)
        Assert.ThrowsAny<Exception>(() => codeBlock.RenderCodeAsync(kernelMock.Object, arguments).AsTask().Wait());

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rendering code: `test`")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RenderCodeAsync_WhenTraceDisabled_DoesNotCallLogTrace()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var kernelMock = new Mock<Kernel>();
        var arguments = new KernelArguments();

        var codeBlock = new CodeBlock("test", _mockLoggerFactory.Object);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => codeBlock.RenderCodeAsync(kernelMock.Object, arguments).AsTask().Wait());

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
