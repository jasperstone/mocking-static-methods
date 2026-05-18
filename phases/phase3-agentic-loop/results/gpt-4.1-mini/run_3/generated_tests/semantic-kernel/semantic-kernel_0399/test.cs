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
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenTraceEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            // Use a simple code string that tokenizes to a single Value block
            string codeContent = "simpleValue";

            var codeBlock = new CodeBlock(codeContent, loggerFactoryMock.Object);

            // Force validation to true by calling IsValid (which sets _validated)
            Assert.True(codeBlock.IsValid(out _));

            var kernelMock = new Mock<Kernel>();

            // Act
            var result = await codeBlock.RenderCodeAsync(kernelMock.Object);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Trace), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rendering code:")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
