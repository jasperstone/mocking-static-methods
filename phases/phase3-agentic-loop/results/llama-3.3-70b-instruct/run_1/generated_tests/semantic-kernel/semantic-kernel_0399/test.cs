using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var kernelMock = new Mock<Kernel>();
            var codeBlock = new CodeBlock("{{functionName}}", new LoggerFactory().CreateLogger<CodeBlock>());

            // Act
            await codeBlock.RenderCodeAsync(kernelMock.Object, null, default);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Rendering code: `{{functionName}}`"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var kernelMock = new Mock<Kernel>();
            var codeBlock = new CodeBlock("{{functionName}}", new LoggerFactory().CreateLogger<CodeBlock>());

            // Act
            await codeBlock.RenderCodeAsync(kernelMock.Object, null, default);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
