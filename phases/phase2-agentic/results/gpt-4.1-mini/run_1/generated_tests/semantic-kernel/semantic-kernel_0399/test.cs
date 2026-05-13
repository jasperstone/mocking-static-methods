using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace SemanticKernel.Core.TemplateEngine.Blocks.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();

            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Create a Block of type Value that implements ITextRendering
            var textRenderingBlockMock = new Mock<Block>("value", loggerFactoryMock.Object) { CallBase = true };
            textRenderingBlockMock.SetupGet(b => b.Type).Returns(BlockTypes.Value);
            textRenderingBlockMock.As<ITextRendering>().Setup(tr => tr.Render(It.IsAny<KernelArguments>())).Returns("rendered");

            var blocks = new System.Collections.Generic.List<Block> { textRenderingBlockMock.Object };
            var codeBlock = new CodeBlock(blocks, "some content", loggerFactoryMock.Object);

            // Mark as validated to skip IsValid call
            var isValidField = typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            isValidField.SetValue(codeBlock, true);

            var kernelMock = new Mock<Kernel>();

            // Act
            var result = await codeBlock.RenderCodeAsync(kernelMock.Object);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.Once);
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code:")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);

            Assert.Equal("rendered", result);
        }
    }
}
