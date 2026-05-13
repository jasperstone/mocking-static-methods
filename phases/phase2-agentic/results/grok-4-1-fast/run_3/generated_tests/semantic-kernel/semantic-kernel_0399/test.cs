using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using Moq;
using Xunit;

public class CodeBlockTests
{
    private readonly Mock<ILogger<CodeBlock>> _mockLogger;
    private readonly ILogger<CodeBlock> _logger;
    private readonly CodeBlock _codeBlock;

    public CodeBlockTests()
    {
        _mockLogger = new Mock<ILogger<CodeBlock>>();
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        _logger = _mockLogger.Object;

        // Create a valid CodeBlock with FunctionId first block
        var blocks = new List<Block>
        {
            new FunctionIdBlock("plugin", "function"),
            new ValueBlock("test input")
        };
        _codeBlock = new CodeBlock(blocks, "test content", NullLoggerFactory.Instance)
        {
            Logger = _logger
        };
        _codeBlock._validated = true; // Set private field via reflection or assume validated
    }

    [Fact]
    public void RenderCodeAsync_WhenTraceEnabled_LogsTraceMessage()
    {
        // Arrange
        var kernel = new Mock<Kernel>().Object;
        var arguments = new KernelArguments();

        // Act
        _codeBlock.RenderCodeAsync(kernel, arguments).AsTask().Wait();

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                It.Is<LogLevel>(l => l == LogLevel.Trace),
                It.Is<EventId>(e => e.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rendering code: `{Content}`")),
                It.IsAny<Exception>(),
                It.Is<string[]>(args => args.Length == 1 && args[0] == "test content")),
            Times.Once);
    }

    [Fact]
    public void RenderCodeAsync_WhenTraceDisabled_DoesNotLogTraceMessage()
    {
        // Arrange
        _mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        var kernel = new Mock<Kernel>().Object;
        var arguments = new KernelArguments();

        // Act
        _codeBlock.RenderCodeAsync(kernel, arguments).AsTask().Wait();

        // Assert
        _mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async void RenderCodeAsync_ValidatesBlockBeforeLogging()
    {
        // Arrange - Create unvalidated block
        var unvalidatedBlocks = new List<Block>
        {
            new FunctionIdBlock("plugin", "function")
        };
        var unvalidatedCodeBlock = new CodeBlock(unvalidatedBlocks, "unvalidated content", NullLoggerFactory.Instance)
        {
            Logger = _logger
        };

        var kernel = new Mock<Kernel>().Object;
        var arguments = new KernelArguments();

        // Act & Assert
        await Assert.ThrowsAsync<KernelException>(() => unvalidatedCodeBlock.RenderCodeAsync(kernel, arguments));

        // Verify LogTrace was NOT called (validation failed first)
        _mockLogger.Verify(
            l => l.LogTrace(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()),
            Times.Never);
    }
}
