using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests;

public class CodeBlockLoggerTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<CodeBlock>> _loggerMock;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Mock<Kernel> _kernelMock;

    public CodeBlockLoggerTests()
    {
        _loggerMock = new Mock<ILogger<CodeBlock>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(It.Is<string>(name => name == "CodeBlock"))).Returns(_loggerMock.Object);
        _loggerFactory = _loggerFactoryMock.Object;
        _kernelMock = new Mock<Kernel>();
    }

    [Fact]
    public async Task RenderCodeAsync_LogsTraceMessage_WhenTraceEnabled()
    {
        // Arrange
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var content = "test content";
        var codeBlock = new CodeBlock(content, _loggerFactory);

        // Act
        await codeBlock.RenderCodeAsync(_kernelMock.Object);

        // Assert
        _loggerMock.Verify(
            l => l.LogTrace(
                "Rendering code: `{Content}`",
                content),
            Times.Once);
    }

    [Fact]
    public async Task RenderCodeAsync_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var content = "test content";
        var codeBlock = new CodeBlock(content, _loggerFactory);

        // Act
        await codeBlock.RenderCodeAsync(_kernelMock.Object);

        // Assert
        _loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
