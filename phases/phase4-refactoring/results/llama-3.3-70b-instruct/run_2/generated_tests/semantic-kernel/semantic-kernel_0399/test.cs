using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var codeBlock = new CodeBlock("{{MyPlugin.MyFunction}}", loggerMock.Object);

            // Act
            await codeBlock.RenderCodeAsync(Mock.Of<Kernel>(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Rendering code: `{{MyPlugin.MyFunction}}`"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var codeBlock = new CodeBlock("{{MyPlugin.MyFunction}}", loggerMock.Object);

            // Act
            await codeBlock.RenderCodeAsync(Mock.Of<Kernel>(), null, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
