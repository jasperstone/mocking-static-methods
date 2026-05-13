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
        public void RenderCodeAsync_Should_LogTrace_When_LoggerIsEnabledAndContentIsValid()
        {
            // Arrange
            var mockLogger = CreateLogger();
            var tokens = new List<Block>
            {
                new TextBlock("some code")
            };
            var codeBlock = new CodeBlock(tokens, "some code", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            codeBlock.GetType().GetProperty("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(codeBlock, true);

            var kernelMock = new Mock<Kernel>();
            var arguments = new KernelArguments();

            // Act
            var task = codeBlock.RenderCodeAsync(kernelMock.Object, arguments);
            var result = task.AsTask().Result;

            // Assert
            mockLogger.Verify(x => x.LogTrace(It.Is<string>(s => s.Contains("Rendering code")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void RenderCodeAsync_Should_ThrowKernelException_When_NotValidatedAndIsValidFails()
        {
            // Arrange
            var mockLogger = CreateLogger();
            var tokens = new List<Block>
            {
                new TextBlock("some code")
            };
            var codeBlock = new CodeBlock(tokens, "some code", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            // Force _validated to false and IsValid to return false
            var isValidMethod = typeof(CodeBlock).GetMethod("IsValid", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // Use reflection to set _validated to false
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(codeBlock, false);
            // Mock IsValid to return false
            // Since IsValid is not virtual, we can't mock it directly, so we simulate by setting _validated to false

            // Act & Assert
            Assert.ThrowsAsync<KernelException>(async () => await codeBlock.RenderCodeAsync(new Kernel()));
        }

        [Fact]
        public void RenderCodeAsync_Should_Throw_When_FirstBlockTypeIsUnexpected()
        {
            // Arrange
            var mockLogger = CreateLogger();
            var tokens = new List<Block>
            {
                new TextBlock("unexpected")
            };
            var codeBlock = new CodeBlock(tokens, "unexpected", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            // Set _validated to true to skip validation
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(codeBlock, true);

            // Force Blocks[0] to have an unexpected type
            var block = codeBlock.Blocks[0];
            // Use reflection to set type to an invalid enum value
            typeof(Block).GetProperty("Type").SetValue(block, (BlockTypes)999);

            // Act & Assert
            Assert.Throws<KernelException>(async () => await codeBlock.RenderCodeAsync(new Kernel()));
        }

        [Fact]
        public void RenderCodeAsync_Should_CallRender_When_BlockTypeIsValueOrVariable()
        {
            // Arrange
            var mockLogger = CreateLogger();
            var textBlock = new Mock<ITextRendering>();
            textBlock.Setup(t => t.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var tokens = new List<Block>
            {
                new TextBlock("value")
            };
            var codeBlock = new CodeBlock(tokens, "value", null);
            codeBlock.GetType().GetProperty("Logger").SetValue(codeBlock, mockLogger.Object);
            // Set _validated to true
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(codeBlock, true);
            // Force first block to be Value type
            var block = codeBlock.Blocks[0];
            typeof(Block).GetProperty("Type").SetValue(block, BlockTypes.Value);
            // Cast to ITextRendering
            var textRendering = block as ITextRendering;
            // Use reflection to set the Render method
            // Instead, we can replace the block with our mock
            var mockTextBlock = new Mock<Block>();
            mockTextBlock.Setup(b => b.Type).Returns(BlockTypes.Value);
            mockTextBlock.As<ITextRendering>().Setup(t => t.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            codeBlock.Blocks[0] = mockTextBlock.Object;

            // Act
            var resultTask = codeBlock.RenderCodeAsync(new Kernel());
            var result = resultTask.AsTask().Result;

            // Assert
            Assert.Equal("rendered", result);
        }
    }
}
