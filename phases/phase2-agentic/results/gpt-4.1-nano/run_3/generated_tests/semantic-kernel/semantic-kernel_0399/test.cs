using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    public class CodeBlockTests
    {
        private Mock<ILogger<CodeBlock>> CreateLoggerMock()
        {
            var mockLogger = new Mock<ILogger<CodeBlock>>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object?>()))
                      .Verifiable();
            mockLogger.Setup(x => x.LogError(It.IsAny<string>()))
                      .Verifiable();
            mockLogger.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?>()))
                      .Verifiable();
            return mockLogger;
        }

        [Fact]
        public async Task RenderCodeAsync_Should_LogTrace_When_TraceEnabled()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var tokens = new List<Block>
            {
                new TextBlock("dummy")
            };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                Logger = loggerMock.Object,
                _validated = true
            };
            codeBlock.Content = "some content";

            // Act
            var result = await codeBlock.RenderCodeAsync(
                kernel: new MockKernel(),
                arguments: null,
                cancellationToken: CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogTrace("Rendering code: `{Content}`", "content"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_Should_Throw_KernelException_When_Not_Validated()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var tokens = new List<Block>
            {
                new TextBlock("dummy")
            };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                Logger = loggerMock.Object,
                _validated = false
            };
            codeBlock.Content = "some content";

            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(async () =>
                await codeBlock.RenderCodeAsync(
                    kernel: new MockKernel(),
                    arguments: null,
                    cancellationToken: CancellationToken.None));
        }

        [Fact]
        public async Task RenderCodeAsync_Should_Call_RenderFunctionCallAsync_For_FunctionId()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var mockKernel = new MockKernel();
            var functionIdBlock = new FunctionIdBlock
            {
                PluginName = "plugin",
                FunctionName = "func"
            };
            var tokens = new List<Block>
            {
                functionIdBlock
            };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                Logger = loggerMock.Object,
                _validated = true
            };
            var called = false;
            codeBlock.RenderFunctionCallAsync = (f, k, args, token) =>
            {
                called = true;
                return new ValueTask<object?>("result");
            };

            // Act
            var result = await codeBlock.RenderCodeAsync(
                kernel: mockKernel,
                arguments: null,
                cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal("result", result);
            Assert.True(called);
        }

        [Fact]
        public void LogTrace_Should_NotLog_When_LogLevel_IsNotEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CodeBlock>>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var tokens = new List<Block>
            {
                new TextBlock("dummy")
            };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                Logger = mockLogger.Object,
                _validated = true
            };
            codeBlock.Content = "some content";

            // Act
            var task = codeBlock.RenderCodeAsync(
                kernel: new MockKernel(),
                arguments: null,
                cancellationToken: CancellationToken.None);

            // Assert
            mockLogger.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
        }
    }

    // Mock implementations for Kernel and related classes
    public class MockKernel : Kernel
    {
        public override ValueTask<KernelResult> InvokeAsync(string pluginName, string functionName, KernelArguments? arguments, CancellationToken cancellationToken)
        {
            return new ValueTask<KernelResult>(new KernelResult { Value = "mocked result" });
        }
    }

    public class KernelResult
    {
        public object? Value { get; set; }
    }

    // Dummy Block classes for testing
    public class TextBlock : Block, ITextRendering
    {
        private readonly string _text;
        public TextBlock(string text)
        {
            _text = text;
        }
        public override bool IsValid(out string errorMsg)
        {
            errorMsg = "";
            return true;
        }
        public object? Render(KernelArguments? args) => _text;
        public override BlockTypes Type => BlockTypes.Value;
        public override string Content => _text;
    }

    public class FunctionIdBlock : Block
    {
        public string PluginName { get; set; } = "";
        public string FunctionName { get; set; } = "";
        public override bool IsValid(out string errorMsg)
        {
            errorMsg = "";
            return true;
        }
        public override BlockTypes Type => BlockTypes.FunctionId;
        public override string Content => $"{PluginName}.{FunctionName}";
    }
}
