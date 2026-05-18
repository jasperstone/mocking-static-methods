using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    public class CodeBlockTests
    {
        private class DummyKernel : Kernel
        {
            public Func<string, string, KernelArguments?, CancellationToken, Task<KernelResult>> InvokeAsyncFunc;

            public override Task<KernelResult> InvokeAsync(string pluginName, string functionName, KernelArguments? arguments, CancellationToken cancellationToken)
            {
                return InvokeAsyncFunc(pluginName, functionName, arguments, cancellationToken);
            }
        }

        [Fact]
        public async Task RenderCodeAsync_Should_LogTrace_When_Enabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()));

            var tokens = new System.Collections.Generic.List<Block>
            {
                new Mock<Block>().Object
            };
            var codeBlock = new CodeBlock(tokens, "content", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);

            // Act
            await codeBlock.RenderCodeAsync(new DummyKernel(), null);

            // Assert
            mockLogger.Verify(x => x.LogTrace("Rendering code: `{Content}`", "content"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_Should_CallRenderFunction_When_BlockTypeIsFunctionId()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var mockKernel = new Mock<Kernel>();
            var functionIdBlock = new Mock<FunctionIdBlock>();
            var tokens = new System.Collections.Generic.List<Block> { functionIdBlock.Object };
            var codeBlock = new CodeBlock(tokens, "content", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            codeBlock.GetType().GetProperty("Blocks").SetValue(codeBlock, tokens);
            // Force validation
            codeBlock.GetType().GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock, true);

            var kernelResult = new KernelResult { Value = "result" };
            mockKernel.Setup(k => k.InvokeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KernelArguments>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kernelResult);

            // Act
            var result = await codeBlock.RenderCodeAsync(mockKernel.Object, null);

            // Assert
            Assert.Equal("result", result);
        }

        [Fact]
        public async Task RenderFunctionCallAsync_Should_InvokeKernelAndReturnResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var fBlock = new FunctionIdBlock
            {
                PluginName = "plugin",
                FunctionName = "func"
            };
            var kernelResult = new KernelResult { Value = "output" };
            mockKernel.Setup(k => k.InvokeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KernelArguments>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kernelResult);

            var codeBlock = new CodeBlock(new System.Collections.Generic.List<Block>(), "content", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);

            // Act
            var result = await codeBlock.RenderFunctionCallAsync(fBlock, mockKernel.Object, null, CancellationToken.None);

            // Assert
            Assert.Equal("output", result);
            mockKernel.Verify(k => k.InvokeAsync("plugin", "func", It.IsAny<KernelArguments>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public void IsValid_Should_ReturnFalse_When_NamedArgInFirstPosition()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var tokens = new System.Collections.Generic.List<Block>
            {
                new Mock<Block>().Object,
                new Mock<Block>().Object
            };
            var codeBlock = new CodeBlock(tokens, "content", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            // Force first block to be NamedArg
            tokens[0] = new Mock<Block>();
            (tokens[0] as Mock<Block>).SetupGet(b => b.Type).Returns(BlockTypes.NamedArg);

            // Act
            var result = codeBlock.IsValid(out var errorMsg);

            // Assert
            Assert.False(result);
            Assert.Contains("Unexpected named argument", errorMsg);
        }
    }
}
