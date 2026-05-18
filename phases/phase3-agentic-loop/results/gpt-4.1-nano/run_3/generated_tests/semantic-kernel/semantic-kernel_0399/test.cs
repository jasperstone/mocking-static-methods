using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    // Dummy implementations to bypass internal accessibility
    internal class DummyBlock : Block
    {
        public override bool IsValid(out string errorMsg)
        {
            errorMsg = "";
            return true;
        }
    }

    internal class DummyTextRendering : ITextRendering
    {
        private readonly object _result;
        public DummyTextRendering(object result) => _result = result;
        public object Render(KernelArguments? args) => _result;
    }

    public class CodeBlockTests
    {
        private static CodeBlock CreateCodeBlockWithFunctionId(ILoggerFactory? loggerFactory = null)
        {
            var blocks = new System.Collections.Generic.List<Block>
            {
                new FunctionIdBlock { PluginName = "p", FunctionName = "f" }
            };
            var cb = new CodeBlock(blocks, "content", loggerFactory);
            cb._validated = true;
            return cb;
        }

        [Fact]
        public async Task RenderCodeAsync_ShouldLogTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = CreateCodeBlockWithFunctionId(loggerFactoryMock.Object);

            // Act
            await codeBlock.RenderCodeAsync(new Kernel());

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void RenderCodeAsync_ShouldThrow_WhenNotValidatedAndInvalid()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var codeBlock = new CodeBlock("content", loggerFactoryMock.Object);
            codeBlock._validated = false;

            // Act & Assert
            Assert.ThrowsAsync<KernelException>(async () => await codeBlock.RenderCodeAsync(new Kernel()));
        }

        [Fact]
        public async Task RenderCodeAsync_ShouldCallRender_WhenTypeIsValueOrVariable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var blocks = new System.Collections.Generic.List<Block>
            {
                new DummyTextRendering("rendered")
            };
            var codeBlock = new CodeBlock(blocks, "content", loggerFactoryMock.Object);
            codeBlock._validated = true;

            // Act
            var result = await codeBlock.RenderCodeAsync(new Kernel());

            // Assert
            Assert.Equal("rendered", result);
        }

        [Fact]
        public async Task RenderCodeAsync_ShouldCallInvokeAsync_WhenTypeIsFunctionId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.InvokeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KernelArguments>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KernelResult { Value = "result" });

            var blocks = new System.Collections.Generic.List<Block>
            {
                new FunctionIdBlock { PluginName = "p", FunctionName = "f" }
            };
            var codeBlock = new CodeBlock(blocks, "content", loggerFactoryMock.Object);
            codeBlock._validated = true;

            // Act
            var result = await codeBlock.RenderCodeAsync(kernelMock.Object);

            // Assert
            Assert.Equal("result", result);
        }
    }
}
