using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.TemplateEngine.Tests
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTraceMessage_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<CodeBlock>>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var codeBlock = new CodeBlock("some content", loggerFactoryMock.Object);
            codeBlock.Blocks.Add(new Mock<Block>().Object); // Add a block to satisfy the switch case

            // Act
            await codeBlock.RenderCodeAsync(null, null, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogTrace("Rendering code: `{Content}`", It.Is<string>(s => s == "some content")),
                Times.Once);
        }
    }
}
