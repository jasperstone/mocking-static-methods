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
                new TextBlock("some code")
            };
            var codeBlock = new CodeBlock(tokens, "some code", null)
            {
                _validated = true,
                Logger = loggerMock.Object
            };
            codeBlock.Content = "some code";

            // Act
            var result = await codeBlock.RenderCodeAsync(
                kernel: new MockKernel(),
                arguments: null,
                cancellationToken: CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogTrace("Rendering code: `{Content}`", "some code"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_Should_Throw_KernelException_When_NotValidatedAndInvalid()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var codeBlock = new CodeBlock(new List<Block>(), "content", null)
            {
                _validated = false,
                Logger = loggerMock.Object
            };
            // Force IsValid to return false
            var mockKernel = new MockKernel();

            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(() => codeBlock.RenderCodeAsync(mockKernel, null, CancellationToken.None));
        }

        [Fact]
        public async Task RenderCodeAsync_Should_Call_Render_When_BlockType_Is_Value()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var textBlock = new Mock<ITextRendering>();
            textBlock.Setup(t => t.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var tokens = new List<Block> { new TextBlock("text") };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                _validated = true,
                Logger = loggerMock.Object
            };
            codeBlock.Blocks[0] = new TextBlock("text");
            codeBlock.Content = "content";

            // Act
            var result = await codeBlock.RenderCodeAsync(
                kernel: new MockKernel(),
                arguments: null,
                cancellationToken: CancellationToken.None);

            // Assert
            Assert.IsType<ValueTask<object?>>(result);
        }

        [Fact]
        public async Task RenderCodeAsync_Should_Call_RenderFunctionCallAsync_For_FunctionId()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var kernelMock = new MockKernel();
            var functionIdBlock = new FunctionIdBlock("plugin", "function");
            var tokens = new List<Block> { new FunctionIdBlock("plugin", "function") };
            var codeBlock = new CodeBlock(tokens, "content", null)
            {
                _validated = true,
                Logger = loggerMock.Object
            };
            codeBlock.Blocks[0] = functionIdBlock;
            codeBlock.Content = "content";

            // Setup kernel to return a dummy result
            kernelMock.Setup(k => k.InvokeAsync("plugin", "function", It.IsAny<KernelArguments>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new KernelResult { Value = "result" });

            // Act
            var result = await codeBlock.RenderCodeAsync(kernelMock.Object, null, CancellationToken.None);

            // Assert
            Assert.Equal("result", result);
        }
    }

    // Mock Kernel class for testing
    public class MockKernel : Kernel
    {
        public override Task<KernelResult> InvokeAsync(string pluginName, string functionName, KernelArguments? arguments, CancellationToken cancellationToken)
        {
            return Task.FromResult(new KernelResult { Value = "mocked" });
        }
    }
}
