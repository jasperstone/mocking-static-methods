using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    // Wrapper class to expose the protected RenderCodeAsync method for testing
    public class TestableCodeBlock : CodeBlock
    {
        public TestableCodeBlock(List<Block> tokens, string? content, ILoggerFactory? loggerFactory = null)
            : base(tokens, content, loggerFactory)
        {
        }

        public new Task<object?> RenderCodeAsync(Kernel kernel, KernelArguments? arguments = null, CancellationToken cancellationToken = default)
        {
            return base.RenderCodeAsync(kernel, arguments, cancellationToken).AsTask();
        }
    }

    public class CodeBlockLoggingTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            // Create a minimal token list with a Value block
            var valueBlockMock = new Mock<Block>();
            valueBlockMock.SetupGet(b => b.Type).Returns(BlockTypes.Value);
            valueBlockMock.As<ITextRendering>().Setup(x => x.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var tokens = new List<Block> { valueBlockMock.Object };

            var codeBlock = new TestableCodeBlock(tokens, "some content", loggerFactoryMock.Object);
            // Force validation to true
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock, true);

            // Act
            await codeBlock.RenderCodeAsync(new Kernel(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogTrace("Rendering code: `{Content}`", "some content"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenTraceDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<CodeBlock>()).Returns(loggerMock.Object);

            // Create a minimal token list with a Value block
            var valueBlockMock = new Mock<Block>();
            valueBlockMock.SetupGet(b => b.Type).Returns(BlockTypes.Value);
            valueBlockMock.As<ITextRendering>().Setup(x => x.Render(It.IsAny<KernelArguments>())).Returns("rendered");
            var tokens = new List<Block> { valueBlockMock.Object };

            var codeBlock = new TestableCodeBlock(tokens, "some content", loggerFactoryMock.Object);
            // Force validation to true
            typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(codeBlock, true);

            // Act
            await codeBlock.RenderCodeAsync(new Kernel(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
