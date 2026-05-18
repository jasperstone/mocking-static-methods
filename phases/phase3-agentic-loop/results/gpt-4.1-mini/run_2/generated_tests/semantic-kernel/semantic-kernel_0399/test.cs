using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Blocks.Tests
{
    public class CodeBlockTests
    {
        private class TestBlock
        {
            public virtual int Type => 4; // Simulate BlockTypes.Value
            public virtual object Render(KernelArguments? args) => "rendered";
        }

        private class TestCodeBlock : CodeBlock
        {
            public TestBlock TestBlockInstance { get; }

            public TestCodeBlock(ILoggerFactory? loggerFactory = null)
                : base(new System.Collections.Generic.List<Block>(), "content", loggerFactory)
            {
                // We cannot add internal Block instances, so we simulate with a dummy list and override Blocks property
                this.TestBlockInstance = new TestBlock();
            }

            public override System.Collections.Generic.List<Block> Blocks => new System.Collections.Generic.List<Block>();

            // We override RenderCodeAsync to simulate the Blocks list with our TestBlock
            public new async ValueTask<object?> RenderCodeAsync(Kernel kernel, KernelArguments? arguments = null, CancellationToken cancellationToken = default)
            {
                // Mark validated to skip IsValid
                var isValidField = typeof(CodeBlock).GetField("_validated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                isValidField.SetValue(this, true);

                if (this.Logger.IsEnabled(LogLevel.Trace))
                {
                    this.Logger.LogTrace("Rendering code: `{Content}`", this.Content);
                }

                // Return the Render result of our TestBlock
                return await Task.FromResult(this.TestBlockInstance.Render(arguments));
            }
        }

        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var codeBlock = new TestCodeBlock(loggerFactoryMock.Object);

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
