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
        private Mock<ILogger<CodeBlock>> CreateLogger()
        {
            var mockLogger = new Mock<ILogger<CodeBlock>>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            return mockLogger;
        }

        [Fact]
        public async Task RenderCodeAsync_Should_LogTrace_When_TraceEnabled()
        {
            // Arrange
            var loggerMock = CreateLogger();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            var content = "some content";
            var codeBlock = new CodeBlock(content, loggerFactoryMock.Object);
            codeBlock.IsValid(out _); // Validate to set _validated to true

            // Setup Blocks with a FunctionId type for testing
            var functionIdBlock = new FunctionIdBlock
            {
                PluginName = "TestPlugin",
                FunctionName = "TestFunction"
            };
            var blocks = new List<Block> { functionIdBlock };
            var codeBlockWithBlocks = new CodeBlock(blocks, content, loggerFactoryMock.Object);
            codeBlockWithBlocks.IsValid(out _); // Validate to set _validated to true

            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.InvokeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<KernelArguments>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new KernelResult { Value = "result" });

            // Act
            await codeBlockWithBlocks.RenderCodeAsync(kernelMock.Object);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }

    // Dummy implementations for missing types
    internal class FunctionIdBlock : Block
    {
        public string PluginName { get; set; }
        public string FunctionName { get; set; }
        public override BlockTypes Type => BlockTypes.FunctionId;
        public override bool IsValid(out string errorMsg) { errorMsg = ""; return true; }
        public string Content => $"{PluginName}.{FunctionName}";
    }
}
