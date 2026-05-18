using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.TemplateEngine.Blocks.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // We cannot instantiate CodeBlock or Block directly because they are internal,
            // so we test the logging by creating a CodeBlock via its public constructor with a string content
            // that will be tokenized into blocks. We use a simple content that results in a single Value block.

            var codeBlock = new CodeBlock("simple text", loggerFactoryMock.Object);

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
        }
    }
}
