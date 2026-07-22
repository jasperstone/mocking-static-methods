using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;

namespace SemanticKernel.Core.TemplateEngine
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var codeBlock = new Microsoft.SemanticKernel.TemplateEngine.Blocks.CodeBlock("{{MyFunction}}", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await codeBlock.RenderCodeAsync(Mock.Of<Microsoft.SemanticKernel.Kernel>(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Rendering code: `{Content}`", codeBlock.Content), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var codeBlock = new Microsoft.SemanticKernel.TemplateEngine.Blocks.CodeBlock("{{MyFunction}}", new LoggerFactory().AddMock(loggerMock.Object));

            // Act
            await codeBlock.RenderCodeAsync(Mock.Of<Microsoft.SemanticKernel.Kernel>(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
