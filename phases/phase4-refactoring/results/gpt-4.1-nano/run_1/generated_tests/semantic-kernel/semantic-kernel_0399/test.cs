using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Exceptions;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            var content = "some content";
            var codeBlock = new CodeBlock(content, loggerFactoryMock.Object);
            var textBlock = new Mock<ITextRendering>();
            textBlock.Setup(t => t.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var blockList = new List<Block> { textBlock.Object };
            var codeBlock2 = new CodeBlock(blockList, content, loggerFactoryMock.Object);
            // Force validation
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock2, true);

            // Act
            await codeBlock2.RenderCodeAsync(new Kernel());

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RenderCodeAsync_ThrowsKernelException_WhenNotValidatedAndInvalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            var codeBlock = new CodeBlock(null, loggerFactoryMock.Object);
            // Force invalid state
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock, false);
            // Make IsValid return false by setting Content to null
            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(async () => await codeBlock.RenderCodeAsync(new Kernel()));
        }

        [Fact]
        public async Task RenderCodeAsync_ReturnsValue_WhenBlockTypeIsValue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            var content = "dummy";
            var textBlockMock = new Mock<ITextRendering>();
            textBlockMock.Setup(t => t.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var blockList = new List<Block> { textBlockMock.Object };
            var codeBlock = new CodeBlock(blockList, content, loggerFactoryMock.Object);
            // Force validation
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock, true);

            // Act
            var result = await codeBlock.RenderCodeAsync(new Kernel());

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RenderCodeAsync_ThrowsUnexpectedTokenType()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            var content = "dummy";
            var codeBlock = new CodeBlock(content, loggerFactoryMock.Object);
            var unknownBlockMock = new Mock<Block>();
            unknownBlockMock.SetupGet(b => b.Type).Returns((BlockTypes)999);
            var blockList = new List<Block> { unknownBlockMock.Object };
            var codeBlock2 = new CodeBlock(blockList, content, loggerFactoryMock.Object);
            // Force validation
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock2, true);

            // Act & Assert
            await Assert.ThrowsAsync<KernelException>(async () => await codeBlock2.RenderCodeAsync(new Kernel()));
        }
    }
}
