using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.TemplateEngine.Tests;

public class CodeBlockTests
{
    private readonly Mock<ILogger<CodeBlock>> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly ILogger<CodeBlock> _logger;

    public CodeBlockTests()
    {
        _mockLogger = new Mock<ILogger<CodeBlock>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory.Setup(f => f.CreateLogger(typeof(CodeBlock).FullName!)).Returns(_mockLogger.Object);
        _logger = _mockLogger.Object;
    }

    [Fact]
    public void RenderCodeAsync_LogsTrace_WhenTraceEnabled()
    {
        // Arrange
        var content = "test content";
        var mockBlock = new Mock<Block>();
        mockBlock.Setup(b => b.Type).Returns(BlockTypes.Value);
        mockBlock.As<ITextRendering>().Setup(r => r.Render(It.IsAny<KernelArguments?>()))
            .Returns((KernelArguments? args) => "rendered value");

        var blocks = new List<Block> { mockBlock.Object };
        var codeBlock = new CodeBlock(blocks, content, _mockLoggerFactory.Object);

        // Enable trace logging
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        var mockKernel = new Mock<Kernel>();
        var arguments = new KernelArguments();

        // Act
        codeBlock.RenderCodeAsync(mockKernel.Object, arguments).AsTask().Wait();

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                It.Is<LogLevel>(level => level == LogLevel.Trace),
                It.Is<EventId>(id => id.Id == 0),
                It.Is<ReadOnlySpan<char>>(s => s.SequenceEqual("Rendering code: `{Content}`".AsSpan())),
                It.IsAny<Exception>(),
                It.Is<Args>(args => args.Length == 1 && args[0].ToString() == content)),
            Times.Once);
    }

    [Fact]
    public void RenderCodeAsync_DoesNotLogTrace_WhenTraceDisabled()
    {
        // Arrange
        var content = "test content";
        var mockBlock = new Mock<Block>();
        mockBlock.Setup(b => b.Type).Returns(BlockTypes.Value);
        mockBlock.As<ITextRendering>().Setup(r => r.Render(It.IsAny<KernelArguments?>()))
            .Returns((KernelArguments? args) => "rendered value");

        var blocks = new List<Block> { mockBlock.Object };
        var codeBlock = new CodeBlock(blocks, content, _mockLoggerFactory.Object);

        // Disable trace logging
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var mockKernel = new Mock<Kernel>();
        var arguments = new KernelArguments();

        // Act
        codeBlock.RenderCodeAsync(mockKernel.Object, arguments).AsTask().Wait();

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<ReadOnlySpan<char>>(),
                It.IsAny<Exception>(),
                It.IsAny<Args>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_UsesNullLogger_WhenLoggerFactoryNull()
    {
        // Arrange & Act
        var codeBlock = new CodeBlock("test", null);

        // Assert - No exception thrown, uses NullLogger
        Assert.NotNull(codeBlock);
    }
}
