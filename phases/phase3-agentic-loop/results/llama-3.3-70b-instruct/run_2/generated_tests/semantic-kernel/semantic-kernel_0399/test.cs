using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel.TemplateEngine;

namespace SemanticKernel.Core.TemplateEngine.Blocks
{
    public class CodeBlockTests
    {
        [Fact]
        public async Task RenderCodeAsync_LogsTrace_WhenEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var codeBlock = new CodeBlock("TestContent", loggerFactoryMock.Object);

            // Act
            await codeBlock.RenderCodeAsync(null, null, default);

            // Assert
            loggerMock.Verify(l => l.LogTrace("Rendering code: `{Content}`", "TestContent"), Times.Once);
        }

        [Fact]
        public async Task RenderCodeAsync_DoesNotLogTrace_WhenDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var codeBlock = new CodeBlock("TestContent", loggerFactoryMock.Object);

            // Act
            await codeBlock.RenderCodeAsync(null, null, default);

            // Assert
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
